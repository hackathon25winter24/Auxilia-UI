using System;
using UnityEngine;

public class GridViewManager : MonoBehaviour
{
    [SerializeField] ConstGridData constGridData; 
    [SerializeField] BattleDataForOnline battleDataForOnline;
    [SerializeField] private Transform canvas;
    public event Action<Vector2> grid_set;
    void Start()
    {
        // 一旦8*5のグリッドの生成をするようにする
        InitializeGrid(8, 5);
    }
    void Update()
    {
        // battleDataForOnlineに変更があるグリッドが書かれるのでそれを参照してグリッドの変更を行う
        if(battleDataForOnline.uniqueGrids.Count != 0)
        {
            SetGrids(battleDataForOnline.uniqueGrids[0].position, battleDataForOnline.uniqueGrids[0].gridType);
            battleDataForOnline.uniqueGrids.RemoveAt(0);
        }
    }
    public void InitializeGrid(int x, int y) // グリッドの初期化
    {
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
    public void ShowGrid(Vector2 grid_position, int grid_id) // グリッドの更新
    {
        grid_set?.Invoke(grid_position);
        SetGrids(grid_position, grid_id);
    }
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        if (NetworkManager.Instance != null)
        {
            
        }
    }
}
