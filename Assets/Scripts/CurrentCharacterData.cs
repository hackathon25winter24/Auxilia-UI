using UnityEngine;
using System;

public class CurrentCharacterData: MonoBehaviour
{
    [SerializeField] int _character_id_ondata;
    [SerializeField] int _current_hp;
    [SerializeField] int _current_move_cost;
    [SerializeField] Vector2Int _current_position;
    [SerializeField] bool _is_poison;
    [SerializeField] bool _is_hemorrhage;
    [SerializeField] bool _is_paralyzed;
    [SerializeField] CharacterData characterData;
    private CharactersData my_data;
    private int max_hp;
    private int default_move_cost;
    public event Action<Vector2Int> OnPositionChanged;


    public int character_id_ondata
    {
        get{return _character_id_ondata;}
        private set{_character_id_ondata = value;}
    }
    public int current_hp
    {
        get{return _current_hp;}
        private set{_current_hp = value;}
    }
    public int current_move_cost
    {
        get{return _current_move_cost;}
        private set{_current_move_cost = value;}
    }
    public Vector2Int current_position
    {
        get{return _current_position;}
        private set{_current_position = value;}
    }
    public bool is_poison
    {
        get{return _is_poison;}
        private set{_is_poison = value;}
    }
    public bool is_hemorrhage
    {
        get{return _is_hemorrhage;}
        private set{_is_hemorrhage = value;}
    }
    public bool is_paralyzed
    {
        get{return _is_paralyzed;}
        private set{_is_paralyzed = value;}
    }


    public void SetId(int id)
    {
        _character_id_ondata = id;
    }
    public void SetDefaultParameter()
    {
        my_data = characterData.characters[_character_id_ondata];
        max_hp = my_data.default_hp;
        default_move_cost = my_data.default_move_cost;
        _current_hp = max_hp;
        _current_move_cost = default_move_cost;
    }
    public void Damage(int damage)
    {
        _current_hp = Mathf.Max(_current_hp - damage, 0);
        Debug.Log($"{my_data.default_name}は{damage}のダメージ。残りHPは{_current_hp}");
    }
    public void Heal(int heal)
    {
        _current_hp = Mathf.Min(_current_hp + heal, max_hp);
        Debug.Log($"{my_data.default_name}は{heal}の回復。残りHPは{_current_hp}");
    }
    public void SetPosition(Vector2Int position)
    {
        _current_position = position;
        //データの初期位置設定に使用。各キャラは元から初期位置に配置されている想定のため移動処理は行わない。
        
    }
    public void MovePosition(Vector2Int direction)
    {
        _current_position += direction;
        OnPositionChanged?.Invoke(_current_position);
    }
    public void SpawnCharacter(int id, Vector2Int position)
    {
        Debug.Log("SpawnCharacterがよばれた");
        SetId(id);
        Debug.Log("IDがセットされた");
        SetDefaultParameter();
        Debug.Log("デフォルトパラメーターがセットされた");
        SetPosition(position);
    }
}
