using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using UnityEngine;
using V2 = Game.Network.V2;

/// <summary>All battle responses live here. Mutate on Unity's main thread, then notify views.</summary>
[CreateAssetMenu(fileName = "BattleDataForOnline", menuName = "Scriptable Objects/BattleDataForOnline")]
public class BattleDataForOnline : ScriptableObject
{
    // Protobuf is not Unity-serializable. JSON retains every field in the Inspector;
    // typed runtime copies are the authoritative data, not the legacy UI projection.
    [SerializeField, TextArea(3, 12)] private string snapshotJson;
    [SerializeField, TextArea(3, 8)] private string definitionsJson;
    [SerializeField, TextArea(3, 8)] private string actionLogsJson;
    [NonSerialized] private V2.Snapshot snapshot;
    [NonSerialized] private V2.DefinitionsResponse definitions;
    [NonSerialized] private DefinitionCosts definitionCosts;
    [NonSerialized] private readonly SortedDictionary<ulong, V2.ActionLog> actionLogs = new();
    [NonSerialized] private readonly Queue<V2.PresentationBatch> pendingPresentation = new();
    [NonSerialized] private readonly SortedDictionary<ulong, V2.PresentationBatch> waitingPresentation = new();
    [NonSerialized] private readonly List<V2.PresentationBatch> pendingViewBatches = new();
    [NonSerialized] private bool snapshotChanged;
    [NonSerialized] private bool definitionsChanged;
    [NonSerialized] private bool logsChanged;
    [NonSerialized] private int notificationGeneration;
    public V2.Snapshot Snapshot => snapshot;
    public V2.State State => snapshot?.State;
    public V2.DefinitionsResponse Definitions => definitions;
    public IReadOnlyCollection<V2.ActionLog> ActionLogs => actionLogs.Values;
    public ulong PresentationSequence { get; private set; }
    public ulong NextLogSequence { get; private set; }
    public bool HasPresentationGap => snapshot != null && PresentationSequence < snapshot.LastLogSequence;
    public double ReceivedAtRealtime { get; private set; }
    public const string StateSyncType = "STATE_SYNC";
    public const string DefinitionsChangedType = "DEFINITIONS_CHANGED";
    public const string ActionLogsChangedType = "ACTION_LOG_CHANGED";
    public event Action<string> Changed;
    // Valid during Changed callbacks. Clone the event when retaining it for asynchronous animation.
    public V2.PresentationEvent CurrentPresentationEvent { get; private set; }
    public ulong CurrentPresentationSequence { get; private set; }

    // Existing UI/tutorial fields remain available. Coordinates use server space.
    public int turn_number;
    public bool is_1p_turn;
    public bool is_finished;
    public string winner_player_id;
    public PlayerState player1 = new();
    public PlayerState player2 = new();
    public List<UniqueGrid> uniqueGrids = new();

    public void ResetRuntime()
    {
        ResetViewNotifications();
        snapshot = null;
        definitions = null;
        definitionCosts = null;
        snapshotJson = definitionsJson = actionLogsJson = "";
        actionLogs.Clear();
        pendingPresentation.Clear();
        waitingPresentation.Clear();
        PresentationSequence = NextLogSequence = 0;
        turn_number = 0;
        is_1p_turn = is_finished = false;
        winner_player_id = "";
        player1 = new PlayerState();
        player2 = new PlayerState();
        uniqueGrids.Clear();
    }

    public bool StoreDefinitions(V2.DefinitionsResponse response)
    {
        if (response == null || response.Equals(definitions)) return false;
        definitions = response.Clone();
        definitionsJson = JsonFormatter.Default.Format(definitions);
        definitionCosts = JsonUtility.FromJson<DefinitionCosts>("{\"items\":" + definitions.DefinitionsJson + "}");
        definitionsChanged = true;
        return true;
    }

    public int? BaseMoveCost(string definitionId) => definitionCosts?.items?.FirstOrDefault(d => d.id == definitionId)?.moveCost;

    [Serializable] private class DefinitionCosts { public DefinitionCost[] items; }
    [Serializable] private class DefinitionCost { public string id; public int moveCost; }

