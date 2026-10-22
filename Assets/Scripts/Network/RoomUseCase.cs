using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Network.V2;
using Grpc.Core;

/// <summary>Call on Unity's main thread. Owns room reception until Disconnect/Dispose.
/// Changes are committed to Data before notification. Never performs rendering or navigation.</summary>
public sealed class RoomUseCase : IDisposable
{
    private readonly LobbyConnector connector;
    private readonly string playerId;
    private readonly CancellationTokenSource lifetime;
    private CancellationTokenSource watching;
    private readonly HashSet<string> announcedMatches = new();
    private int generation;
    private bool busy, disposed;
    private uint roomId;
    public RoomData Data { get; }
    public event Action<Status> Failed;
    // Consumers decide how to display/select/watch; this is not a scene transition.
    public event Action<string, LobbyPhase> MatchAvailable;

    public RoomUseCase(LobbyConnector connector, RoomData data, string playerId, CancellationToken ct = default)
    {
        this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        Data = data != null ? data : throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrEmpty(playerId)) throw new ArgumentException("Login before configuring the room use case.", nameof(playerId));
        this.playerId = playerId;
        lifetime = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Data.ResetOnline(); Data.SetBusy(false);
    }

    public UniTask Create(string name, CancellationToken ct = default) => Enter(token => connector.Create(name, token), ct);
    public UniTask Join(uint id, CancellationToken ct = default) => Enter(token => connector.Join(id, token), ct);
    public UniTask Resume(uint id, CancellationToken ct = default) => Enter(token => connector.Get(id, token), ct);

    private async UniTask Enter(Func<CancellationToken, Task<RoomSnapshot>> request, CancellationToken ct)
    {
        BeginOperation();
        int current = generation;
        try
        {
            // Do not silently leave a server-side membership when switching rooms.
            if (roomId != 0) throw new InvalidOperationException("Leave or disconnect the current room first.");
            Data.SetConnection(RoomConnectionState.Connecting);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, lifetime.Token);
            var snapshot = await request(linked.Token);
            await UniTask.SwitchToMainThread();
            if (!IsCurrent(current) || ct.IsCancellationRequested) return;
            if (snapshot == null || snapshot.RoomId == 0) throw new InvalidOperationException("Missing room snapshot.");
            Data.ResetOnline();
            if (!IsCurrent(current)) return;
            announcedMatches.Clear();
            roomId = snapshot.RoomId;
            Store(snapshot, current);
            if (IsCurrent(current) && roomId != 0) StartWatching();
        }
        catch (RpcException e)
        {
            await UniTask.SwitchToMainThread();
            if (IsCurrent(current)) { Data.SetConnection(RoomConnectionState.Unavailable); Failed?.Invoke(e.Status); }
            throw;
        }
        finally
        {
            await UniTask.SwitchToMainThread();
            if (IsCurrent(current) && roomId == 0 && Data.Connection == RoomConnectionState.Connecting)
                Data.SetConnection(RoomConnectionState.Disconnected);
            EndOperation();
        }
    }

    public UniTask EnterRing(CancellationToken ct = default) => Command(RoomAction.EnterRing, ct: ct);
    public UniTask LeaveRing(CancellationToken ct = default) => Command(RoomAction.LeaveRing, ct: ct);
    public UniTask SetReady(bool ready, CancellationToken ct = default) => Command(RoomAction.SetReady, ready: ready, ct: ct);
    public UniTask Rename(string name, CancellationToken ct = default) => Command(RoomAction.Rename, name: name, ct: ct);
    public UniTask StartMatch(CancellationToken ct = default) => Command(RoomAction.StartMatch, ct: ct);
    public UniTask Leave(CancellationToken ct = default) => Command(RoomAction.LeaveRoom, ct: ct);

    private async UniTask Command(RoomAction action, bool ready = false, string name = "", CancellationToken ct = default)
    {
        BeginOperation();
        int current = generation;
        try
        {
            if (roomId == 0 || Data.Revision == 0) throw new InvalidOperationException("Enter a room first.");
            // Avoid the watch's membership-revoked response racing a successful leave reply.
            if (action == RoomAction.LeaveRoom) StopWatching();
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, lifetime.Token);
            var result = await connector.Apply(new RoomCommand { RoomId = roomId, ExpectedRevision = Data.Revision,
                Action = action, Ready = ready, RoomName = name }, linked.Token);
            await UniTask.SwitchToMainThread();
            if (!IsCurrent(current)) return;
            Store(result, current);
            if (action == RoomAction.LeaveRoom && IsCurrent(current)) Disconnect();
        }
        catch (RpcException e)
        {
            await UniTask.SwitchToMainThread();
            if (IsCurrent(current))
            {
                // Recover authoritative state on rejection or ambiguous transport failure.
                // Never retry a mutation (including StartMatch/Create).
                if (e.StatusCode != StatusCode.Unauthenticated && e.StatusCode != StatusCode.Cancelled)
                {
                    try { await Refresh(ct); } catch (RpcException) { } catch (OperationCanceledException) { }
                }
                if (IsCurrent(current)) Failed?.Invoke(e.Status);
            }
            throw;
        }
        finally
        {
            await UniTask.SwitchToMainThread();
            if (action == RoomAction.LeaveRoom && IsCurrent(current) && roomId != 0 && watching == null) StartWatching();
            EndOperation();
        }
    }

    public async UniTask Refresh(CancellationToken ct = default)
    {
        int current = generation;
        if (roomId == 0 || disposed) throw new InvalidOperationException("Enter a room first.");
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, lifetime.Token);
        var snapshot = await connector.Get(roomId, linked.Token);
        await UniTask.SwitchToMainThread();
        if (IsCurrent(current) && !ct.IsCancellationRequested) Store(snapshot, current);
    }

    private void Store(RoomSnapshot snapshot, int current)
    {
        if (!IsCurrent(current) || snapshot?.RoomId != roomId) return;
        if (!Data.StoreOnline(snapshot, playerId) || !IsCurrent(current)) return;
        // Read the committed state, not a response which a nested callback may have superseded.
        var latest = Data.Snapshot;
        if (!string.IsNullOrEmpty(latest?.MatchId) && (latest.Phase == LobbyPhase.Selecting || latest.Phase == LobbyPhase.Battling)
            && announcedMatches.Add(latest.MatchId + ":" + latest.Phase)) MatchAvailable?.Invoke(latest.MatchId, latest.Phase);
    }

    private void StartWatching()
    {
        StopWatching();
        watching = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        Watch(generation, roomId, watching.Token).Forget();
    }
    private async UniTask Watch(int current, uint id, CancellationToken ct)
    {
        try
        {
            while (IsCurrent(current) && !ct.IsCancellationRequested)
            {
                try
                {
                    using var call = connector.Watch(id, ct);
                    while (await call.ResponseStream.MoveNext(ct))
                    {
                        await UniTask.SwitchToMainThread(ct);
                        if (!IsCurrent(current)) return;
                        Store(call.ResponseStream.Current, current);
                        if (!IsCurrent(current)) return;
                        Data.SetConnection(RoomConnectionState.Connected);
                    }
                }
                catch (RpcException e)
                {
                    await UniTask.SwitchToMainThread();
                    if (!IsCurrent(current) || ct.IsCancellationRequested) return;
                    if (e.StatusCode == StatusCode.NotFound || e.StatusCode == StatusCode.PermissionDenied || e.StatusCode == StatusCode.Unauthenticated)
                    {
                        // Keep the last server snapshot for diagnostics, but disable use via connection status.
                        StopWatching(); roomId = 0; generation++;
                        Data.SetConnection(RoomConnectionState.Unavailable); Failed?.Invoke(e.Status);
                        return;
                    }
                    Failed?.Invoke(e.Status);
                }
                await UniTask.SwitchToMainThread(ct);
                if (!IsCurrent(current)) return;
                Data.SetConnection(RoomConnectionState.Reconnecting);
                await UniTask.Delay(1000, cancellationToken: ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    private bool IsCurrent(int value) => !disposed && !lifetime.IsCancellationRequested && generation == value;
    private void BeginOperation()
    {
        if (disposed) throw new ObjectDisposedException(nameof(RoomUseCase));
        lifetime.Token.ThrowIfCancellationRequested();
        if (busy) throw new InvalidOperationException("A room operation is already pending.");
        busy = true; Data.SetBusy(true);
    }
    private void EndOperation() { busy = false; if (!disposed) Data.SetBusy(false); }
    private void StopWatching() { var old = watching; watching = null; old?.Cancel(); old?.Dispose(); }
    public void Disconnect()
    {
        generation++; roomId = 0; StopWatching(); announcedMatches.Clear(); Data.ResetOnline();
    }
    public void Dispose()
    {
        if (disposed) return;
        Disconnect(); disposed = true; lifetime.Cancel(); lifetime.Dispose(); Data.SetBusy(false);
    }
}
