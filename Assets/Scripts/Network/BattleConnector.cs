using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using UnityEngine;
using Game.Network.V2;

/// <summary>V2 transport. Every successful response is stored before any view notification.</summary>
public class BattleConnector : MonoBehaviour
{
    private NetworkClientCore core;
    private BattleServiceV2.BattleServiceV2Client client;
    private BattleServiceV2.BattleServiceV2Client streamClient;
    private CancellationTokenSource streamCts;
    private readonly SemaphoreSlim receiveLock = new(1, 1);
    private readonly SemaphoreSlim actionLock = new(1, 1);
    private int roomGeneration;
    private uint activeRoom;
    public BattleDataForOnline Data { get; private set; }

    public void Initialize(NetworkClientCore networkCore, BattleDataForOnline data)
    {
        core = networkCore;
        client = new BattleServiceV2.BattleServiceV2Client(core.Channel);
        streamClient = new BattleServiceV2.BattleServiceV2Client(core.StreamChannel);
        BindData(data);
    }

    public void BindData(BattleDataForOnline data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (Data == data) return;
        StopStream().Forget();
        roomGeneration++;
        activeRoom = 0;
        Data = data;
        Data.ResetRuntime();
    }

    private Metadata Headers => core.SessionHeaders;
    private void EnterRoom(uint roomId)
    {
        if (activeRoom == roomId) return;
        StopStream().Forget();
        roomGeneration++;
        activeRoom = roomId;
        Data.ResetRuntime();
    }

    public async UniTask<DefinitionsResponse> GetDefinitions(CancellationToken ct = default)
    {
        var generation = roomGeneration;
        try
        {
            using var call = client.GetDefinitionsAsync(new Empty(), Headers, cancellationToken: ct);
            var response = await call.ResponseAsync;
            await UniTask.SwitchToMainThread(ct);
            if (generation == roomGeneration && Data.StoreDefinitions(response)) Data.NotifyChanged();
            return response;
        }
        catch (RpcException e) { await Report(e, ct); return null; }
    }

    public UniTask<Snapshot> CreateGameData(uint roomId, CancellationToken ct = default)
    {
        EnterRoom(roomId);
        return Receive(() => client.CreateGameAsync(new CreateGameRequest { RoomId = roomId }, Headers, cancellationToken: ct), null, ct);
    }

    // Room IDs locate a match; commands and streaming always use the returned string MatchId.
    public UniTask<Snapshot> GetGameData(int roomId, CancellationToken ct = default)
    {
        EnterRoom(checked((uint)roomId));
        return Receive(() => client.GetRoomGameAsync(new CreateGameRequest { RoomId = (uint)roomId }, Headers, cancellationToken: ct), null, ct);
    }
    public UniTask<Snapshot> GetGameData(CancellationToken ct = default)
    {
        var request = CurrentGame();
        return Receive(() => client.GetGameDataAsync(request, Headers, cancellationToken: ct), request.MatchId, ct);
    }
    public UniTask<Snapshot> RegisterCharacters(string[] definitionIds, CancellationToken ct = default)
    {
        var request = new SelectionRequest { MatchId = CurrentGame().MatchId };
        request.DefinitionIds.Add(definitionIds);
        return Receive(() => client.RegisterCharactersAsync(request, Headers, cancellationToken: ct), request.MatchId, ct);
    }
    public UniTask<Snapshot> Ready(CancellationToken ct = default)
    {
        var request = CurrentGame();
        return Receive(() => client.ReadyAsync(request, Headers, cancellationToken: ct), request.MatchId, ct);
    }
    public UniTask<Snapshot> CancelGame(CancellationToken ct = default)
    {
        var request = CurrentGame();
        return Receive(() => client.CancelGameAsync(request, Headers, cancellationToken: ct), request.MatchId, ct);
    }

    // The caller sends intent only. Damage, affected targets, costs, tiles and timers are server-owned.
    public UniTask<Snapshot> SendMove(string characterId, int x, int y, CancellationToken ct = default) =>
        SendAction("MOVE", characterId, 0, new Position { X = x, Y = y }, null, ct);
    public UniTask<Snapshot> SendAttack(string characterId, int attackIndex, Position target, Position direction, CancellationToken ct = default) =>
        SendAction("ATTACK", characterId, attackIndex, target, direction, ct);
    public UniTask<Snapshot> SendTurnEnd(CancellationToken ct = default) => SendAction("END_TURN", "", 0, null, null, ct);
    public UniTask<Snapshot> Surrender(CancellationToken ct = default) => SendAction("SURRENDER", "", 0, null, null, ct);

