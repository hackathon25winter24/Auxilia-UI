using System;
using System.Collections;
using System.Diagnostics;
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
    public void Initialize(BattleDataForOnline battleData)
    {
        battleDataForOnline = battleData;
    }
    public void ChangeView(string type)
    {
        switch(type)
        {
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
