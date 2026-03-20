using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GridDataforOnline gridDataforOnline;
    public BattleDataforOmline battleDataforOnline;
    public Image[] grids; 
    public Sprite NormalGrid;
    public Sprite ProhibitGrid;
    public Sprite BaseGrid;
    public Sprite CharacterGrid;
    public Sprite AttackGrid;
    public Sprite MakibishiGrid;
    public Sprite LandmineGrid;

    public GameConnector gameConnector;
    public RoomData roomData;
    public PlayerData playerData;

    // デバッグ用：前フレームのグリッド状態キャッシュ（変化検知用）
    private int[,] _prevGridState = new int[5, 8];

    private T GetSo<T>(T existing) where T : ScriptableObject
    {
        if (existing != null) return existing;
        var targets = Resources.FindObjectsOfTypeAll<T>();
        if (targets.Length > 0) return targets[0];
        return null;
    }

    void Awake()
    {
        roomData = GetSo(roomData);
        playerData = GetSo(playerData);
        gameConnector = FindFirstObjectByType<GameConnector>();

        // 全グリッドの初期化
        for (int y = 0; y < 5; y++)
        {
            if (gridDataforOnline.grid_state_y[y] == null)
            gridDataforOnline.grid_state_y[y] = new Grid_state_y();

            if (gridDataforOnline.sub_grid_state_y[y] == null)
            gridDataforOnline.sub_grid_state_y[y] = new Sub_grid_state_y();

            if (gridDataforOnline.grid_attack_position_y[y] == null)
            gridDataforOnline.grid_attack_position_y[y] = new Grid_attack_position_y();

            for (int x = 0; x < 8; x++)
            {
                gridDataforOnline.grid_state_y[y].grid_state_x[x] = 0;
                gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] = 0;
                gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] = 0;
            }
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
            for (int x = 0; x < 8; x++)
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
            case 3: grids[grid_index].sprite = MakibishiGrid; break;
            case 4: grids[grid_index].sprite = LandmineGrid; break;
            default: grids[grid_index].sprite = NormalGrid; break;
        }

        // 2. キャラクター位置の反映 (上書き)
        for(int i = 0; i <= 5; i++)
        {
        if (gridDataforOnline.grid_state_y[y].grid_state_x[x] == -1 && battleDataforOnline.character_isSelected[i])
        {
            grids[battleDataforOnline.charactersBattleDatas[i].now_character_position.x + battleDataforOnline.charactersBattleDatas[i].now_character_position.y * 8].sprite = CharacterGrid;
        }
        }

        // 3. 攻撃範囲の反映
        if (gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] == 1)
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
        bool changed = false;
        var logSb = new System.Text.StringBuilder();

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                int current = gridDataforOnline.grid_state_y[y].grid_state_x[x];

                // 前フレームから変化があったマスだけログ記録
                if (current != _prevGridState[y, x])
                {
                    changed = true;
                    string spriteName = current switch
                    {
                        1    => "BaseGrid",
                        -2   => "ProhibitGrid",
                        -1   => "NormalGrid(Char)",
                        3    => "MakibishiGrid",
                        4    => "LandmineGrid",
                        _    => "NormalGrid"
                    };
                    logSb.AppendLine($"  [{x},{y}] {_prevGridState[y, x]} -> {current} ({spriteName})");
                    _prevGridState[y, x] = current;
                }

                SetGridVisual(x, y);
            }
        }

        if (changed)
        {
            Debug.Log($"<color=lime>[GridManager] グリッド変化検知:\n{logSb}</color>");
            
            // サーバーに全グリッドデータを送信
            if (gameConnector != null && roomData != null && playerData != null)
            {
                bool is1p = (battleDataforOnline.my_player_id == 0);
                _ = gameConnector.SendGridUpdate(roomData.room_id, playerData.user_id, gridDataforOnline, is1p);
            }
        }
    }

    public void SyncPrevGridState()
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                _prevGridState[y, x] = gridDataforOnline.grid_state_y[y].grid_state_x[x];
            }
        }
    }
}