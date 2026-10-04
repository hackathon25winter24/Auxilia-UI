using System;
using UnityEngine;

public class GridViewManager : MonoBehaviour
{
    [SerializeField] ConstGridData constGridData; 
    [SerializeField] BattleDataForOnline battleDataForOnline;
    [SerializeField] private Transform canvas;
    [SerializeField] private ViewManager viewManager;
    public event Action<Vector2> delete_grid;
    public event Action<bool> delete_all_grid;
    void Start()
    {
        InitializeGrid(8, 5); // 一旦8*5のグリッドの生成をするようにする
        viewManager.grid_changed += ShowGrid;
    }
    public void ShowGrid()
    {
        // battleDataForOnlineに変更があるグリッドが書かれるのでそれを参照してグリッドの変更を行う
        while(battleDataForOnline.uniqueGrids.Count != 0)
        {
            ShowOneGrid(battleDataForOnline.uniqueGrids[0].position, battleDataForOnline.uniqueGrids[0].gridType);
            battleDataForOnline.uniqueGrids.RemoveAt(0);
        }
    }
    public void InitializeGrid(int x, int y) // グリッドの初期化
    {
        delete_all_grid?.Invoke();
        for(int i = 0; i < y; i++)
        {
            for(int j = 0; j < x; j++)
            {
                SetGrids(new Vector2(j, i), 0);
            }
        }
    }
    public void SetGrids(Vector2 position, int grid_id) // グリッドの生成
    {
        if(0 <= grid_id && grid_id < constGridData.grids.Length)
        {
            GameObject gridObj = Instantiate(constGridData.grids[grid_id].grid, canvas);
            RectTransform rectTransform = gridObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(-175f + position.x * 50f, 60f - position.y * 50f);
                rectTransform.localScale = Vector3.one;
                gridObj.transform.SetSiblingIndex(2);
            }
            Grid grid = gridObj.GetComponent<Grid>();
            if (grid != null)
            {
                grid.Initialize(this);
            }
            grid.position = position;
        }
    }
    public void ShowOneGrid(Vector2 grid_position, int grid_id) // 一つのグリッドの更新
    {
        delete_grid?.Invoke(grid_position);
        SetGrids(grid_position, grid_id);
    }
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        viewManager.grid_changed -= ShowGrid;
    }
}
