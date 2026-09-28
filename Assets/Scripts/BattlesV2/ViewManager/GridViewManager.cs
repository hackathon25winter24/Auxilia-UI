using System;
using UnityEngine;

public class GridViewManager : MonoBehaviour
{
    [SerializeField] ConstGridData constGridData; 
    [SerializeField] private Transform canvas;
    public event EventHandler GridSet;
    void Start()
    {
        SetGrids(8, 5);
    }
    public void SetGrids(int x, int y) // グリッドの生成
    {
        for(int i = 0; i < y; i++)
        {
            for(int j = 0; j < x; j++)
            {
                GameObject gridObj = Instantiate(constGridData.grids[0].grid, canvas);
                RectTransform rectTransform = gridObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(-175f + j * 50f, 60f - i * 50f);
                    rectTransform.localScale = Vector3.one;
                }
            }
        }
    }
    public void ShowGrid(Vector2 reset_grid, int grid_id) // グリッドの表示の更新
    {
        // グリッドの見た目の変更を書く
    }
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        if (NetworkManager.Instance != null)
        {
            
        }
    }
}
