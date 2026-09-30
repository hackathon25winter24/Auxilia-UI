using System;

public class ViewManager
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
    public void ChangeView()
    {}
    
}
