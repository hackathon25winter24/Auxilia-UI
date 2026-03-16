using UnityEngine;

[CreateAssetMenu(fileName = "MatchingData", menuName = "Scriptable Objects/MatchingData")]
public class MatchingData : ScriptableObject
{
    public int num_room;
    public bool[] room_is_selected;
    public int num_room_joiner;
    
    [SerializeField] JoinnersData[] _joinners;

    public JoinnersData[] joinners
    {
        get{return _joinners;}
    }
}

[System.Serializable]
public class JoinnersData
{
    public string name;
    public int state;
    public int rate;
}