    public bool StoreSnapshot(V2.Snapshot response)
    {
        if (response?.State == null) return false;
        bool first = snapshot == null || State.MatchId != response.State.MatchId;
        if (!first && (response.State.Revision < State.Revision || response.LastLogSequence < snapshot.LastLogSequence))
            return false;
        if (!first && response.State.Revision == State.Revision && response.LastLogSequence == snapshot.LastLogSequence &&
            DateTimeOffset.TryParse(State.ServerTime, out var oldTime) &&
            DateTimeOffset.TryParse(response.State.ServerTime, out var newTime) && newTime < oldTime) return false;
        if (response.Equals(snapshot)) return false;
        if (first)
        {
            // A new match invalidates queued notifications for the previous match.
            bool keepDefinitionsChanged = definitionsChanged;
            ResetViewNotifications();
            definitionsChanged = keepDefinitionsChanged;
            actionLogs.Clear();
            actionLogsJson = "";
            pendingPresentation.Clear();
            waitingPresentation.Clear();
            NextLogSequence = 0;
            // First connection establishes the board without replaying historical attacks.
            PresentationSequence = response.LastLogSequence;
        }
        snapshot = response.Clone();
        snapshotJson = JsonFormatter.Default.Format(snapshot);
        ReceivedAtRealtime = Time.realtimeSinceStartupAsDouble;
        if (!first)
            foreach (var batch in snapshot.PresentationBatches) QueuePresentation(batch);
        DrainPresentation();
        ProjectState();
        snapshotChanged = true;
        return true;
    }

    public bool StoreLogs(V2.LogResponse response)
    {
        if (response == null || State == null) return false;
        bool changed = false;
        foreach (var log in response.Logs)
        {
            if ((log.After ?? log.Before)?.MatchId != State.MatchId) continue;
            if (!actionLogs.TryGetValue(log.Sequence, out var old) || !old.Equals(log))
            {
                actionLogs[log.Sequence] = log.Clone();
                changed = true;
            }
            // Pre-event history still advances the cursor without inventing animations.
            QueuePresentation(log.Presentation ?? new V2.PresentationBatch { Sequence = log.Sequence });
        }
        if (NextLogSequence != response.NextSequence) changed = true;
        NextLogSequence = response.NextSequence;
        var all = new V2.LogResponse { NextSequence = NextLogSequence };
        all.Logs.Add(actionLogs.Values);
        actionLogsJson = JsonFormatter.Default.Format(all);
        logsChanged |= changed;
        return changed;
    }

    private void QueuePresentation(V2.PresentationBatch batch)
    {
        if (batch.Sequence <= PresentationSequence) return;
        waitingPresentation[batch.Sequence] = batch.Clone();
        DrainPresentation();
    }

    private void DrainPresentation()
    {
        while (snapshot != null && PresentationSequence < snapshot.LastLogSequence &&
            waitingPresentation.TryGetValue(PresentationSequence + 1, out var next))
        {
            waitingPresentation.Remove(next.Sequence);
            pendingPresentation.Enqueue(next);
            pendingViewBatches.Add(next.Clone());
            PresentationSequence = next.Sequence;
        }
    }

    public bool TryDequeuePresentation(out V2.PresentationBatch batch)
    {
        batch = pendingPresentation.Count == 0 ? null : pendingPresentation.Dequeue();
        return batch != null;
    }

    public void NotifyChanged()
    {
        // Snapshot and any recovered history have already been committed. Do not use
        // State.LastEvent: it describes only the last change and loses intermediate hits.
        var batches = pendingViewBatches.ToArray();
        bool sync = snapshotChanged && !batches.Any(b => b.Events.Count > 0);
        bool notifyDefinitions = definitionsChanged;
        bool notifyLogs = logsChanged;
        int generation = notificationGeneration;
        pendingViewBatches.Clear();
        snapshotChanged = definitionsChanged = logsChanged = false;
        try
        {
            if (sync) Changed?.Invoke(StateSyncType);
            foreach (var batch in batches)
                foreach (var change in batch.Events)
                {
                    if (generation != notificationGeneration) return;
                    CurrentPresentationSequence = batch.Sequence;
                    CurrentPresentationEvent = change;
                    Changed?.Invoke(change.Type);
                }
            CurrentPresentationSequence = 0;
            CurrentPresentationEvent = null;
            if (generation != notificationGeneration) return;
            if (notifyDefinitions) Changed?.Invoke(DefinitionsChangedType);
            if (generation != notificationGeneration) return;
            if (notifyLogs) Changed?.Invoke(ActionLogsChangedType);
        }
        finally
        {
            CurrentPresentationSequence = 0;
            CurrentPresentationEvent = null;
        }
    }

