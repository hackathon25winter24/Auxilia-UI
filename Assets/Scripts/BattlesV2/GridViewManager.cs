using System.Collections.Generic;
using UnityEngine;

public class GridViewManager : MonoBehaviour
{
    public GameObject grid;
    public List<GameObject> grids;
    void Start()
    {
        
    }
    public void SetGrids() // グリッドの生成
    {
        for(int i = 0; i < 40; i++)
        {
            Instantiate(grid, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
    public void ShowGrid() // グリッドの表示の更新
    {}
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        if (NetworkManager.Instance != null)
        {
            
        }
    }
}