    private async UniTask<Snapshot> SendAction(string type, string character, int attack, Position target, Position direction, CancellationToken ct)
    {
        var generation = roomGeneration;
        var matchId = CurrentGame().MatchId;
        await actionLock.WaitAsync(ct);
        try
        {
            await UniTask.SwitchToMainThread(ct);
            if (generation != roomGeneration || Data.State?.MatchId != matchId) return null;
            var request = new ActionRequest {
                MatchId = matchId, CommandId = Guid.NewGuid().ToString("N"),
                ExpectedRevision = Data.State.Revision, CharacterId = character,
                AttackIndex = attack, Target = target?.Clone(), Direction = direction?.Clone()
            };
            AsyncUnaryCall<Snapshot> Call() => type switch {
                "MOVE" => client.ApplyMoveAsync(request, Headers, cancellationToken: ct),
                "ATTACK" => client.ApplyAttackAsync(request, Headers, cancellationToken: ct),
                "END_TURN" => client.EndTurnAsync(request, Headers, cancellationToken: ct),
                "SURRENDER" => client.SurrenderAsync(request, Headers, cancellationToken: ct),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
            // An ambiguous transport failure retries the SAME command ID and revision.
            return await Receive(Call, matchId, ct, retryTransport: true);
        }
        finally { actionLock.Release(); }
    }

    private GameRequest CurrentGame()
    {
        if (Data?.State == null) throw new InvalidOperationException("先にGetRoomGameで試合を取得してください。");
        return new GameRequest { MatchId = Data.State.MatchId };
    }

    private async UniTask<Snapshot> Receive(Func<AsyncUnaryCall<Snapshot>> send, string expectedMatch, CancellationToken ct, bool retryTransport = false)
    {
        var generation = roomGeneration;
        try
        {
            Snapshot response;
            try { using var call = send(); response = await call.ResponseAsync; }
            catch (RpcException e) when (retryTransport && (e.StatusCode == StatusCode.Unavailable || e.StatusCode == StatusCode.DeadlineExceeded))
            {
                await UniTask.Delay(500, cancellationToken: ct);
                using var retry = send();
                response = await retry.ResponseAsync;
            }
            await StoreResponse(response, generation, expectedMatch, ct);
            return generation == roomGeneration ? response : null;
        }
        catch (RpcException e)
        {
            await Report(e, ct);
            // Refresh after rejection, but never automatically reissue the rejected action.
            if (retryTransport && generation == roomGeneration && Data.State?.MatchId == expectedMatch)
                await GetGameData(ct);
            return null;
        }
    }

    private async UniTask StoreResponse(Snapshot response, int generation, string expectedMatch, CancellationToken ct)
    {
        await receiveLock.WaitAsync(ct);
        bool changed = false;
        try
        {
            await UniTask.SwitchToMainThread(ct);
            if (generation != roomGeneration || (expectedMatch != null && Data.State?.MatchId != expectedMatch)) return;
            if (Data.State != null && response.State.MatchId != Data.State.MatchId) StopStream().Forget();
            changed = Data.StoreSnapshot(response);
            // Only gaps beyond the bundled window need history RPCs.
            while (Data.HasPresentationGap)
            {
                ulong cursor = Data.PresentationSequence;
                using var call = client.FetchActionLogAsync(new LogRequest {
                    MatchId = Data.State.MatchId, AfterSequence = cursor, Limit = 100
                }, Headers, cancellationToken: ct);
                var logs = await call.ResponseAsync;
                await UniTask.SwitchToMainThread(ct);
                if (generation != roomGeneration) return;
                changed |= Data.StoreLogs(logs);
                if (Data.PresentationSequence <= cursor) break;
            }
        }
        finally
        {
            await UniTask.SwitchToMainThread();
            if (changed && generation == roomGeneration) Data.NotifyChanged();
            receiveLock.Release();
        }
    }

    public async UniTask<LogResponse> FetchActionLog(ulong afterSequence, uint limit = 100, CancellationToken ct = default)
    {
        var matchId = CurrentGame().MatchId;
        var generation = roomGeneration;
        try
        {
            using var call = client.FetchActionLogAsync(new LogRequest { MatchId = matchId, AfterSequence = afterSequence, Limit = limit }, Headers, cancellationToken: ct);
            var response = await call.ResponseAsync;
            await UniTask.SwitchToMainThread(ct);
            if (generation == roomGeneration && Data.State?.MatchId == matchId && Data.StoreLogs(response)) Data.NotifyChanged();
            return response;
        }
        catch (RpcException e) { await Report(e, ct); return null; }
    }

    public void StartStream(CancellationToken ct = default)
    {
        StopStream().Forget();
        var matchId = CurrentGame().MatchId;
        streamCts = CancellationTokenSource.CreateLinkedTokenSource(ct, this.GetCancellationTokenOnDestroy());
        StreamLoop(matchId, roomGeneration, streamCts.Token).Forget();
    }

    private async UniTask StreamLoop(string matchId, int generation, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && generation == roomGeneration)
            {
                try
                {
                    using var call = streamClient.StreamGame(new GameRequest { MatchId = matchId }, Headers, cancellationToken: ct);
                    while (await call.ResponseStream.MoveNext(ct))
                        await StoreResponse(call.ResponseStream.Current, generation, matchId, ct);
                    if (Data.State?.Finished == true) return;
                }
                catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled && ct.IsCancellationRequested) { return; }
                catch (RpcException e)
                {
                    await Report(e, ct);
                    if (e.StatusCode == StatusCode.Unauthenticated || e.StatusCode == StatusCode.PermissionDenied || e.StatusCode == StatusCode.NotFound) return;
                }
                // Delay after normal EOF as well, to avoid a reconnect spin loop.
                await UniTask.Delay(3000, cancellationToken: ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async UniTask Report(RpcException e, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);
        await UniTask.SwitchToMainThread(ct);
        core.ShowErrorMessage($"通信に失敗しました ({e.StatusCode}): {e.Status.Detail}");
    }
    public UniTask StopStream()
    {
        var source = streamCts;
        streamCts = null;
        source?.Cancel();
        source?.Dispose();
        return UniTask.CompletedTask;
    }
    private void OnDestroy() { roomGeneration++; StopStream().Forget(); }
}
