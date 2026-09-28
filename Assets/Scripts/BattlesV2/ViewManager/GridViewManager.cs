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
        
    }
    public void SetGrids() // グリッドの生成
    {
        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 8; i++)
            {
                GameObject mono_grid = Instantiate(constGridData.grids[0].grid, new Vector3(-175 + j * 50, 30 - i * 50, 0), Quaternion.identity);
            }
        }
    }
    public void ShowGrid(Vector2 reset_grid) // グリッドの表示の更新
    {
        for(int i = 0; i < 40; i++)
        {
            // グリッドの見た目の変更を書く
        }
    }
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        if (NetworkManager.Instance != null)
        {
            
        }
    }
}
