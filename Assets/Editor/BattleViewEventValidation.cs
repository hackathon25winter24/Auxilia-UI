using System;
using System.Collections.Generic;
using System.Linq;
using Game.Network.V2;
using UnityEditor;
using UnityEngine;

public static class BattleViewEventValidation
{
    [MenuItem("Tools/Auxilia/Validate typed view events")]
    public static void Run()
    {
        var data = ScriptableObject.CreateInstance<BattleDataForOnline>();
        var other = ScriptableObject.CreateInstance<BattleDataForOnline>();
        var go = new GameObject("Typed view event validation");
        try
        {
            var view = go.AddComponent<ViewManager>();
            view.Initialize(data);
            var types = new List<string>();
            var indices = new List<uint>();
            data.Changed += type => {
                types.Add(type);
                if (data.CurrentPresentationEvent != null) indices.Add(data.CurrentPresentationEvent.Index);
            };
            int moved = 0, damages = 0, synced = 0;
            view.moved += () => moved++;
            view.damaged += () => {
                Require(view.CurrentEvent.TargetId == "target" && view.CurrentSequence > 0, "payload available while callback executes");
                Require(data.State.Revision >= view.CurrentSequence, "latest board already stored");
                damages++;
            };
            view.state_synced += () => synced++;
            data.StoreSnapshot(SnapshotAt(1, "DAMAGED"));
            data.NotifyChanged();
            Require(types.SequenceEqual(new[] { "STATE_SYNC" }) && damages == 0, "initial sync must not replay past attacks");
            types.Clear();
            string[] expected = { "SKILL_USED", "MOVED", "DAMAGED", "DAMAGED", "COST_CHANGED" };
            var next = SnapshotAt(2, expected);
            data.StoreSnapshot(next);
            Require(types.Count == 0, "do not notify before receive transaction finishes");
            data.NotifyChanged();
            Require(types.SequenceEqual(expected) && indices.SequenceEqual(new uint[] { 0, 1, 2, 3, 4 }), "preserve all types, including repeated damage, in server order");
            Require(moved == 1 && damages == 2 && synced == 1, "dispatch matching view events");
            Require(data.CurrentPresentationEvent == null, "clear payload context after dispatch");
            types.Clear();
            data.StoreSnapshot(next);
            data.NotifyChanged();
            Require(types.Count == 0, "duplicate snapshot and duplicate flush do not replay");

            var clock = next.Clone();
            clock.State.ServerTime = "2026-10-07T00:00:01Z";
            data.StoreSnapshot(clock);
            data.NotifyChanged();
            Require(types.SequenceEqual(new[] { "STATE_SYNC" }) && damages == 2, "clock-only update does not replay damage");
            types.Clear();
            data.StoreDefinitions(new DefinitionsResponse { DefinitionsJson = "[]", RulesVersion = "test" });
            data.NotifyChanged();
            Require(types.SequenceEqual(new[] { "DEFINITIONS_CHANGED" }), "definition update has separate type");
            types.Clear();

            // Sequence 3 is missing. Sequence 4 must wait for recovered history.
            data.StoreSnapshot(SnapshotAt(4, "DAMAGED"));
            data.NotifyChanged();
            Require(damages == 2, "do not dispatch out-of-order events");
            types.Clear();
            var logs = new LogResponse { NextSequence = 3 };
            var third = SnapshotAt(3, "MOVED");
            logs.Logs.Add(new ActionLog { Sequence = 3, After = third.State, Presentation = third.PresentationBatches[0] });
            data.StoreLogs(logs);
            data.NotifyChanged();
            Require(types.SequenceEqual(new[] { "MOVED", "DAMAGED", "ACTION_LOG_CHANGED" }) && moved == 2 && damages == 3, "recovered history dispatched before newer events");
            types.Clear();
            data.StoreLogs(logs);
            data.NotifyChanged();
            Require(types.Count == 0, "history replay does not notify twice");

            view.Initialize(data);
            data.StoreSnapshot(SnapshotAt(5, "DAMAGED"));
            data.NotifyChanged();
            Require(damages == 4, "Initialize does not double subscribe");
            view.Initialize(other);
            data.StoreSnapshot(SnapshotAt(6, "DAMAGED"));
            data.NotifyChanged();
            Require(damages == 4, "rebinding unsubscribes old SO");
            other.StoreSnapshot(SnapshotAt(1));
            other.NotifyChanged();
            other.StoreSnapshot(SnapshotAt(2, "DAMAGED"));
            other.NotifyChanged();
            Require(damages == 5, "new SO subscribed");
            view.enabled = false;
            other.StoreSnapshot(SnapshotAt(3, "DAMAGED"));
            other.NotifyChanged();
            Require(damages == 5, "disabled view does not dispatch");
            Debug.Log("BATTLE_VIEW_EVENT_VALIDATION_PASSED");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(data);
            UnityEngine.Object.DestroyImmediate(other);
        }
    }

    private static Snapshot SnapshotAt(ulong sequence, params string[] types)
    {
        var snapshot = new Snapshot {
            RoomId = 1, LastLogSequence = sequence, PresentationFromSequence = sequence,
            State = new State { MatchId = "match", Revision = sequence, ServerTime = "2026-10-07T00:00:00Z" }
        };
        var batch = new PresentationBatch { Sequence = sequence, Version = 1 };
        for (int i = 0; i < types.Length; i++)
            batch.Events.Add(new PresentationEvent { Index = (uint)i, Type = types[i], TargetId = "target", Amount = i });
        snapshot.PresentationBatches.Add(batch);
        return snapshot;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("Typed view event validation: " + message);
    }
}
