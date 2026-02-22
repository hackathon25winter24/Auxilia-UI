using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    [SerializeField] CharactersData[] _characters;

    public CharactersData[] characters
    {
        get{return _characters;}
    }
}



[System.Serializable]
public class CharactersData
{
    [SerializeField] string _default_name;
    [SerializeField] int _default_id;
    [SerializeField] int _default_hp;
    [SerializeField] int _default_move_cost;
    [SerializeField] Attacks[] _attacks;

    public string default_name
    {
        get{return _default_name;}
    }

    public int default_id
    {
        get{return _default_id;}
    }

    public int default_hp
    {
        get{return _default_hp;}
    }

    public int default_move_cost
    {
        get{return _default_move_cost;}
    }

    public Attacks[] attacks
    {
        get{return _attacks;}
    }
}



[System.Serializable]
public class Attacks
{
    [SerializeField] string _default_attack_name;
    [SerializeField] int _default_attack_cost;
    [SerializeField] int _default_attack_power;
    [SerializeField] Vector2Int[] _default_attack_range;

    public string default_attack_name
    {
        get{return _default_attack_name;}
    }

    public int default_attack_cost
    {
        get{return _default_attack_cost;}
    }

    public int default_attack_power
    {
        get{return _default_attack_power;}
    }

    public Vector2Int[] default_attack_range
    {
        get{return _default_attack_range;}
    }
}