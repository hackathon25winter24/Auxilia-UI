using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string player_name;
    public int player_rate;
    public int home_character_ID;
    public int story_progress;
}
