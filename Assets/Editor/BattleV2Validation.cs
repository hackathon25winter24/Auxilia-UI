using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Network.V2;
using Google.Protobuf;
using Grpc.Core;
using UnityEditor;
using UnityEngine;

/// <summary>Offline receive-path regression checks. No login or live server is required.</summary>
public static class BattleV2Validation
{
    [MenuItem("Tools/Auxilia/Validate V2 receive pipeline")]
    public static void Run()
    {
        CheckDataStore();
        CheckConnectorAndView();
        BattleViewEventValidation.Run();
        Debug.Log("BATTLE_V2_VALIDATION_PASSED: data fidelity, ordering, deduplication, history gaps, commands and typed view notifications.");
    }

    private static void Require(bool value, string reason)
    {
        if (!value) throw new Exception("V2 validation: " + reason);
    }

    private static Snapshot Sample(ulong sequence, string match = "test-match")
    {
        var result = new Snapshot {
            RoomId = 7, LastLogSequence = sequence, PresentationFromSequence = sequence,
            RulesVersion = "test-rules", P1Rate = 1100, P2Rate = 900, P1RateDelta = 12, P2RateDelta = -12,
            State = new State {
                MatchId = match, Revision = sequence, Started = true, Turn = 3,
                TurnPlayerId = "p1", Phase = "action", ServerTime = "2026-09-30T10:00:00Z",
                TurnDeadline = "2026-09-30T10:02:00Z", TestOwnerId = "test-owner",
                LastEvent = new Game.Network.V2.Event { Sequence = sequence, Type = "test", Text = "received" }
            }
        };
        result.State.Players.Add(new Player { Id = "p1", Name = "one", Cost = 40 });
        result.State.Players.Add(new Player { Id = "p2", Name = "two", Cost = 30 });
        result.State.ReadyPlayerIds.Add(new[] { "p1", "p2" });
        result.State.Bases.Add(new Base { OwnerId = "p1", Hp = 321, MaxHp = 400, Position = new Position { X = 0, Y = 2 } });
        result.State.Bases.Add(new Base { OwnerId = "p2", Hp = 400, MaxHp = 400, Position = new Position { X = 7, Y = 2 } });
        var c = new Character {
            Id = "p1-instance", DefinitionId = "sophie", OwnerId = "p1", Name = "Sophie", Hp = 45, MaxHp = 100,
            Position = new Position { X = 2, Y = 3 }, ReviveUsed = true, DepartureUsed = true,
            DrankTurn = 1, HangoverTurn = 2, HangoverUntil = 4, CombatStance = true, BarrierTurn = 3, Wriggling = true
        };
        c.Effects.Add("毒");
        c.TemporaryBuffs.Add("俊足");
        c.UsedSkills.Add("skill", 3);
        result.State.Characters.Add(c);
        result.State.Characters.Add(new Character { Id = "p2-instance", DefinitionId = "sophie", OwnerId = "p2", Hp = 90, MaxHp = 100, Position = new Position { X = 6, Y = 3 } });
        result.State.TileEffects.Add(new TileEffect { OwnerId = "p1", Type = "まきびし", Hp = 10, Position = new Position { X = 4, Y = 2 } });
        result.State.BlockedCells.Add(new Position { X = 3, Y = 0 });
        result.State.Events.Add(result.State.LastEvent.Clone());
        result.SelectedPlayerIds.Add(new[] { "p1", "p2" });
        result.PresentationBatches.Add(Batch(sequence));
        return result;
    }

    private static PresentationBatch Batch(ulong sequence)
    {
        var b = new PresentationBatch { Version = 1, Sequence = sequence, BeforeRevision = sequence - 1, AfterRevision = sequence, CommandId = "cmd-" + sequence, ActionType = "ATTACK" };
        b.Events.Add(new PresentationEvent { Index = 0, Type = "DAMAGED", Cause = "FOLLOW_UP", SourceCharacterId = "p1-instance", SourcePlayerId = "p1", TargetKind = "CHARACTER", TargetId = "p2-instance", Amount = 20, BeforeValue = 100, AfterValue = 80 });
        return b;
    }

    private static ActionLog Log(ulong sequence, bool legacy = false) => new ActionLog {
        Sequence = sequence, Before = Sample(sequence - 1).State, After = Sample(sequence).State,
        Presentation = legacy ? null : Batch(sequence)
    };

