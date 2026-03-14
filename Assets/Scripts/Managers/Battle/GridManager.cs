using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GridDataforOnline gridDataforOnline;
    public GridDataforLocal gridDataforLocal;
    public Image[] grids; 

    public Sprite NormalGrid;
    public Sprite ProhibitGrid;
    public Sprite BaseGrid;
    public Sprite CharacterGrid;
    public Sprite AttackGrid;

    void Awake()
    {
        // 全グリッドの初期化
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                gridDataforOnline.grid_state_y[y].grid_state_x[x] = 0;
                gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] = 0;
            }
        }

        for (int i = 0; i < 40; i++)
        {
            gridDataforLocal.grid_character_position[i] = 0;
            gridDataforLocal.grid_attack_position[i] = 0;
        }
        
        // 特殊なグリッドの設定
        SetInitialGridState(1, 1, -2);
        SetInitialGridState(5, 1, -2);
        SetInitialGridState(0, 2, 1);
        SetInitialGridState(7, 2, 1);
        SetInitialGridState(2, 3, -2);
        SetInitialGridState(6, 3, -2);
        SetInitialGridState(0, 0, -1);
        SetInitialGridState(1, 2, -1);
        SetInitialGridState(0, 4, -1);
        SetInitialGridState(7, 0, -1);
        SetInitialGridState(6, 2, -1);
        SetInitialGridState(7, 4, -1);
    }

    // コードをスッキリさせるための補助関数
    void SetInitialGridState(int x, int y, int state) {
        gridDataforOnline.grid_state_y[y].grid_state_x[x] = state;
    }

    void Start()
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++) // ここが i++ になっていたのを修正
            {
                SetGridVisual(x, y);
                if (gridDataforOnline.grid_state_y[y].grid_state_x[x] != -1)
                {
                    gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] =
                    gridDataforOnline.grid_state_y[y].grid_state_x[x];
                }
            }
        }
    }

    public void SetGridVisual(int x, int y)
    {
        // ガード句：範囲外アクセス防止
        if (x < 0 || x >= 8 || y < 0 || y >= 5) return;

        // 1次元配列用のインデックス計算 (重要！)
        int grid_index = y * 8 + x; 

        // 1. 基本地形の反映
        switch (gridDataforOnline.grid_state_y[y].grid_state_x[x])
        {
            case 1:  grids[grid_index].sprite = BaseGrid; break;
            case -2: grids[grid_index].sprite = ProhibitGrid; break;
            case -1: grids[grid_index].sprite = NormalGrid; break;
            default: grids[grid_index].sprite = NormalGrid; break;
        }

        // 2. キャラクター位置の反映 (上書き)
        if (gridDataforLocal.grid_character_position[grid_index] == 1)
        {
            grids[grid_index].sprite = CharacterGrid;
        }

        // 3. 攻撃範囲の反映
        if (gridDataforLocal.grid_attack_position[grid_index] == 1)
        {
            // 地形が 0 (通常) の時だけ攻撃色にする
            if (gridDataforOnline.grid_state_y[y].grid_state_x[x] == 0)
            {
                grids[grid_index].sprite = AttackGrid;
            }
            if (gridDataforOnline.grid_state_y[y].grid_state_x[x] == -1)
            {
                grids[grid_index].sprite = AttackGrid;
            }
        }
    }
    
    void Update()
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                SetGridVisual(x, y);
            }
        }
    }
}