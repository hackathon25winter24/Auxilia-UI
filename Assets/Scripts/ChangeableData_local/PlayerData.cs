using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string player_name;
    public int player_rate;
    public int home_character_ID;
    public int story_progress;
    public int battle_number;
    public int win_number;
    public string user_id;
    public string username;
    public string password;

    public int character_formation_one;
    public int character_formation_two;
    public int character_formation_three;
}
