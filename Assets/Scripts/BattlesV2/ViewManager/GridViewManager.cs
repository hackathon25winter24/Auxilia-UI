using System;
using UnityEngine;
using System.Collections.Generic;

public class GridViewManager : MonoBehaviour
{
    [SerializeField] ConstGridData constGridData; 
    [SerializeField] BattleDataForOnline battleDataForOnline;
    [SerializeField] private Transform canvas;
    public event Action<Vector2> delete_grid;
    void Start()
    {
        // 一旦8*5のグリッドの生成をするようにする
        InitializeGrid(8, 5);
    }
    public void ChangeView(BattleDataForOnline data, UserData user)
    {
        battleDataForOnline = data;
        bool flip = data.State.Players.Count > 1 && user != null && data.State.Players[1].Id == user.user_id;
        var desired = new Dictionary<Vector2Int, int>();
        foreach (var cell in data.uniqueGrids)
        {
            ShowGrid(battleDataForOnline.uniqueGrids[0].position, battleDataForOnline.uniqueGrids[0].gridType);
            battleDataForOnline.uniqueGrids.RemoveAt(0);
        }
        foreach (var b in data.State.Bases)
            desired[new Vector2Int(flip ? 7 - b.Position.X : b.Position.X, b.Position.Y)] = 2;
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 8; x++)
            {
                var p = new Vector2Int(x, y);
                int type = desired.TryGetValue(p, out var value) ? value : 0;
                if (!displayedGrids.TryGetValue(p, out var old) || old != type) ShowGrid(p, type);
            }
    }
    public void InitializeGrid(int x, int y) // グリッドの初期化
    {
        for(int i = 0; i < y; i++)
        {
            for(int j = 0; j < x; j++)
            {
                if (!displayedGrids.ContainsKey(new Vector2Int(j, i))) SetGrids(new Vector2(j, i), 0);
            }
        }
    }
    public void SetGrids(Vector2 position, int grid_id) // グリッドの生成
    {
        if(constGridData != null && canvas != null && 0 <= grid_id && grid_id < constGridData.grids.Length)
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
                grid.position = position;
            }
            displayedGrids[Vector2Int.RoundToInt(position)] = grid_id;
        }
    }
    public void ShowGrid(Vector2 grid_position, int grid_id) // グリッドの更新
    {
        delete_grid?.Invoke(grid_position);
        SetGrids(grid_position, grid_id);
    }
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        if (NetworkManager.Instance != null)
        {
            
        }
    }
}
