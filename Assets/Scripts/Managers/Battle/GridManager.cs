using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GridDataforOnline gridDataforOnline;
    public BattleDataForOnline battleDataForOnline;
    public Image[] grids; 
    public Sprite NormalGrid;
    public Sprite ProhibitGrid;
    public Sprite BaseGrid;
    public Sprite CharacterGrid;
    public Sprite AttackGrid;
    public Sprite MakibishiGrid;
    public Sprite LandmineGrid;

    public RoomData roomData;
    public UserData userData;

    private PlayerState self;
    private PlayerState opponent;

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
        userData = GetSo(userData);

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
        bool is_1p = (userData.user_id == battleDataForOnline.player1.player_id);
        self     = (is_1p) ? battleDataForOnline.player1 : battleDataForOnline.player2;
        opponent = (is_1p) ? battleDataForOnline.player2 : battleDataForOnline.player1;
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
        for(int i = 0; i < 3; i++)
        {
            // 自分がキャラを選択している時
        if (gridDataforOnline.grid_state_y[y].grid_state_x[x] == -1 && self.characters[i].character_isSelected)
        {
            grids[self.characters[i].now_character_position.x + self.characters[i].now_character_position.y * 8].sprite = CharacterGrid;
        }
        if (gridDataforOnline.grid_state_y[y].grid_state_x[x] == -1 && opponent.characters[i].character_isSelected)
        {
            grids[opponent.characters[i].now_character_position.x + opponent.characters[i].now_character_position.y * 8].sprite = CharacterGrid;
        }
        }

        // 3. 攻撃範囲の反映
        if (gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] == 1)
        {
            // 地形が 0 (通常) 、-1（プレイヤー）の時だけ攻撃色にする
            int 撒菱や拠点の足元は赤くならないということかな = 0;
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
    }

/*  Updateでやってる処理だと思うのでおそらくいらないはず。問題なければ削除してOK
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
    */

    public Vector2Int ConvertCoordinateForServer(int x, int y, bool is1p)// 1p2pで反転させたグリッド座標を返す
    {
        if (is1p) return new Vector2Int(x, y);
        return new Vector2Int(7 - x, y);
    }
}