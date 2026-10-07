using System;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public BattleDataForOnline battleDataForOnline;
    public event Action skill_used;
    public event Action targeted;
    public event Action damaged;
    public event Action healed;
    public event Action defeated;
    public event Action revived;
    public event Action moved;
    public event Action effected;
    public event Action attack_blocked;
    public event Action effect_blocked;
    public event Action form_changed;
    public event Action grid_changed;
    public event Action grid_hp_changed;
    public event Action cost_changed;
    public event Action match_started;
    public event Action turn_changed;
    public event Action phase_changed;
    public event Action match_finished;
    public event Action state_synced;
    public event Action definitions_changed;
    public event Action action_logs_changed;
    public event Action character_spawned;
    public event Action character_removed;
    public event Action match_created;
    public Game.Network.V2.PresentationEvent CurrentEvent => battleDataForOnline?.CurrentPresentationEvent;
    public ulong CurrentSequence => battleDataForOnline?.CurrentPresentationSequence ?? 0;
    private BattleDataForOnline subscribedData;

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }
    public void Initialize(BattleDataForOnline battleData)
    {
        battleDataForOnline = battleData;
        if (isActiveAndEnabled) Subscribe();
        else Unsubscribe();
    }

    private void Subscribe()
    {
        Unsubscribe();
        subscribedData = battleDataForOnline;
        if (subscribedData != null) subscribedData.Changed += ChangeView;
    }

    private void Unsubscribe()
    {
        if (subscribedData != null) subscribedData.Changed -= ChangeView;
        subscribedData = null;
    }

    private void OnDestroy() => Unsubscribe();

    public void ChangeView(string type)
    {
        if (this == null || !isActiveAndEnabled) return;
        switch(type)
        {
            case BattleDataForOnline.StateSyncType : state_synced?.Invoke(); break;
            case BattleDataForOnline.DefinitionsChangedType : definitions_changed?.Invoke(); break;
            case BattleDataForOnline.ActionLogsChangedType : action_logs_changed?.Invoke(); break;
            case "MATCH_CREATED" : match_created?.Invoke(); break;
            case "CHARACTER_SPAWNED" : character_spawned?.Invoke(); break;
            case "CHARACTER_REMOVED" : character_removed?.Invoke(); break;
            case "SKILL_USED" : skill_used?.Invoke(); break;
            case "TARGETED" : targeted?.Invoke(); break;
            case "DAMAGED" : damaged?.Invoke(); break;
            case "HEALED" : healed?.Invoke(); break;
            case "DEFEATED" : defeated?.Invoke(); break;
            case "REVIVED" : revived?.Invoke(); break;
            case "MOVED" : moved?.Invoke(); break;
            case "EFFECT_ADDED" : effected?.Invoke(); break;
            case "EFFECT_REMOVED" : effected?.Invoke(); break;
            case "ATTACK_BLOCKED" : attack_blocked?.Invoke(); break;
            case "EFFECT_BLOCKED" : effect_blocked?.Invoke(); break;
            case "FORM_CHANGED" : form_changed?.Invoke(); break;
            case "TILE_ADDED" : grid_changed?.Invoke(); break;
            case "TILE_REMOVED" : grid_changed?.Invoke(); break;
            case "TILE_HP_CHANGED" : grid_hp_changed?.Invoke(); break;
            case "COST_CHANGED" : cost_changed?.Invoke(); break;
            case "MATCH_STARTED" : match_started?.Invoke(); break;
            case "TURN_CHANGED" : turn_changed?.Invoke(); break;
            case "PHASE_CHANGED" : phase_changed?.Invoke(); break;
            case "MATCH_FINISHED" : match_finished?.Invoke(); break;
            default: Debug.Log(type + "は未知のtypeです"); break;
        }
    }
    
}