    private static void CheckDataStore()
    {
        var data = ScriptableObject.CreateInstance<BattleDataForOnline>();
        try
        {
            var first = Sample(1);
            Require(data.StoreSnapshot(first), "initial snapshot accepted");
            Require(data.Snapshot.Equals(first), "all fields retained");
            var so = new SerializedObject(data);
            var storedJson = so.FindProperty("snapshotJson").stringValue;
            Require(Snapshot.Parser.ParseJson(storedJson).Equals(first), "serialized SO JSON preserves all fields");
            Require(!data.TryDequeuePresentation(out _), "initial history not animated");
            first.State.Characters[0].Hp = 1;
            Require(data.State.Characters[0].Hp == 45, "received protobuf cloned");
            Require(data.player1.characters[0].character_id != data.player2.characters[0].character_id, "instance IDs stay distinct for same definition");
            Require(data.uniqueGrids.Single(x => x.type == "まきびし").gridType == 2, "backend tile names mapped");

            var latest = Sample(35);
            latest.PresentationBatches.Clear();
            for (ulong i = 4; i <= 35; i++) latest.PresentationBatches.Add(Batch(i));
            latest.PresentationFromSequence = 4;
            data.StoreSnapshot(latest);
            Require(data.HasPresentationGap && !data.TryDequeuePresentation(out _), "gap prevents out-of-order animations");
            var history = new LogResponse { NextSequence = 3 };
            history.Logs.Add(Log(2, legacy: true));
            history.Logs.Add(Log(3));
            Require(data.StoreLogs(history), "history stored");
            for (ulong i = 2; i <= 35; i++)
                Require(data.TryDequeuePresentation(out var batch) && batch.Sequence == i, "contiguous event replay " + i);
            Require(!data.HasPresentationGap && !data.TryDequeuePresentation(out _), "gap fully recovered");
            Require(!data.StoreSnapshot(latest) && !data.StoreLogs(history), "duplicates ignored");
            Require(!data.StoreSnapshot(Sample(2)) && data.State.Revision == 35, "late response cannot roll back state");
            var olderClock = latest.Clone();
            olderClock.State.ServerTime = "2026-09-30T09:59:00Z";
            Require(!data.StoreSnapshot(olderClock), "late clock cannot reset countdown");

            var future = new LogResponse { NextSequence = 36 };
            future.Logs.Add(Log(36));
            data.StoreLogs(future);
            Require(!data.TryDequeuePresentation(out _), "future history waits for its snapshot");
            var next = Sample(36);
            next.PresentationBatches.Clear();
            data.StoreSnapshot(next);
            Require(data.TryDequeuePresentation(out var deferred) && deferred.Sequence == 36, "buffered history released when state catches up");
            data.StoreSnapshot(Sample(1, "rematch"));
            Require(data.ActionLogs.Count == 0 && data.PresentationSequence == 1, "new match resets history and cursor");
            var defs = new DefinitionsResponse { DefinitionsJson = "[{\"id\":\"sophie\",\"moveCost\":10,\"description\":\"retained\"}]", RulesVersion = "all-fields" };
            Require(data.StoreDefinitions(defs) && data.Definitions.Equals(defs) && !data.StoreDefinitions(defs), "definitions retained and deduplicated");
            Require(data.BaseMoveCost("sophie") == 10 && data.BaseMoveCost("unknown") == null, "selection costs use server definitions");
        }
        finally { UnityEngine.Object.DestroyImmediate(data); }
    }

    private static void CheckConnectorAndView()
    {
        var go = new GameObject("V2 validation transport");
        var data = ScriptableObject.CreateInstance<BattleDataForOnline>();
        try
        {
            var core = go.AddComponent<NetworkClientCore>();
            core.SetSession("test-token", "p1");
            var connector = go.AddComponent<BattleConnector>();
            var fake = new FakeClient { Response = Sample(1) };
            typeof(BattleConnector).GetField("core", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(connector, core);
            typeof(BattleConnector).GetField("client", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(connector, fake);
            connector.BindData(data);
            var view = go.AddComponent<ViewManager>();
            view.Initialize(data);
            int notifications = 0;
            data.Changed += type => {
                Require(data.State != null && data.player1.base_hp == data.State.Bases[0].Hp, "SO projection committed before ChangeView");
                notifications++;
            };
            Complete(connector.GetRoomGameData(7));
            Require(notifications == 1, "first receive invokes ChangeView exactly once");
            Complete(connector.GetGameData());
            Require(notifications == 1, "duplicate receive does not invoke ChangeView");
            fake.Response = Sample(5);
            fake.History = new LogResponse { NextSequence = 4 };
            for (ulong i = 2; i <= 4; i++) fake.History.Logs.Add(Log(i));
            Complete(connector.GetGameData());
            Require(fake.HistoryCalls == 1 && !data.HasPresentationGap, "connector fetches history only for a gap");
            Require(notifications == 6, "initial sync, four combat events and one history update");
            fake.Response = Sample(6);
            Complete(connector.SendMove("p1-instance", 2, 1));
            Require(fake.Command.ExpectedRevision == 5 && fake.Command.MatchId == "test-match" &&
                fake.Command.CharacterId == "p1-instance" && !string.IsNullOrEmpty(fake.Command.CommandId), "V2 intent contains instance, match, revision and command ID");
            Require(fake.SawAuthorization && notifications == 7, "authenticated action response stored and notified");
            view.Initialize(data);
            fake.Response = Sample(7);
            Complete(connector.GetGameData());
            Require(notifications == 8, "rebinding does not double subscribe");
            view.enabled = false;
            fake.Response = Sample(8);
            Complete(connector.GetGameData());
            Require(notifications == 9, "SO still notifies when view is disabled");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(data); }
    }

    private static void Complete(UniTask<Snapshot> operation)
    {
        var awaiter = operation.GetAwaiter();
        Require(awaiter.IsCompleted, "in-memory RPC must complete on main thread");
        Require(awaiter.GetResult() != null, "RPC succeeds");
    }

    private sealed class FakeClient : BattleServiceV2.BattleServiceV2Client
    {
        public Snapshot Response;
        public LogResponse History;
        public int HistoryCalls;
        public ActionRequest Command;
        public bool SawAuthorization;
        private AsyncUnaryCall<T> Reply<T>(T value, Metadata headers)
        {
            SawAuthorization |= headers.Any(h => h.Key == "authorization" && h.Value == "Bearer test-token");
            return new AsyncUnaryCall<T>(Task.FromResult(value), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
        }
        public override AsyncUnaryCall<Snapshot> GetRoomGameAsync(CreateGameRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default) => Reply(Response, headers);
        public override AsyncUnaryCall<Snapshot> GetGameDataAsync(GameRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default) => Reply(Response, headers);
        public override AsyncUnaryCall<Snapshot> ApplyMoveAsync(ActionRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        { Command = request.Clone(); return Reply(Response, headers); }
        public override AsyncUnaryCall<LogResponse> FetchActionLogAsync(LogRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        { HistoryCalls++; return Reply(History, headers); }
    }
}
