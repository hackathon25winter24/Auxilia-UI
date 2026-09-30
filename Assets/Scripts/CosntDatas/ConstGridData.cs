using UnityEngine;

[CreateAssetMenu(fileName = "ConstGridData", menuName = "Scriptable Objects/ConstGridData")]
public class ConstGridData : ScriptableObject
{
    [SerializeField] GridData[] _grids;

    public GridData[] grids
    {
        get{return _grids;}
    }
}

[System.Serializable]
public class GridData
{
    [SerializeField] string _grid_name;
    public string grid_name
    {
        get{return _grid_name;}
    }
    [SerializeField] GameObject _grid;
    public GameObject grid
    {
        get{return _grid;}
    }
    [SerializeField] string _grid_detail;
    public string grid_detail
    {
        get{return _grid_detail;}
    }
}