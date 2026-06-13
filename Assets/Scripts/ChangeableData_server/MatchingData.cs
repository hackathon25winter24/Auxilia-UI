using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MatchingData", menuName = "Scriptable Objects/MatchingData")]
public class MatchingData : ScriptableObject
{
    public int num_room;
    public int selected_room_id;

    public List<RoomsData> rooms = new List<RoomsData>();
}

[System.Serializable]
public class RoomsData
{
    public int num_room_joiner;
    public int room_id;
    public string room_name;
    public string owner_id;
    public string owner_name;
    public bool room_is_gamestarted;
}