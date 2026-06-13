using UnityEngine;

[CreateAssetMenu(fileName = "GridDataforOnline", menuName = "Scriptable Objects/GridDataforOnline")]
public class GridDataforOnline : ScriptableObject
{
    [SerializeField] Grid_state_y[] _grid_state_y = new Grid_state_y[5]; // 例えば5行
    public Grid_state_y[] grid_state_y => _grid_state_y;

    // 座標(x, y)を直接指定して値を取れる関数（ショートカット）
    public int GetState(int x, int y)
    {
        if (y < 0 || y >= _grid_state_y.Length) return -1;
        if (x < 0 || x >= _grid_state_y[y].grid_state_x.Length) return -1;
        
        return _grid_state_y[y].grid_state_x[x];
    }

    [SerializeField] Sub_grid_state_y[] _sub_grid_state_y = new Sub_grid_state_y[5]; // 例えば5行
    public Sub_grid_state_y[] sub_grid_state_y => _sub_grid_state_y;

    // 座標(x, y)を直接指定して値を取れる関数（ショートカット）
    public int SubGetState(int x, int y)
    {
        if (y < 0 || y >= _sub_grid_state_y.Length) return -1;
        if (x < 0 || x >= _sub_grid_state_y[y].sub_grid_state_x.Length) return -1;
        
        return _sub_grid_state_y[y].sub_grid_state_x[x];
    }

    [SerializeField] Grid_attack_position_y[] _grid_attack_position_y = new Grid_attack_position_y[5]; // 例えば5行
    public Grid_attack_position_y[] grid_attack_position_y => _grid_attack_position_y;

    // 座標(x, y)を直接指定して値を取れる関数（ショートカット）
    public int GetAttackPosition(int x, int y)
    {
        if (y < 0 || y >= _grid_attack_position_y.Length) return -1;
        if (x < 0 || x >= _grid_attack_position_y[y].grid_attack_position_x.Length) return -1;
        
        return _grid_attack_position_y[y].grid_attack_position_x[x];
    }
}

[System.Serializable]
public class Grid_state_y
{
    public int[] grid_state_x = new int[8]; // 8列
}

[System.Serializable]
public class Sub_grid_state_y
{
    public int[] sub_grid_state_x = new int[8]; // 8列
}

[System.Serializable]
public class Grid_attack_position_y
{
    public int[] grid_attack_position_x = new int[8]; // 8列
}