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
    [SerializeField] Sprite _default_sprite;
    [SerializeField] Sprite _select_image;
    [SerializeField] Sprite _detail_image;
    [SerializeField] Sprite _default_sprite_mini;
    [SerializeField] Sprite _default_sprite_smallwindow;
    [SerializeField] int _default_hp;
    [SerializeField] int _default_move_cost;
    [SerializeField] Attacks[] _attacks;
    [SerializeField] Sprite _attack_button_backimage;
    [SerializeField] Passive[] passive;

    public string default_name
    {
        get{return _default_name;}
    }

    public int default_id
    {
        get{return _default_id;}
    }
    
    public Sprite default_sprite
    {
        get{return _default_sprite;}
    }

    public Sprite select_image
    {
        get { return _select_image; }
    }

    public Sprite detail_image
    {
        get { return _detail_image; }
    }
    public Sprite default_sprite_mini
    {
        get{return _default_sprite_mini;}
    }

    public Sprite default_sprite_smallwindow
    {
        get{return _default_sprite_smallwindow;}
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

    public Sprite attack_button_backimage
    {
        get{return _attack_button_backimage;}
    }
}



[System.Serializable]
public class Attacks
{
    [SerializeField] string _default_attack_name;
    [SerializeField] int _default_attack_cost;
    [SerializeField] int _default_attack_power;
    [SerializeField] int _default_attack_target;
    public int[] debuffs = new int[8];
    [SerializeField] Sprite _attack_button;
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

    public int default_attack_target
    {
        get{return _default_attack_target;}
    }

    public Sprite attack_button
    {
        get { return _attack_button; }
    }

    public Vector2Int[] default_attack_range
    {
        get{return _default_attack_range;}
    }
}

[System.Serializable]
public class Passive
{
    [SerializeField] int _passive_power;
    [SerializeField] int _passive_target;
    [SerializeField] Vector2Int[] _passive_range;

    public int passive_power
    {
        get{return _passive_power;}
    }
    public int passive_target
    {
        get{return _passive_target;}
    }
    public Vector2Int[] passive_range
    {
        get{return _passive_range;}
    }
}