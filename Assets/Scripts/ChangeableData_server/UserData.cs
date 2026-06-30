using UnityEngine;

[CreateAssetMenu(fileName = "UserData", menuName = "Scriptable Objects/UserData")]
public class UserData : ScriptableObject
{
    public string user_id;
    public string user_name;
    public string password;// DBではハッシュ化されて保存
    public int story_progress;
    public int num_wins;
    public int num_battles;
    public int rate;
    public int home_character_id;
    public int deck1;
    public int deck2;
    public int deck3;
}
