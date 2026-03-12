using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GridDataforOnline gridDataforOnline;
    public Image[] grids; 

    public Sprite NormalGrid;
    public Sprite ProhibitGrid;
    public Sprite BaseGrid;

    void Awake()
    {
        for (int i = 0; i < gridDataforOnline.grid_state.Length; i++)
        {
            gridDataforOnline.grid_state[i] = 0;
        }
        gridDataforOnline.grid_state[9] = -2;
        gridDataforOnline.grid_state[13] = -2;
        gridDataforOnline.grid_state[16] = 1;
        gridDataforOnline.grid_state[23] = 1;
        gridDataforOnline.grid_state[26] = -2;
        gridDataforOnline.grid_state[30] = -2;
    }

    void Start()
    {
        // データの数（grid_stateの長さ）に合わせてループ
        for (int i = 0; i < gridDataforOnline.grid_state.Length; i++)
        {
            // インデックスが grids 配列の範囲内かチェック（安全策）
            if (i < grids.Length)
            {
                SetGridVisual(i);
            }
        }
    }

    public void SetGridVisual(int grid_index)
    {
        // grid_stateの内容を見て、見た目を切り替える
        switch (gridDataforOnline.grid_state[grid_index])
        {
            case 1:
                grids[grid_index].sprite = BaseGrid;
                break;
            case -2:
                grids[grid_index].sprite = ProhibitGrid;
                break;
            case -1:
                grids[grid_index].sprite = NormalGrid;
                break;
            default:
                grids[grid_index].sprite = NormalGrid;
                break;
        }
    }
}