using UnityEngine;

[CreateAssetMenu(fileName = "GridDataforOnline", menuName = "Scriptable Objects/GridDataforOnline")]
public class GridDataforOnline : ScriptableObject
{
    public int[] grid_state = new int[40];
    public int[] sub_grid_state = new int[40];
}
