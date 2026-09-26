using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridViewManager : MonoBehaviour
{
    public GameObject grid;
    public List<List<Image>> grids_image;
    void Start()
    {
        
    }
    public void SetGrids() // グリッドの生成
    {
        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 8; i++)
            {
                GameObject mono_grid = Instantiate(grid, new Vector3(-175 + j * 50, 30 - i * 50, 0), Quaternion.identity);
                grids_image[j][i] = mono_grid.GetComponent<Image>();
            }
        }
    }
    public void ShowGrid() // グリッドの表示の更新
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