    private void ResetViewNotifications()
    {
        notificationGeneration++;
        pendingViewBatches.Clear();
        snapshotChanged = definitionsChanged = logsChanged = false;
        CurrentPresentationSequence = 0;
        CurrentPresentationEvent = null;
    }

    private void ProjectState()
    {
        turn_number = State.Turn;
        is_finished = State.Finished;
        winner_player_id = State.WinnerId;
        player1 = ProjectPlayer(0, snapshot.P1Rate, snapshot.P1RateDelta, player1);
        player2 = ProjectPlayer(1, snapshot.P2Rate, snapshot.P2RateDelta, player2);
        is_1p_turn = State.TurnPlayerId == player1.player_id;
        uniqueGrids = State.BlockedCells.Select(p => new UniqueGrid { position = Position(p), gridType = 0 }).ToList();
        uniqueGrids.AddRange(State.TileEffects.Select(t => new UniqueGrid {
            position = Position(t.Position), gridType = t.Type switch {
                "地雷" => 1, "まきびし" => 2, "毒ガス" => 3, "不変" => 4, _ => -1
            }, type = t.Type, owner_id = t.OwnerId, hp = t.Hp
        }));
    }

    private PlayerState ProjectPlayer(int index, int rate, int delta, PlayerState previous)
    {
        if (index >= State.Players.Count) return new PlayerState();
        var p = State.Players[index];
        var b = State.Bases.FirstOrDefault(x => x.OwnerId == p.Id);
        return new PlayerState {
            player_id = p.Id, player_name = p.Name, current_cost_remaining = p.Cost,
            base_position = Position(b?.Position), base_hp = b?.Hp ?? 0, base_max_hp = b?.MaxHp ?? 0,
            rate = rate, rate_updown = delta,
            characters = State.Characters.Where(c => c.OwnerId == p.Id).Select(c => new CharactersBattleData {
                character_id = c.Id, definition_id = c.DefinitionId,
                unique_id = BattleCharacterIds.ToLocal(c.DefinitionId),
                now_character_hp = c.Hp, max_hp = c.MaxHp,
                now_character_move_cost = -1, // V1/tutorial field: V2 does not transmit an effective move cost.
                now_character_position = Position(c.Position), effects = c.Effects.ToArray(),
                character_isSelected = previous?.characters?.FirstOrDefault(x => x.character_id == c.Id)?.character_isSelected ?? false,
                debuffs = new[] { "威力上昇", "俊足", "俊敏化", "毒", "麻痺", "鈍足", "鈍化", "出血" }.Select(c.Effects.Contains).ToArray()
            }).ToArray()
        };
    }
    private static Vector2Int Position(V2.Position p) => p == null ? default : new Vector2Int(p.X, p.Y);
}

// Explicit mapping: backend definition order is not CharacterData.asset order.
public static class BattleCharacterIds
{
    private static readonly string[] Ids = { "sophie", "jude", "nadia", "tsukiha", "aoi", "sena", "berenice", "chiyo", "shicho", "zina", "dana" };
    public static string ToServer(int index) => index >= 0 && index < Ids.Length ? Ids[index] : throw new ArgumentOutOfRangeException(nameof(index));
    public static int ToLocal(string id) => Array.IndexOf(Ids, id);
}

[Serializable]
public class PlayerState
{
    public string player_id;
    public string player_name;
    public Vector2Int base_position;
    public int base_hp;
    public int base_max_hp;
    public int current_cost_remaining;
    public CharactersBattleData[] characters = Array.Empty<CharactersBattleData>();
    public int rate;
    public int rate_updown;
}

[Serializable]
public class CharactersBattleData
{
    public string character_id;
    public string definition_id;
    public int unique_id; // Sprite index, NEVER the server instance ID.
    public int now_character_hp;
    public int max_hp;
    public bool character_isSelected;
    public int now_character_move_cost;
    public bool[] debuffs = new bool[8];
    public string[] effects = Array.Empty<string>();
    public Vector2Int now_character_position;
}

[Serializable]
public class UniqueGrid
{
    public Vector2Int position;
    public int gridType;
    public string type;
    public string owner_id;
    public int hp;
}
