using UnityEngine;

[CreateAssetMenu(fileName = "BattleDataforOmline", menuName = "Scriptable Objects/BattleDataforOmline")]
public class BattleDataforOmline : ScriptableObject
{
    [SerializeField] CharactersBattleData[] charactersBattleDatas;
}

[System.Serializable]
public class CharactersBattleData
{
    public int now_character_hp;
    public int now_character_maxhp;
    public int now_character_atk;
    public bool is_attack_up;
    public bool is_speed_up;
    public bool is_attackcost_down;
    public bool is_poison;
    public bool is_paralysis;
    public bool is_speed_down;
    public bool is_attackcost_up;
    public bool is_bleeding;
}
