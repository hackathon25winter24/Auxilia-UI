using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "Scriptable Objects/RoomData")]
public class RoomData : ScriptableObject
{
    public int room_id;
    public string room_name;
    public int room_my_index;
    public UsersData[] usersData;
}

[System.Serializable]
public class UsersData
{
    public string user_name;
    public int user_rate;
    public bool is_host;
    public int user_state;
    // -1:いない、0:観戦者、1:1P、2:2P
    public bool is_ready;
}