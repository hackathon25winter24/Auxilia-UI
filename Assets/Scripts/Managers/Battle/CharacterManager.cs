using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    public RectTransform[] characters; 
    public Image[] character_image;
    public BattleDataforLocal battleDataforLocal;
    public InputData inputData;
    public GridDataforOnline gridDataforOnline;
    public CharacterData characterData;
    public BattleDataforOmline battleDataforOnline;
    public int[] on_grid_number;
    public int[] on_grid_number_x;
    public int[] on_grid_number_y;
    public int selected_character_id;
    public RectTransform AttackButton;
    public Image AttackButtonBackImage;
    public Image[] AttackButtonOne;
    public Image[] CharacterSmallwindow;
    public RectTransform BackButton;
    public int attack_number;
    public bool is_attacking;

    void Awake()
    {
        BackButton.gameObject.SetActive(false);
        // 配列が空、または要素が足りない場合の安全策
        if (characters == null || characters.Length < 6)
        {
            Debug.LogError("CharacterManager: characters配列に6つの要素をアサインしてください。");
            return;
        }

        for (int i = 0; i <= 5; i++)
        {
        battleDataforOnline.character_isSelected[i] = false;
        }

        AttackButton.gameObject.SetActive(false);

        // anchoredPositionを使用して、Canvas内の相対座標で配置する
        SetupCharacter(0, new Vector2(-175, 30), 0, 0); // 1人目: (0,0)
        SetupCharacter(1, new Vector2(-125, -70), 1, 2); // 2人目: (1,2) ※17付近
        SetupCharacter(2, new Vector2(-175, -170), 0, 4); // 3人目: (0,4) ※32付近
        SetupCharacter(3, new Vector2(175, 30), 7, 0); // 4人目: (7,0) ※7付近
        SetupCharacter(4, new Vector2(125, -70), 6, 2); // 5人目: (6,2) ※22付近
        SetupCharacter(5, new Vector2(175, -170), 7, 4); // 6人目: (7,4) ※39付近

        for (int i = 0; i <= 5; i++)
        {
        battleDataforOnline.charactersBattleDatas[i].now_character_hp = characterData.characters[battleDataforLocal.character_id[i]].default_hp;
        battleDataforOnline.charactersBattleDatas[i].now_character_maxhp = characterData.characters[battleDataforLocal.character_id[i]].default_hp;
        battleDataforOnline.charactersBattleDatas[i].now_character_move_cost = characterData.characters[battleDataforLocal.character_id[i]].default_move_cost;
        }
        battleDataforOnline.now_my_cost = 50;

        for (int i = 0; i <= 2; i++)
        {
        characters[i].localScale = new Vector3(-1, 1, 1);
        }

        is_attacking = false;
    }

    // 初期配置を簡潔にするための補助関数
    void SetupCharacter(int id, Vector2 pos, int gridX, int gridY)
    {
        characters[id].anchoredPosition = pos;
        character_image[id].sprite = characterData.characters[battleDataforLocal.character_id[id]].default_sprite_mini;
        CharacterSmallwindow[id].sprite = characterData.characters[battleDataforLocal.character_id[id]].default_sprite_smallwindow;
        on_grid_number_x[id] = gridX;
        on_grid_number_y[id] = gridY;
        on_grid_number[id] = gridY * 8 + gridX; // 1次元番号も一応同期
    }

    public void OnButtonClick(string buttonName)
    {
        if (battleDataforLocal.is_myturn == false)return;
        if (buttonName == "1" || buttonName == "2" || buttonName == "3" || buttonName == "BackButton")
        {
        DeselectAll();
        }

        if (battleDataforOnline.character_isSelected[0] || battleDataforOnline.character_isSelected[1] || battleDataforOnline.character_isSelected[2])
        {
            if(buttonName == "BackButton")
            {
            for (int i = 0; i <= 5; i++)
            {
            battleDataforOnline.character_isSelected[i] = false;
            }
            AttackButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
            is_attacking = false;
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] = 0;
                }
            }
            }

        if(buttonName == "Attack1") 
        {
            if (battleDataforOnline.now_my_cost - characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_cost <0)return;
            AttackButton.gameObject.SetActive(false);
            attack_number = 0;
            is_attacking = true;
        }
        if(buttonName == "Attack2") 
        {
            if (battleDataforOnline.now_my_cost - characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_cost <0)return;
            AttackButton.gameObject.SetActive(false);
            attack_number = 1;
            is_attacking = true;
        }
        if(buttonName == "Attack3") 
        {
            if (battleDataforOnline.now_my_cost - characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_cost <0)return;
            AttackButton.gameObject.SetActive(false);
            attack_number = 2;
            is_attacking = true;
        }
        
        }

        if(buttonName == "BackButton")
            {
            for (int i = 0; i <= 5; i++)
            {
            battleDataforOnline.character_isSelected[i] = false;
            }
            AttackButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
            }
        
        switch (buttonName)
        {
            case "1":
            selected_character_id = 0;
            BackButton.gameObject.SetActive(true);
            battleDataforOnline.character_isSelected[selected_character_id] = true;
            AttackButton.gameObject.SetActive(true);
            AttackButton.anchoredPosition = characters[0].anchoredPosition + new Vector2(100, 0); 
            for (int i = 0; i <= 2; i++)
            {
            AttackButtonOne[i].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[i].attack_button;    
            }
            AttackButtonBackImage.sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attack_button_backimage;
                break;
            case "2":
            selected_character_id = 1;
            BackButton.gameObject.SetActive(true);
            battleDataforOnline.character_isSelected[selected_character_id] = true;
            AttackButton.gameObject.SetActive(true);
            AttackButton.anchoredPosition = characters[1].anchoredPosition + new Vector2(100, 0); 
            for (int i = 0; i <= 2; i++)
            {
            AttackButtonOne[i].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[i].attack_button;    
            }
            AttackButtonBackImage.sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attack_button_backimage;
                break;
            case "3":
            selected_character_id = 2;
            BackButton.gameObject.SetActive(true);
            battleDataforOnline.character_isSelected[selected_character_id] = true;
            AttackButton.gameObject.SetActive(true);
            AttackButton.anchoredPosition = characters[2].anchoredPosition + new Vector2(100, 0); 
            for (int i = 0; i <= 2; i++)
            {
            AttackButtonOne[i].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[i].attack_button;    
            }
            AttackButtonBackImage.sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attack_button_backimage;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    void Update()
    {
    if (!AnyCharacterSelected() && !is_attacking) return;

    if (is_attacking) 
    {
        ClearAttackRange(); 
        Attack(); 

        // 左クリックで攻撃を確定させる
        if (inputData.left_mouse_button_ispressed)
        {
            ConfirmAttack();
        }
    }
    else 
    {
        // 移動処理
        if (inputData.up_key_ispressed)    TryMove(0, -1, new Vector2(0, 50));
        else if (inputData.down_key_ispressed)  TryMove(0, 1, new Vector2(0, -50));
        else if (inputData.right_key_ispressed) TryMove(1, 0, new Vector2(50, 0));
        else if (inputData.left_key_ispressed)  TryMove(-1, 0, new Vector2(-50, 0));
    }

    for (int i = 0; i <= 5; i++)
    {
    battleDataforOnline.charactersBattleDatas[i].now_character_position
     = new Vector2Int(on_grid_number_x[i],on_grid_number_y[i]);
    }

    if (battleDataforLocal.is_myturn == false)
    {
        UpdateCharacterPosition();
    }
    }

    public void UpdateCharacterPosition()
    {
    for (int i = 3; i <= 5; i++)
        {
        int worldPosX = battleDataforOnline.charactersBattleDatas[i].now_character_position.x * 50 - 175;
        int worldPosY = battleDataforOnline.charactersBattleDatas[i].now_character_position.y * -50 + 30;
        characters[i].anchoredPosition = new Vector2Int(worldPosX, worldPosY);
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

    void TryMove(int moveX, int moveY, Vector2 posDelta)
    {
    int nextX = on_grid_number_x[selected_character_id] + moveX;
    int nextY = on_grid_number_y[selected_character_id] + moveY;

    if (nextX < 0 || nextX >= 8 || nextY < 0 || nextY >= 5) return;

    if(battleDataforOnline.now_my_cost - characterData.characters[battleDataforLocal.character_id[selected_character_id]].default_move_cost <0)
    return;

    // 進入可能かチェック (Online側のデータを見る)
    if (gridDataforOnline.grid_state_y[nextY].grid_state_x[nextX] >= 0)
    {
        // A. 現在の場所（移動元）を元の地形に戻す
        UpdateGridState(on_grid_number_x[selected_character_id], on_grid_number_y[selected_character_id], 0);

        // 座標更新
        on_grid_number_x[selected_character_id] = nextX;
        on_grid_number_y[selected_character_id] = nextY;
        on_grid_number[selected_character_id] = nextY * 8 + nextX;

        // B. 新しい場所（移動先）を「キャラあり」状態にする
        UpdateGridState(nextX, nextY, -1);

        characters[selected_character_id].anchoredPosition += posDelta;
        AttackButton.anchoredPosition += posDelta;

        int cost = characterData.characters[battleDataforLocal.character_id[selected_character_id]].default_move_cost;
        battleDataforOnline.now_my_cost -= cost;
    }
    }

    // グリッド情報の更新を一括で行う
    void UpdateGridState(int x, int y, int state)
    {
    // 2. Online側の更新 (-1:キャラあり, それ以外:元の地形)
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

    bool AnyCharacterSelected()
    {
        foreach (bool selected in battleDataforOnline.character_isSelected) if (selected) return true;
        return false;
    }

    public void Attack()
    {
    int currentX = on_grid_number_x[selected_character_id];
    int currentY = on_grid_number_y[selected_character_id];
    var ranges = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_range;

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

    // 2. マウスがキャラから見てどの方向にいるか判定する関数
    private Vector2Int GetMouseDirection()
    {
    // キャラクターのUI座標とマウスの座標（中心原点）の差分を取る
    Vector2 charPos = characters[selected_character_id].anchoredPosition;
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

    public void ClearAttackRange()
    {
    for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++)
            {
            gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] = 0;
            }
        }
    }

    public void ConfirmAttack()
    {
    int cost = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_cost;
    battleDataforOnline.now_my_cost -= cost;
    // 1. 現在の攻撃の威力を取得
    int power = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_power;

    // 2. 敵キャラクター（ID: 3, 4, 5）が攻撃範囲内にいるかチェック
    // ※ プレイヤーが 0,1,2 / 敵が 3,4,5 という構成を想定
    for (int i = 3; i <= 5; i++)
    {
        // 敵がいるマスの攻撃フラグが 1 ならヒット！
        if (gridDataforOnline.grid_attack_position_y[on_grid_number_y[i]].grid_attack_position_x[on_grid_number_x[i]] == 1)
        {
            ApplyDamage(i, power);
        }
    }

    // 3. 攻撃状態の解除
    is_attacking = false;
    ClearAttackRange();
    
    // UIの非表示設定など（必要に応じて）
    BackButton.gameObject.SetActive(false);
    for (int i = 0; i <= 5; i++) battleDataforOnline.character_isSelected[i] = false;

    Debug.Log("攻撃完了");
    }

    private void ApplyDamage(int targetId, int damage)
    {
    // HPを減らす
    battleDataforOnline.charactersBattleDatas[targetId].now_character_hp -= damage;

    Debug.Log($"キャラ {targetId} に {damage} ダメージ！ 残りHP: {battleDataforOnline.charactersBattleDatas[targetId].now_character_hp}");

    // 死亡判定
    if (battleDataforOnline.charactersBattleDatas[targetId].now_character_hp <= 0)
    {
        battleDataforOnline.charactersBattleDatas[targetId].now_character_hp = 0;
        ProcessDeath(targetId);
    }
    }

    private void ProcessDeath(int targetId)
    {
    Debug.Log($"キャラ {targetId} は倒れた！");
    // オブジェクトを非表示にする、または墓標にするなどの演出
    characters[targetId].gameObject.SetActive(false);
    // グリッド上の存在情報を消す
    UpdateGridState(on_grid_number_x[targetId], on_grid_number_y[targetId], 0);
    }

    private void DeselectAll()
    {
    for (int i = 0; i <= 5; i++)
    {
        // 2. 選択フラグをすべて false にする
        battleDataforOnline.character_isSelected[i] = false;
    }
    is_attacking = false;
    ClearAttackRange();
    AttackButton.gameObject.SetActive(false);
    }
}
