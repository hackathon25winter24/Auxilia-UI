using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridViewManager : MonoBehaviour
{
    [SerializeField] ConstGridData constGridData; 
    public event EventHandler GridSet;
    void Start()
    {
        SetGrids(8, 5);
    }
    public void SetGrids(int x, int y) // グリッドの生成
    {
        for(int i = 0; i < y; i++)
        {
            for(int j = 0; j < x; i++)
            {
                Instantiate(constGridData.grids[0].grid, new Vector3(-175 + j * 50, 30 - i * 50, 0), Quaternion.identity);
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
