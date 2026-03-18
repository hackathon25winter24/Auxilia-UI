using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MatchingData", menuName = "Scriptable Objects/MatchingData")]
public class MatchingData : ScriptableObject
{
    public int num_room;

    public List<RoomsData> rooms = new List<RoomsData>();
}

[System.Serializable]
public class RoomsData
{
    public int room_id;
    public bool room_is_selected;
    public int num_room_joiner;
    public bool room_is_gamestarted;
    public string room_host;
    public string room_name;
    public List<JoinnersData> joinners = new List<JoinnersData>();
}

[System.Serializable]
public class JoinnersData
{
    public string name;
    public int state;
    public int rate;
}