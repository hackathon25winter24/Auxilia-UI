using UnityEngine;

[CreateAssetMenu(fileName = "GridDataforLocal", menuName = "Scriptable Objects/GridDataforLocal")]
public class GridDataforLocal : ScriptableObject
{
    [SerializeField] Grid_character_position_y[] _grid_character_position_y = new Grid_character_position_y[5]; // 例えば5行
    public Grid_character_position_y[] grid_character_position_y => _grid_character_position_y;

    // 座標(x, y)を直接指定して値を取れる関数（ショートカット）
    public int GetCharacterPosition(int x, int y)
    {
        if (y < 0 || y >= _grid_character_position_y.Length) return -1;
        if (x < 0 || x >= _grid_character_position_y[y].grid_character_position_x.Length) return-1;
        
        return _grid_character_position_y[y].grid_character_position_x[x];
    }

    [SerializeField] Grid_attack_position_y[] _grid_attack_position_y = new Grid_attack_position_y[5]; // 例えば5行
    public Grid_attack_position_y[] grid_attack_position_y => _grid_attack_position_y;

    // 座標(x, y)を直接指定して値を取れる関数（ショートカット）
    public int GetAttackPosition(int x, int y)
    {
        if (y < 0 || y >= _grid_attack_position_y.Length) return -1;
        if (x < 0 || x >= _grid_attack_position_y[y].grid_attack_position_x.Length) return-1;
        
        return _grid_attack_position_y[y].grid_attack_position_x[x];
    }
    
}

[System.Serializable]
public class Grid_character_position_y
{
    public int[] grid_character_position_x = new int[8]; // 8列
}

[System.Serializable]
public class Grid_attack_position_y
{
    public int[] grid_attack_position_x = new int[8]; // 8列
}
