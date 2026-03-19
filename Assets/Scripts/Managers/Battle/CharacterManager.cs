using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Game.Network;

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
    public int now_damage;
    public GameConnector gameConnector;
    public RoomData roomData;
    public PlayerData playerData;

    // 送信済みデータのキャッシュ（毎フレーム送るのを防ぐ）
    private int[] _lastSentX = new int[6];
    private int[] _lastSentY = new int[6];


    void Awake()
    {
        gameConnector = FindFirstObjectByType<GameConnector>().GetComponent<GameConnector>();
        roomData = FindFirstObjectByType<RoomData>();
        playerData = FindFirstObjectByType<PlayerData>();
        gameConnector.characterManager = this.GetComponent<CharacterManager>();
        gameConnector.StartStream((uint)roomData.room_id, playerData.user_id); // ルームIDとユーザーIDを渡して開始
    }
    void OnDestroy()
    {
        _ = gameConnector.StopStream();// 自動更新を終了する。場所は必要に応じて変えてください
    }

    // キャラ選択・攻撃選択・移動後に䘯び出せるよう async void ヘルパー
    private async void RefreshGameData()
    {
        if (gameConnector == null || roomData == null) return;
        var data = await gameConnector.GetGameData(roomData.room_id);
        if (this == null) return; // シーン移動中に破棄された場合
        GetBattleData(data);
    }
    public void InitCharacterUI()
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
        battleDataforOnline.charactersBattleDatas[i].now_character_hp = characterData.characters[battleDataforLocal.character_id[i]].default_hp;
        battleDataforOnline.charactersBattleDatas[i].now_character_maxhp = characterData.characters[battleDataforLocal.character_id[i]].default_hp;
        battleDataforOnline.charactersBattleDatas[i].now_character_move_cost = characterData.characters[battleDataforLocal.character_id[i]].default_move_cost;

        for(int j = 0; j <= 7; j++)
        {
            battleDataforOnline.charactersBattleDatas[i].debuffs[j] = false;
        }
        }

        AttackButton.gameObject.SetActive(false);

        // anchoredPositionを使用して、Canvas内の相対座標で配置する
        SetupCharacter(0, new Vector2(-175, 30), 0, 0); // 1人目: (0,0)
        SetupCharacter(1, new Vector2(-125, -70), 1, 2); // 2人目: (1,2) ※17付近
        SetupCharacter(2, new Vector2(-175, -170), 0, 4); // 3人目: (0,4) ※32付近
        SetupCharacter(3, new Vector2(175, 30), 7, 0); // 4人目: (7,0) ※7付近
        SetupCharacter(4, new Vector2(125, -70), 6, 2); // 5人目: (6,2) ※22付近
        SetupCharacter(5, new Vector2(175, -170), 7, 4); // 6人目: (7,4) ※39付近

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
            attack_number = 0;
            if (battleDataforOnline.now_my_cost - characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_cost <0)return;
            AttackButton.gameObject.SetActive(false);
            is_attacking = true;
            RefreshGameData(); // 攻撃選択時
        }
        if(buttonName == "Attack2") 
        {
            attack_number = 1;
            if (battleDataforOnline.now_my_cost - characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_cost <0)return;
            AttackButton.gameObject.SetActive(false);
            is_attacking = true;
            RefreshGameData(); // 攻撃選択時
        }
        if(buttonName == "Attack3") 
        {
            attack_number = 2;
            if (battleDataforOnline.now_my_cost - characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_cost <0)return;
            AttackButton.gameObject.SetActive(false);
            is_attacking = true;
            RefreshGameData(); // 攻撃選択時
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
            RefreshGameData(); // キャラ選択時
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
            RefreshGameData(); // キャラ選択時
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
            RefreshGameData(); // キャラ選択時
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
        if(battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[4])
        {

        }else
        {
        // 移動処理
        if (inputData.up_key_ispressed)    TryMove(0, -1, new Vector2(0, 50));
        else if (inputData.down_key_ispressed)  TryMove(0, 1, new Vector2(0, -50));
        else if (inputData.right_key_ispressed) TryMove(1, 0, new Vector2(50, 0));
        else if (inputData.left_key_ispressed)  TryMove(-1, 0, new Vector2(-50, 0));
        }
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

    // 死亡判定
    for(int i = 0; i <= 5; i++)
    {
    if (battleDataforOnline.charactersBattleDatas[i].now_character_hp <= 0)
    {
        battleDataforOnline.charactersBattleDatas[i].now_character_hp = 0;
        ProcessDeath(i);
    }
    }

    if(battleDataforLocal.is_myturn)
    {
        SendBattleData();
    }else
    {
        //GetBattleData();
        // データは自動で送られてくるらしいです
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

        characters[selected_character_id].anchoredPosition += posDelta;
        AttackButton.anchoredPosition += posDelta;

        //デバフマスの処理をする
        if(gridDataforOnline.grid_state_y[nextY].grid_state_x[nextX] == 3)
        {
            battleDataforOnline.charactersBattleDatas[selected_character_id].now_character_hp -= 10;
            battleDataforOnline.charactersBattleDatas[selected_character_id].now_character_move_cost += 2;
        }
        if(gridDataforOnline.grid_state_y[nextY].grid_state_x[nextX] == 4)
        {
            battleDataforOnline.charactersBattleDatas[selected_character_id].now_character_hp = 50;
            gridDataforOnline.sub_grid_state_y[nextY].sub_grid_state_x[nextX] = 0;
        }

        // B. 新しい場所（移動先）を「キャラあり」状態にする
        UpdateGridState(nextX, nextY, -1);

        int cost = characterData.characters[battleDataforLocal.character_id[selected_character_id]].default_move_cost;
        if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[1])
    {
        battleDataforOnline.now_my_cost -= cost -2;
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[5])
    {
        battleDataforOnline.now_my_cost -= cost +2;
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[1]&&battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[5])
    {
        battleDataforOnline.now_my_cost -= cost;
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[1]&&battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[5])
    {
        battleDataforOnline.now_my_cost -= cost;
    }

        RefreshGameData(); // 移動後
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
    // 1. 現在の攻撃の威力を取得
    int power = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_power;

    int target = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_target;

    // 2. キャラクターが攻撃範囲内にいるかチェック
    // ※ プレイヤーが 0,1,2 / 敵が 3,4,5 という構成を想定
    int hit_character = 0;
    if (target == 1 || target == 2)
    {
    for (int i = 0; i <= 3; i++)
    {
        // 味方がいるマスの攻撃フラグが 1 ならヒット！
        if (gridDataforOnline.grid_attack_position_y[on_grid_number_y[i]].grid_attack_position_x[on_grid_number_x[i]] == 1)
        {
            ApplyDamage(i, power);
            hit_character++;

            BuffDebuff(i);
        }
    }
    }

    if (target == 0 || target == 2)
    {
    for (int i = 3; i <= 5; i++)
    {
        // 敵がいるマスの攻撃フラグが 1 ならヒット！
        if (gridDataforOnline.grid_attack_position_y[on_grid_number_y[i]].grid_attack_position_x[on_grid_number_x[i]] == 1)
        {
            ApplyDamage(i, power);
            hit_character++;

            BuffDebuff(i);
        }
    }
    if (gridDataforOnline.grid_attack_position_y[battleDataforOnline.opponent_base_position.y].grid_attack_position_x[battleDataforOnline.opponent_base_position.x] == 1)
    {
        hit_character++;
        battleDataforOnline.opponent_base_hp -= power;
        if (battleDataforOnline.opponent_base_hp <= 0)
    {
        battleDataforOnline.win_player_id = battleDataforOnline.my_player_id;
        battleDataforOnline.game_end = true;
    }
    }
    }

    if (hit_character == 0)
    {

        if (battleDataforOnline.selected_character[selected_character_id] == 3 && attack_number == 2)
        {
            for (int y = 0; y <= 4; y++)
            {
                for (int x = 0; x <= 7; x++)
                {
                    if (gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] != 1)
                    {
                    if (gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] == 1)
                    {
                        gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] = 3;
                        gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] = 3;
                    }
                    }else
                    {
                        //当たったキャラクターがいないときの処理を書く
                    }
                }
            }
        }else if (battleDataforOnline.selected_character[selected_character_id] == 6 && attack_number == 0)
        {
            for (int y = 0; y <= 4; y++)
            {
                for (int x = 0; x <= 7; x++)
                {
                    if (gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] != 1)
                    {
                    if (gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] == 1)
                    {
                        gridDataforOnline.grid_state_y[y].grid_state_x[x] = 4;
                        gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] = 4;
                    }
                    }else
                    {
                        //当たったキャラクターがいないときの処理を書く
                    }
                }
            }
        }else
        {
            //当たったキャラクターがいないときの処理を書く
        }

        // 3. 攻撃状態の解除
    is_attacking = false;
    ClearAttackRange();
    
    // UIの非表示設定など（必要に応じて）
    BackButton.gameObject.SetActive(false);
    for (int i = 0; i <= 5; i++) battleDataforOnline.character_isSelected[i] = false;
    }else
    {
    if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[2])
    {
        battleDataforOnline.now_my_cost -= cost -5;
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[6])
    {
        battleDataforOnline.now_my_cost -= cost +5;
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[2]&&battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[6])
    {
        battleDataforOnline.now_my_cost -= cost;
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[2]&&battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[6])
    {
        battleDataforOnline.now_my_cost -= cost;
    }

    // 3. 攻撃状態の解除
    is_attacking = false;
    ClearAttackRange();
    
    // UIの非表示設定など（必要に応じて）
    BackButton.gameObject.SetActive(false);
    for (int i = 0; i <= 5; i++) battleDataforOnline.character_isSelected[i] = false;

    // 【デバッグ】攻撃データをコンソールに出力（UIには反映しない）
    int attackCost = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_cost;
    int attackPow  = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].default_attack_power;
    Debug.Log($"<color=orange>[SendAttack] 攻撃送信: roomId={roomData?.room_id}, playerId={playerData?.user_id}, attackerCharaId={selected_character_id}, attackType={attack_number}, power={attackPow}, cost={attackCost}, baseHP1={battleDataforOnline.base_hp}, baseHP2={battleDataforOnline.opponent_base_hp}</color>");

    Debug.Log("攻撃完了");
    }
    }

    private void ApplyDamage(int targetId, int damage)
    {
    // HPを減らす
    if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[7])
    {
        now_damage = (int)(damage * 0.75);
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[0])
    {
        now_damage = (int)(damage * 1.25);
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[0]&&battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[7])
    {
        now_damage = damage;
    }else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[0]&&battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[7])
    {
        now_damage = damage;
    }
    battleDataforOnline.charactersBattleDatas[targetId].now_character_hp -= now_damage;

    Debug.Log($"キャラ {targetId} に {damage} ダメージ！ 残りHP: {battleDataforOnline.charactersBattleDatas[targetId].now_character_hp}");
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

    public void BuffDebuff(int index)
    {
        for(int i = 0; i <= 7; i++)
        {
            int dice = Random.Range(1, 101);
        if (characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].debuffs[i] > 0)
        {
            if (battleDataforOnline.charactersBattleDatas[index].debuffs[i] == false)
            {
            if (characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].debuffs[i] <= dice)
            {
                battleDataforOnline.charactersBattleDatas[index].debuffs[i] = true;
                switch(i)
                {
                    case 1:
                    battleDataforOnline.charactersBattleDatas[index].now_character_move_cost -= 2;
                    break;
                    case 5:
                    battleDataforOnline.charactersBattleDatas[index].now_character_move_cost += 2;
                    break;
                    default:
                    break;
                }
            }
            }
        }else
        {
            if (battleDataforOnline.charactersBattleDatas[index].debuffs[i] == true)
            {
            if (characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[attack_number].debuffs[i] * -1 <= dice)
            {
                battleDataforOnline.charactersBattleDatas[index].debuffs[i] = false;
            }
            }
        }
        }
    }

    void SendBattleData()
    {
        if (playerData == null || roomData == null) return;
        string pid = playerData.user_id;
        int rid = roomData.room_id;

        // 移動したキャラだけ位置を送信（毎フレーム送らないよう差分チェック）
        for (int i = 0; i <= 2; i++)
        {
            if (on_grid_number_x[i] != _lastSentX[i] || on_grid_number_y[i] != _lastSentY[i])
            {
                _ = gameConnector.SendMove(rid, pid, i, on_grid_number_x[i], on_grid_number_y[i]);
                _lastSentX[i] = on_grid_number_x[i];
                _lastSentY[i] = on_grid_number_y[i];
            }
        }
    }

    public void GetBattleData(GameDataResponse data)
    {
        if (data == null) return;

        bool is1p = (playerData.user_id == data.Player1Id);

        // 自分原方・HP
        battleDataforOnline.base_hp = is1p ? (int)data.BaseHp1 : (int)data.BaseHp2;
        battleDataforOnline.opponent_base_hp = is1p ? (int)data.BaseHp2 : (int)data.BaseHp1;

        // ターン情報
        bool isMyTurn = is1p ? data.Is1PTurn : !data.Is1PTurn;
        battleDataforOnline.now_moving_player = isMyTurn ? battleDataforOnline.my_player_id : (battleDataforOnline.my_player_id == 0 ? 1 : 0);

        // コストをサーバーから反映
        battleDataforOnline.now_my_cost    = is1p ? (int)data.Cost1P : (int)data.Cost2P;
        battleDataforOnline.now_enemy_cost = is1p ? (int)data.Cost2P : (int)data.Cost1P;

        // 相手キャラクター位置を反映（インデックス3〜5が相手側）
        int opponentOffset = 0;
        foreach (var c in data.Characters)
        {
            bool charIs1p = c.Is1P;
            bool charIsMine = (is1p == charIs1p);
            if (!charIsMine && opponentOffset < 3)
            {
                int idx = 3 + opponentOffset;
                // 相手キャラのHPを更新
                battleDataforOnline.charactersBattleDatas[idx].now_character_hp = (int)c.Hp;
                opponentOffset++;
            }
        }

        // ゲーム終了判定
        if (data.IsFinished)
        {
            battleDataforOnline.game_end = true;
            battleDataforOnline.win_player_id = (data.WinnerPlayerId == playerData.user_id) ? battleDataforOnline.my_player_id : (battleDataforOnline.my_player_id == 0 ? 1 : 0);
        }

        // 【デバッグ】受信データをコンソールに出力（UIには反映しない）
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<color=cyan>[GetBattleData] サーバーからゲームデータ受信</color>");
        sb.AppendLine($"  1PTurn={data.Is1PTurn}  BaseHp1={data.BaseHp1}  BaseHp2={data.BaseHp2}  IsFinished={data.IsFinished}  Winner={data.WinnerPlayerId}");
        foreach (var c in data.Characters)
        {
            sb.AppendLine($"  Chara: Is1P={c.Is1P}  CharaId={c.CharacterId}  HP={c.Hp}  PosX={c.PositionX}  PosY={c.PositionY}");
        }
        Debug.Log(sb.ToString());
    }

    // BattleOnlineManagerのEndMyTurnから呼び出す用の機能
    public void NotifyTurnEnd()
    {
        if (playerData == null || roomData == null) return;
        _ = gameConnector.SendTurnEnd(roomData.room_id, playerData.user_id);
    }
}
