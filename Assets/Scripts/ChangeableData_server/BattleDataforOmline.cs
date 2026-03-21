using UnityEngine;

[CreateAssetMenu(fileName = "BattleDataforOmline", menuName = "Scriptable Objects/BattleDataforOmline")]
public class BattleDataforOmline : ScriptableObject
{
    public CharactersBattleData[] charactersBattleDatas;
    public int[] selected_character;
    public int now_my_cost;
    public int now_enemy_cost;
    public bool[] character_isSelected;
    public int my_player_id;
    public bool game_end;
    public string win_player_id;
    public int all_move_cost;
    public int palyer1_cost;
    public int palyer2_cost;
    public string opponent_name;
    public string player1_name;
    public string player2_name;

    public int now_moving_player;
    public bool isPlayer;
    public int opponent_rate;
    public int rate;
    public int my_rate_updown;
    public int opponent_rate_updown;
    public int base_hp;
    public int opponent_base_hp;
    public Vector2Int base_position;
    public Vector2Int opponent_base_position;
    public int now_turn;
}

[System.Serializable]
public class CharactersBattleData
{
    public uint unique_id;
    public bool[] debuffs = new bool[8];
    public int now_character_hp;
    public int now_character_maxhp;
    public int now_character_move_cost;
    public Vector2Int now_character_position;
}