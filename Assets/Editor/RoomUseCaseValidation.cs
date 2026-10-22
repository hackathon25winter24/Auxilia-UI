using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Network.V2;
using Grpc.Core;
using UnityEditor;
using UnityEngine;

public static class RoomUseCaseValidation
{
    // Batch entry point without -quit: allow the reconnect delay/continuations to run.
    public static async void RunAll()
    {
        try
        {
            Run();
            var data = ScriptableObject.CreateInstance<RoomData>();
            var fake = new Fake { Response = Snapshot(1), Stream = new[] { Snapshot(2) }, FailWatches = 1 };
            using var useCase = new RoomUseCase(new LobbyConnector(fake, fake, () => new Metadata()), data, "p1");
            try
            {
                Complete(useCase.Resume(7));
                Check(data.Connection == RoomConnectionState.Reconnecting, "transient error not marked reconnecting");
                await UniTask.Delay(1500);
                Check(fake.Watches == 2 && data.Revision == 2 && data.Connection == RoomConnectionState.Connected, "watch did not reconnect");
                useCase.Disconnect();
                await UniTask.Delay(1100);
                Check(fake.Watches == 2 && data.Snapshot == null, "disconnect reconnected unexpectedly");
            }
            finally { useCase.Dispose(); UnityEngine.Object.DestroyImmediate(data); }
            BattleV2Validation.Run();
            Debug.Log("ROOM_RECONNECT_VALIDATION_PASSED");
            EditorApplication.Exit(0);
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static RoomSnapshot Snapshot(ulong revision, LobbyPhase phase = LobbyPhase.Waiting)
    {
        var value = new RoomSnapshot { RoomId = 7, Revision = revision, RoomName = "room", OwnerId = "observer", Phase = phase,
            MatchId = phase == LobbyPhase.Waiting ? "" : "match", CanReady = true };
        value.Members.Add(new LobbyMember { UserId = "p1", Seat = 1, Name = "one", Rate = 1200 });
        value.Members.Add(new LobbyMember { UserId = "observer", Seat = 0 });
        return value;
    }
    private static void Complete(UniTask task) { var a = task.GetAwaiter(); Check(a.IsCompleted, "fake RPC did not finish"); a.GetResult(); }

    [MenuItem("Tools/Auxilia/Validate room use case")]
    public static void Run()
    {
        var data = ScriptableObject.CreateInstance<RoomData>();
        var fake = new Fake { Response = Snapshot(1) };
        var transport = new LobbyConnector(fake, fake, () => new Metadata { { "authorization", "Bearer test" } });
        using var useCase = new RoomUseCase(transport, data, "p1");
        try
        {
            int changes = 0, matches = 0;
            data.Changed += type => {
                if (data.Snapshot != null)
                    Check(data.room_id == data.Snapshot.RoomId && data.usersData[1].is_host && !data.usersData[0].is_host,
                        "store/projection must precede notifications and owner is not necessarily 1P");
                if (type == RoomChangeType.PhaseChanged) changes++;
            };
            useCase.MatchAvailable += (id, phase) => { Check(id == data.MatchId && data.Snapshot.Phase == phase, "handoff before commit"); matches++; };
            fake.Stream = new[] { Snapshot(1), Snapshot(2, LobbyPhase.Selecting), Snapshot(2, LobbyPhase.Selecting) };
            Complete(useCase.Join(7));
            Check(data.Revision == 2 && changes == 1 && matches == 1 && fake.Watches == 1, "stream ordering/dedup/handoff");
            var copy = data.Snapshot; copy.RoomName = "tampered";
            Check(data.Snapshot.RoomName == "room", "snapshot leaked mutable authority");
            fake.Response = Snapshot(1); Complete(useCase.Refresh()); Check(data.Revision == 2, "old unary overwrote stream");
            fake.Response = Snapshot(3, LobbyPhase.Battling); Complete(useCase.SetReady(true));
            Check(fake.Command.ExpectedRevision == 2 && fake.Command.Ready && matches == 2, "intent revision/phase handoff");
            Complete(useCase.Refresh()); Check(matches == 2, "duplicate handoff");
            fake.Reject = true; fake.Response = Snapshot(4, LobbyPhase.Battling);
            try { Complete(useCase.Rename("rename")); throw new Exception("rejection lost"); }
            catch (RpcException e) { Check(e.StatusCode == StatusCode.Aborted, "wrong status"); }
            Check(fake.Commands == 2 && data.Revision == 4 && !data.IsBusy, "rejection must refresh, never retry mutation");
            fake.Reject = false;
            fake.Pending = new TaskCompletionSource<RoomSnapshot>();
            var pending = useCase.Refresh();
            useCase.Disconnect();
            fake.Pending.SetResult(Snapshot(99)); Complete(pending);
            Check(data.Snapshot == null, "late response survived disconnect");
            fake.Pending = null; fake.Response = Snapshot(5); fake.Stream = Array.Empty<RoomSnapshot>();
            Complete(useCase.Resume(7));
            Check(data.Revision == 5 && fake.Watches == 2 && fake.Authorized, "resume/authentication");
            fake.Response = Snapshot(6); Complete(useCase.Leave());
            Check(data.Snapshot == null && data.Connection == RoomConnectionState.Disconnected, "leave lifecycle");
            Debug.Log("ROOM_USE_CASE_VALIDATION_PASSED");
        }
        finally { useCase.Dispose(); UnityEngine.Object.DestroyImmediate(data); }
    }
    private sealed class Fake : LobbyServiceV2.LobbyServiceV2Client
    {
        public RoomSnapshot Response;
        public RoomSnapshot[] Stream = Array.Empty<RoomSnapshot>();
        public RoomCommand Command;
        public int Watches, Commands, FailWatches;
        public bool Authorized, Reject;
        public TaskCompletionSource<RoomSnapshot> Pending;
        private AsyncUnaryCall<RoomSnapshot> Reply(Metadata headers, bool reject = false)
        {
            Authorized |= headers.Any(h => h.Key == "authorization" && h.Value == "Bearer test");
            var task = reject ? Task.FromException<RoomSnapshot>(new RpcException(new Status(StatusCode.Aborted, "stale"))) : Pending?.Task ?? Task.FromResult(Response);
            return new AsyncUnaryCall<RoomSnapshot>(task, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
        }
        public override AsyncUnaryCall<RoomSnapshot> JoinLobbyAsync(LobbyRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default) => Reply(headers);
        public override AsyncUnaryCall<RoomSnapshot> GetLobbyAsync(LobbyRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default) => Reply(headers);
        public override AsyncUnaryCall<RoomSnapshot> ApplyRoomCommandAsync(RoomCommand request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        { Commands++; Command = request.Clone(); return Reply(headers, Reject); }
        public override AsyncServerStreamingCall<RoomSnapshot> WatchRoom(LobbyRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        { Watches++; return new AsyncServerStreamingCall<RoomSnapshot>(new Reader(Stream, FailWatches-- > 0), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }); }
    }
    private sealed class Reader : IAsyncStreamReader<RoomSnapshot>
    {
        private readonly RoomSnapshot[] values;
        private int index;
        private readonly bool fail;
        public RoomSnapshot Current { get; private set; }
        public Reader(RoomSnapshot[] values, bool fail = false) { this.values = values; this.fail = fail; }
        public Task<bool> MoveNext(CancellationToken ct)
        {
            if (fail) return Task.FromException<bool>(new RpcException(new Status(StatusCode.Unavailable, "test reconnect")));
            if (index < values.Length) { Current = values[index++]; return Task.FromResult(true); }
            return Wait(ct);
        }
        private static async Task<bool> Wait(CancellationToken ct) { await Task.Delay(Timeout.Infinite, ct); return false; }
    }
}
