using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TutorialCharacter : MonoBehaviour
{
    [Header("自分自身の情報")]
    public RectTransform myTransform;
    public bool is_selected;

    [Header("データ参照")]
    public InputData inputData;
    public GridDataforOnline gridDataforOnline;
    public CharacterData characterData;

    public BattleDataForOnline battleDataForTutorial;
    public StoryManagerData storyManagerData;
    public TutorialBattleManager tutorialBattleManager;

    [Header("共有UI・共通参照")]
    public RectTransform attackButton;
    public Image attackButtonBackImage;
    public Image[] attackButtonOne;
    public RectTransform BackButton;

    [Header("自身の状態ステータス")]
    public int on_grid_number_x;
    public int on_grid_number_y;
    public int attack_number;
    public bool is_attacking;

    void Start()
{
    battleDataForTutorial = tutorialBattleManager.battleDataForTutorial;// 同じbattleDataForTutorialを取得する
    attackButton.gameObject.SetActive(false);
    is_selected = false;
    battleDataForTutorial.player1.characters[1].character_isSelected = false;
    
    // 1. グリッドの初期位置をセット (x=1, y=2)
    on_grid_number_x = 1;
    on_grid_number_y = 2;

    // 2. 見た目の位置をグリッドに合わせる (手動入力ではなく計算させる)
    SyncUIPositionFromGrid();

    myTransform.localScale = new Vector3(-1, 1, 1);
    
    // 3. 自分のいる場所を占有状態にする
    UpdateGridState(on_grid_number_x, on_grid_number_y, -1);
}

    void SyncUIPositionFromGrid()
{
    // 元のロジック: x*50-175, y*-50+30
    float worldPosX = on_grid_number_x * 50 - 175;
    float worldPosY = on_grid_number_y * -50 + 30;
    myTransform.anchoredPosition = new Vector2(worldPosX, worldPosY);
}

    void Update()
    {
        battleDataForTutorial.player1.characters[1].now_character_position.x = on_grid_number_x;
        battleDataForTutorial.player1.characters[1].now_character_position.y = on_grid_number_y;

        // 自分が選択されていない、かつ攻撃中でもないなら何もしない
        if (!is_selected && !is_attacking) return;

        // 自分のターンでないなら移動・攻撃不可
        if (battleDataForTutorial.is_1p_turn == false) 
        {
            return;
        }

        if (is_attacking)
        {
            ClearAttackRange();
            Attack();
            if (inputData.left_mouse_button_ispressed) ConfirmAttack();
        }
        else
        {
            if (storyManagerData.Tutorial_progress > 1)
            {
            if (inputData.up_key_ispressed) TryMove(0, -1, new Vector2(0, 50));
            else if (inputData.down_key_ispressed) TryMove(0, 1, new Vector2(0, -50));
            else if (inputData.right_key_ispressed) TryMove(1, 0, new Vector2(50, 0));
            else if (inputData.left_key_ispressed) TryMove(-1, 0, new Vector2(-50, 0));
            }
        }
    }

    public void OnMySelected() // ボタンクリックの代わりにこれを呼ぶ
    {
        if (battleDataForTutorial.is_1p_turn == false) return;

        is_selected = true;
        battleDataForTutorial.player1.characters[1].character_isSelected = true;
        attackButton.gameObject.SetActive(true);
        BackButton.gameObject.SetActive(true);
        attackButton.anchoredPosition = myTransform.anchoredPosition + new Vector2(100, 0);

        // 攻撃ボタンの見た目更新
        for (int i = 0; i <= 2; i++)
        {
            attackButtonOne[i].sprite = characterData.characters[0].attacks[i].attack_button;
        }
        attackButtonBackImage.sprite = characterData.characters[0].attack_button_backimage;
    }

    void TryMove(int moveX, int moveY, Vector2 posDelta)
{
    // 追加: 連続移動を防ぐため、一度ボタンを離すまで待つか、
    // ここで移動キーを false に書き換える処理を入れるのが理想的です。
    
    int nextX = on_grid_number_x + moveX;
    int nextY = on_grid_number_y + moveY;

    if (nextX < 0 || nextX >= 8 || nextY < 0 || nextY >= 5) return;

    if (battleDataForTutorial.player1.current_cost_remaining - 10 < 0) 
    {
        Debug.Log("コスト不足！");
        return;
    }

    // 移動先が 0 以上（空きマス）なら移動可能
    if (gridDataforOnline.grid_state_y[nextY].grid_state_x[nextX] >= 0)
    {
        UpdateGridState(on_grid_number_x, on_grid_number_y, 0);

        on_grid_number_x = nextX;
        on_grid_number_y = nextY;
        
        // UIの位置を更新
        SyncUIPositionFromGrid();
        
        // 攻撃ボタンも追従
        attackButton.anchoredPosition = myTransform.anchoredPosition + new Vector2(100, 0);

        UpdateGridState(on_grid_number_x, on_grid_number_y, -1);

        battleDataForTutorial.player1.current_cost_remaining -= 10;
        
        // 【重要】移動したら一旦フラグを折る（inputData側で制御していない場合）
        // inputData.up_key_ispressed = false; // 必要に応じて
    }
}

    // 補助関数群（内容は元のコードと同様のため、変数名を myID 向けに調整して使用）
    void UpdateGridState(int x, int y, int state)
    {
    if (state == -1)
    {
        gridDataforOnline.grid_state_y[y].grid_state_x[x] = -1;
    }
    else
    {
        // キャラが去った後は、保存しておいた元の地形(sub_grid_state)を復元する
        gridDataforOnline.grid_state_y[y].grid_state_x[x] = 
        gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x];
    }
    }

    void Attack()
    {
        int currentX = on_grid_number_x;
        int currentY = on_grid_number_y;
        var ranges = characterData.characters[0].attacks[attack_number].default_attack_range;

        // マウスの方向を取得 (Vector2Int.up, down, left, right のいずれかが返る)
        Vector2Int direction = GetMouseDirection();

        foreach (Vector2Int range in ranges)
        {
            // 方向に基づいて攻撃範囲を回転/反転させる計算
            // デフォルトの範囲データが「右向き(X+)」を正としていると仮定した場合の計算例
            Vector2Int rotatedRange = RotateRange(range, direction);

            int targetX = currentX + rotatedRange.x;
            int targetY = currentY + rotatedRange.y;

            if (targetX >= 0 && targetX < 8 && targetY >= 0 && targetY < 5)
            {
                gridDataforOnline.grid_attack_position_y[targetY].grid_attack_position_x[targetX] = 1;
            }
        }
    }

    // 回転ロジック
    private Vector2Int RotateRange(Vector2Int range, Vector2Int dir)
    {
        if (dir == Vector2Int.right) return range; 
        if (dir == Vector2Int.left)  return new Vector2Int(-range.x, -range.y); 
    
        // 上下方向：XとYを入れ替える
        // グリッド座標系で「上」が Y-1 の場合は符号に注意
        if (dir == Vector2Int.up)    return new Vector2Int(range.y, -range.x); 
        if (dir == Vector2Int.down)  return new Vector2Int(-range.y, range.x); 

        return range;
    }

    private Vector2Int GetMouseDirection()
    {
        // キャラクターのUI座標とマウスの座標（中心原点）の差分を取る
        Vector2 charPos = myTransform.anchoredPosition;
        Vector2 mousePos = inputData.mouse_position;
        Vector2 diff = mousePos - charPos;

        // XとYの絶対値を比較して、どちらの方向に大きく動いているか判定
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            // 左右方向
            return diff.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            // 上下方向
            // UIの座標系に合わせる（上がプラスならup、下がマイナスならdown）
            return diff.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }

    void ConfirmAttack()
    {
        int cost = characterData.characters[0].attacks[attack_number].default_attack_cost;
        // 1. 現在の攻撃の威力を取得
        int power = characterData.characters[0].attacks[attack_number].default_attack_power;

        if(inputData.left_mouse_button_ispressed)
        {
        // 敵がいるマスの攻撃フラグが 1 ならヒット！
        if (gridDataforOnline.grid_attack_position_y[2].grid_attack_position_x[7] == 1)
        {
            battleDataForTutorial.player2.base_hp -= power;
            battleDataForTutorial.player1.current_cost_remaining -= cost;
            is_attacking = false;
            is_selected = false;
            ClearAttackRange();
        }else
        {
            is_attacking = false;
            is_selected = false;
            ClearAttackRange();
        }
        }
        if(inputData.right_mouse_button_ispressed)
        {
            is_attacking = false;
            is_selected = false;
            ClearAttackRange();
        }
    }

    public void ClearAttackRange()
    {
        // 全グリッド（縦5 × 横8）の攻撃範囲表示をリセット
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] = 0;
            }
        }
    }

    public void OnButtonClick(int buttonName)
    {
        if(buttonName == 0)
        {
            is_selected = false;
            battleDataForTutorial.player1.characters[1].character_isSelected = false;
            attackButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
        }
        if(storyManagerData.Tutorial_progress > 3)
        {
        if(buttonName == 1) 
        {
            attack_number = 0;
            if (battleDataForTutorial.player1.current_cost_remaining - characterData.characters[0].attacks[attack_number].default_attack_cost <0)return;
            attackButton.gameObject.SetActive(false);
            is_attacking = true;
        }
        if(buttonName == 2) 
        {
            Debug.Log("attack2");
            attack_number = 1;
            if (battleDataForTutorial.player1.current_cost_remaining - characterData.characters[0].attacks[attack_number].default_attack_cost <0)return;
            attackButton.gameObject.SetActive(false);
            is_attacking = true;
        }
        if(buttonName == 3) 
        {
            Debug.Log("attack3");
            attack_number = 2;
            if (battleDataForTutorial.player1.current_cost_remaining - characterData.characters[0].attacks[attack_number].default_attack_cost <0)return;
            attackButton.gameObject.SetActive(false);
            is_attacking = true;
        }
        }
    }
}