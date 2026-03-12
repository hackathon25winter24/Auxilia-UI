using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GridDataforOnline gridDataforOnline;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        gridDataforOnline.grid_state[9] = -1;
        gridDataforOnline.grid_state[13] = -1;
        gridDataforOnline.grid_state[26] = -1;
        gridDataforOnline.grid_state[30] = -1;
    }

    void Start()
    {
        for (int i = 0; i < 41; i++)
        {
            if (gridDataforOnline.grid_state[i] != 0)
            {
                SetGrids(i);
            }
        }
    }

    public void SetGrids(int grig_number)
    {
        switch (gridDataforOnline.grid_state[grig_number])
        {
            case -1:
                Debug.Log("移動不能マス: ");
                break;
            default:
                Debug.Log("不明なボタン: ");
                break;
        }
    }
}
