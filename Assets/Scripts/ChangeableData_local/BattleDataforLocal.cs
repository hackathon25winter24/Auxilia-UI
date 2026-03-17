using UnityEngine;

[CreateAssetMenu(fileName = "BattleDataforLocal", menuName = "Scriptable Objects/BattleDataforLocal")]
public class BattleDataforLocal : ScriptableObject
{
    public bool is_myturn;
    public int[] character_id;

    public CharactersBattleDataLocal[] charactersBattleDatasLocal;
    public int now_my_cost;
    public int now_enemy_cost;
    public string enemy_name;
    public int all_move_cost;
    public bool game_end;
    public bool is_win;
}

[System.Serializable]
public class CharactersBattleDataLocal
{
    public int now_character_hp;
    public int now_character_maxhp;
    public int now_character_move_cost;
    public bool is_attack_up;
    public bool is_speed_up;
    public bool is_attackcost_down;
    public bool is_poison;
    public bool is_paralysis;
    public bool is_speed_down;
    public bool is_attackcost_up;
    public bool is_bleeding;
    public Vector2Int now_character_position;
}