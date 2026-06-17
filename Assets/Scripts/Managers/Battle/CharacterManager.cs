using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Game.Network;

public class CharacterManager : MonoBehaviour
{
    public bool is_1p;// is_1pは自キャラ相手キャラ、自ターン相手ターンの判別に用いる
    public PlayerState self;
    public PlayerState opponent;
    public InputData inputData;
    public GridDataforOnline gridDataforOnline;
    public CharacterData characterData;
    public BattleDataForOnline battleDataforOnline;
    public int selected_character_index = -1;// selected_character_id -> selected_character_index  -1は未選択を表したいがエラー吐かないか心配
    public RectTransform BackButton;
    public BattleViewManager battleViewManager;
    public int attack_number;
    public bool is_attacking = false;
    public GameConnector gameConnector {
        get {
            // 他の GameConnector が Awake で自分自身を Destroy していても、
            // 正しいシングルトンインスタンス(最初に Awake が完了したもの)を確実に取得するようにする
            if (GameConnector.instance != null) return GameConnector.instance;
            if (_gameConnector == null) _gameConnector = GameConnector.instance;
            return _gameConnector;
        }
        set { _gameConnector = value; }
    }
    private GameConnector _gameConnector;
    // おそらくシングルトンになれば上の方が良い。暫定的な処理を書いておきます。
    public BattleConnector battleConnector;
    public RoomData roomData;
    public UserData userData;

    public event System.Action<AttackEventData> OnAttackExecuted;
    
    // 攻撃情報を通知するための構造体
    public struct AttackEventData
    {
        public uint attackerUniqueId;
        public uint targetUniqueId;   // 0の場合は拠点
        public int finalDamage;
        public int attackType;        // 0-2 (自分) または サーバーからの種類
        public bool isPlayerAttack;   // 自分が実行したか
    }

    // 送信済みデータのキャッシュ（毎フレーム送るのを防ぐ）
    private int[] _lastSentX = new int[6];
    private int[] _lastSentY = new int[6];
    private Vector2Int _lastAttackDirection;
    private bool _isFirstAttackFrame; // 連続クリック防止用


    private T GetSo<T>(T existing) where T : ScriptableObject
    {
        if (existing != null) return existing;
        var targets = Resources.FindObjectsOfTypeAll<T>();
        if (targets.Length > 0) return targets[0];
        return null;
    }

    void Awake()
    {
        Debug.Log("[CharacterManager] Awake Started");
        // 暫定的な処理
        battleConnector = FindFirstObjectByType<BattleConnector>();
        roomData = GetSo(roomData);
        userData = GetSo(userData);
    }
        
    void Start()
    {
        is_1p = (userData.user_id == battleDataforOnline.player1.player_id);// battleOnlineManagerのAwake以降で呼び出すようにする
        self     = (is_1p) ? battleDataforOnline.player1 : battleDataforOnline.player2;
        opponent = (is_1p) ? battleDataforOnline.player2 : battleDataforOnline.player1;
    }
    void OnDestroy()
    {
        _ = battleConnector.StopStream();// 自動更新を終了する。場所は必要に応じて変えてください
    }


    public void OnButtonClick(string buttonName)
    {
        if (battleDataforOnline.is_1p_turn != is_1p)return;// 相手のターンなら何もしない

        if (buttonName == "1" || buttonName == "2" || buttonName == "3" || buttonName == "BackButton")// 自キャラかバックボタン
        {
            DeselectAll();// 動かすキャラを選択する最初の状態に復帰する処理。現在動かしているキャラから新たなキャラの処理ができるようにする
        }

        switch (buttonName)// 自キャラをクリックした時の処理
        {
            case "1":
                SEManager.instance?.PlaySelectSE();
                selected_character_index = 0;
                BackButton.gameObject.SetActive(true);
                // キャラクターの選択をサーバーに通知する処理は現状不要。battleDataForOnlineを直接書き換える。
                // 次フレームでフロントのis_selectedがtrueになる
                self.characters[selected_character_index].character_isSelected = true;
                battleViewManager.ShowAttackWindow();
                _ = SendGridData();     int これは何の処理 = 0;
                break;
            case "2":
                SEManager.instance?.PlaySelectSE();
                selected_character_index = 1;
                BackButton.gameObject.SetActive(true);
                self.characters[selected_character_index].character_isSelected = true;
                battleViewManager.ShowAttackWindow();
                _ = SendGridData();
                break;
            case "3":
                SEManager.instance?.PlaySelectSE();
                selected_character_index = 2;
                BackButton.gameObject.SetActive(true);
                self.characters[selected_character_index].character_isSelected = true;
                battleViewManager.ShowAttackWindow();
                _ = SendGridData();
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }

        if(buttonName == "BackButton")
        {
            // DeselectAllで実行されていない処理を書いてます
            SEManager.instance?.PlayBackSE();
            BackButton.gameObject.SetActive(false);
        }
        
        // キャラが選択されて攻撃ボタンが出ているとき
        if (self.characters[0].character_isSelected || self.characters[1].character_isSelected || self.characters[2].character_isSelected)
        {
            if(buttonName == "BackButton")// おそらくここにはいらないはず
            {
                /*  DeselectAllで実行済みの処理
                for (int i = 0; i <= 2; i++)
                {
                self.characters[i].character_isSelected = false;
                }
                attackButton.gameObject.SetActive(false);
                is_attacking = false;
                */


                /* 通常のBackButtonで実行される処理
                SEManager.instance?.PlayBackSE();
                BackButton.gameObject.SetActive(false);
                */

                int これは何の処理 = 0;
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
                SEManager.instance?.PlaySelectSE();
                attack_number = 0;
                // コスト不足ならリターン
                if (self.current_cost_remaining - characterData.characters[self.characters[selected_character_index].unique_id].attacks[attack_number].default_attack_cost < 0) return;
                // 攻撃範囲選択処理に移行
                battleViewManager.HideAttackWindow();
                is_attacking = true;
                _isFirstAttackFrame = true; // ガード開始
                // グリッドは後で考える
                _ = SendGridData();
            }
            if(buttonName == "Attack2") 
            {
                SEManager.instance?.PlaySelectSE();
                attack_number = 1;
                // コスト不足ならリターン
                if (self.current_cost_remaining - characterData.characters[self.characters[selected_character_index].unique_id].attacks[attack_number].default_attack_cost < 0) return;
                // 攻撃範囲選択処理に移行
                battleViewManager.HideAttackWindow();
                is_attacking = true;
                _isFirstAttackFrame = true; // ガード開始
                _ = SendGridData();
            }
            if(buttonName == "Attack3") 
            {
                SEManager.instance?.PlaySelectSE();
                attack_number = 2;
                // コスト不足ならリターン
                if (self.current_cost_remaining - characterData.characters[self.characters[selected_character_index].unique_id].attacks[attack_number].default_attack_cost < 0) return;
                // 攻撃範囲選択処理に移行
                battleViewManager.HideAttackWindow();
                is_attacking = true;
                _isFirstAttackFrame = true; // ガード開始
                _ = SendGridData();
            }
        }
    }

    private async Task SendGridData()// 現状だとGridDataForOnlineをフロントで書き換えて、そのデータをサーバーに同期させる構造になってるように見える。
    {
        if (battleConnector != null && roomData != null && userData != null && battleDataforOnline != null)
        {
            Debug.Log($"<color=yellow><b>[SendGridData] 送信開始</b>: Room={roomData.room_id}, User={userData.user_id}</color>");
            Debug.Log($"<color=cyan>[SendGridData] Sending Cost: {self.current_cost_remaining}</color>");
            // 一旦コメントアウト
            // await gameConnector.SendGridUpdate(roomData.room_id, userData.user_id, gridDataforOnline, battleDataforOnline, is_1p, self.current_cost_remaining);
        }
    }

    void Update()
    {
        if (!AnyCharacterSelected() && !is_attacking) return;

        if (is_attacking) 
        {
            ClearAttackRange(); 
            Attack(); 

            // 攻撃方向が変わった場合、または初めて攻撃モードに入った場合にグリッド同期
            Vector2Int currentDir = GetMouseDirection();
            if (currentDir != _lastAttackDirection)
            {
                SEManager.instance?.PlayClickSE();
                _lastAttackDirection = currentDir;
                // ここで攻撃キャラId、1Pか、攻撃番号、攻撃予告方向をバックに送ればGridに組み込む必要はなくなる。
                // Gridにするならここにサーバーへのグリッド変更通知（どのグリッドがどう変わるのか）を送信
                _ = SendGridData();
            }

            // 左クリックで攻撃を確定させる
            if (inputData.left_mouse_button_ispressed)
            {
                if (_isFirstAttackFrame)
                {
                    // ボタンが押されたまま（UIクリックの継続）なら確定させない
                    return;
                }
                Debug.Log("<color=white><b>[Update] 攻撃確定入力を検知</b></color>");
                ConfirmAttack();
            }
            else
            {
                // ボタンが離されたらガード解除
                _isFirstAttackFrame = false;
            }
        }
        else 
        {
            if(selected_character_index > 0 && self.characters[selected_character_index].debuffs[4])// 麻痺
            {

            }else
            {
                // 移動処理
                if (inputData.up_key_ispressed)    TryMove(0, -1);
                else if (inputData.down_key_ispressed)  TryMove(0, 1);
                else if (inputData.right_key_ispressed) TryMove(1, 0);
                else if (inputData.left_key_ispressed)  TryMove(-1, 0);
            }
        }

        // 死亡判定はサーバーで通知して欲しい。フロントでは判定処理を持たないで攻撃や移動と同じようにイベントを貰ってProsessDeathを叩く
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

    async void TryMove(int moveX, int moveY)
    {
        SEManager.instance?.PlayClickSE();// 移動の可否問わず音が流れます。うるさかったら移動してね
        int currentX = self.characters[selected_character_index].now_character_position.x;
        int currentY = self.characters[selected_character_index].now_character_position.y;
        int nextX = currentX + moveX;
        int nextY = currentY + moveY;

        if (nextX < 0 || nextX >= 8 || nextY < 0 || nextY >= 5) return;// 長方形グリッドの外にはいかない

        if(self.current_cost_remaining - characterData.characters[self.characters[selected_character_index].unique_id].default_move_cost < 0) return;

    // 進入可能かチェック (Online側のデータを見る)
    if (gridDataforOnline.grid_state_y[nextY].grid_state_x[nextX] >= 0)
    {
        // A. 現在の場所（移動元）を元の地形に戻す
        int 直接Gridを変更しない = 0;
        UpdateGridState(currentX, currentY, 0);

        // 座標更新
        await battleConnector.SendMove(roomData.room_id, self.player_id, self.characters[selected_character_index].unique_id, 
        nextX, nextY);

        //デバフマスの処理をする
        if(gridDataforOnline.grid_state_y[nextY].grid_state_x[nextX] == 3)
        {
            // ここではサーバーにデバフマス3を踏んだことを通知する
            // battleDataforOnline.charactersBattleDatas[selected_character_id].now_character_hp -= 10;
            // battleDataforOnline.charactersBattleDatas[selected_character_id].now_character_move_cost += 2;
        }
        if(gridDataforOnline.grid_state_y[nextY].grid_state_x[nextX] == 4)
        {
            // 上と同様
            // battleDataforOnline.charactersBattleDatas[selected_character_id].now_character_hp = 50;
            // gridDataforOnline.sub_grid_state_y[nextY].sub_grid_state_x[nextX] = 0;
        }

        // B. 新しい場所（移動先）を「キャラあり」状態にする
        UpdateGridState(nextX, nextY, -1);

        // コストは少なくともBattleDataForOnlineの移動コストを利用するはず。新たに計算する必要はない
        /*
        int cost = characterData.characters[self.characters[selected_character_index].unique_id].default_move_cost;
        if (self.characters[selected_character_index].debuffs[1])
        {
            // バフデバフがかかっていたら移動コストを変化させる。SendMoveに代入するコストを変えれば良さそう
            cost -= 2;
        }
        else if (self.characters[selected_character_index].debuffs[5])
        {
            cost += 2;
        }
        */
        // ここでSendMoveを呼べば良い？
    
    // 移動した際にグリッドデータを送信
    _ = SendGridData();

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
        foreach (CharactersBattleData my_chara in self.characters) if (my_chara.character_isSelected) return true;
        return false;
    }

    public void Attack()
    {
    int currentX = self.characters[selected_character_index].now_character_position.x;
    int currentY = self.characters[selected_character_index].now_character_position.y;
    var ranges = characterData.characters[self.characters[selected_character_index].unique_id].attacks[attack_number].default_attack_range;

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
        // キャラクターのキャンバスに対する相対座標とマウスのキャンバス座標の差分を取る
        Vector2 charPos = battleViewManager.characters[selected_character_index].transform.localPosition;
        Vector2 mousePos = inputData.mouse_position;
        
        Vector2 diff = mousePos - charPos;

        Vector2Int result;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            result = diff.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            result = diff.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        // 変化があった時だけログを出す等しても良いが、デバッグのため一旦そのまま出す
        // Debug.Log($"[GetMouseDirection] charPos={charPos}, mousePos={mousePos}, diff={diff}, result={result}");

        return result;
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

    public async void ConfirmAttack()
    {
    Debug.Log("<color=white><b>[ConfirmAttack] 処理開始</b></color>");
    Debug.Log("攻撃時の音が欲しかったらこの辺につければ良さそう");
    int cost = characterData.characters[self.characters[selected_character_index].unique_id].attacks[attack_number].default_attack_cost;
    // 1. 現在の攻撃の威力を取得
    int power = characterData.characters[self.characters[selected_character_index].unique_id].attacks[attack_number].default_attack_power;

    int target = characterData.characters[self.characters[selected_character_index].unique_id].attacks[attack_number].default_attack_target;

    Debug.Log($"[ConfirmAttack] selected_char={selected_character_index}, attack_num={attack_number}, cost={cost}, power={power}, targetType={target}");

    // 2. キャラクターが攻撃範囲内にいるかチェック
    int hit_character = 0;
    if (target == 1 || target == 2)
    {
        for (int i = 0; i <= 2; i++)
        {
            // 味方がいるマスの攻撃フラグが 1 ならヒット！
            int cx = self.characters[i].now_character_position.x;
            int cy = self.characters[i].now_character_position.y;
            if (gridDataforOnline.grid_attack_position_y[cy].grid_attack_position_x[cx] == 1)
            {
                int finalDamage = ApplyDamage(i, power);
                hit_character++;

                BuffDebuff(i);
                await SendAttackInfo(i);

                // イベントの発火
                OnAttackExecuted?.Invoke(new AttackEventData {
                attackerUniqueId = (uint)self.characters[selected_character_index].unique_id,
                targetUniqueId = (uint)self.characters[i].unique_id,
                finalDamage = finalDamage,
                attackType = attack_number,
                isPlayerAttack = true
                });
            }
        }
    }

    if (target == 0 || target == 2)
    {
    for (int i = 0; i <= 2; i++)
    {
        // 敵がいるマスの攻撃フラグが 1 ならヒット！
        int cx = opponent.characters[i].now_character_position.x;
        int cy = opponent.characters[i].now_character_position.y;
        if (gridDataforOnline.grid_attack_position_y[cy].grid_attack_position_x[cx] == 1)
        {
            int finalDamage = ApplyDamage(i, power);
            hit_character++;

            BuffDebuff(i);
            await SendAttackInfo(i);

            // イベントの発火
            OnAttackExecuted?.Invoke(new AttackEventData {
                attackerUniqueId = (uint)self.characters[selected_character_index].unique_id,
                targetUniqueId = (uint)opponent.characters[i].unique_id,
                finalDamage = finalDamage,
                attackType = attack_number,
                isPlayerAttack = true
            });
        }
    }
    int 拠点位置をどう検知するか要検討 = 0;
    if (gridDataforOnline.grid_attack_position_y[opponent.base_position.y].grid_attack_position_x[opponent.base_position.x] == 1)
    {
        hit_character++;
        
        // 拠点へのダメージにも倍率を適用
        bool hasDown = self.characters[selected_character_index].debuffs[7];
        bool hasUp = self.characters[selected_character_index].debuffs[0];
        float multiplier = 1.0f;
        if (hasDown && hasUp) multiplier = 1.0f;
        else if (hasDown)     multiplier = 0.75f;
        else if (hasUp)       multiplier = 1.25f;

        int finalBaseDamage = Mathf.RoundToInt(power * multiplier);
        // SendAttackで送れば良さそう
        // battleDataforOnline.opponent_base_hp -= finalBaseDamage;
        
        await SendAttackInfo(-1);

        // イベントの発火 (拠点への攻撃)
        Debug.Log($"<color=white>[CharacterManager] OnAttackExecuted(Base) 発火: attacker={self.characters[selected_character_index].unique_id}</color>");
        OnAttackExecuted?.Invoke(new AttackEventData {
            attackerUniqueId = (uint)self.characters[selected_character_index].unique_id,
            targetUniqueId = 0, // 拠点を0とする
            finalDamage = finalBaseDamage,
            attackType = attack_number,
            isPlayerAttack = true
        });

        // ここで書く処理ではない気がする
        /*
        if (battleDataforOnline.opponent_base_hp <= 0)
        {
            battleDataforOnline.win_player_id = userData.user_id;
            battleDataforOnline.game_end = true;
        }
        */
    }
    }

    if (hit_character > 0)
    {
        Debug.Log("キャラクターまたは拠点に攻撃がヒットしました");
    }
    else
    {
        // ヒットしなかった場合、特定のスキルによる設置物（トラップ等）の処理
        int そもそもここに書くべきか = 0;

        if (self.characters[selected_character_index].unique_id == 3 && attack_number == 2)
        {
            for (int y = 0; y <= 4; y++)
            {
                for (int x = 0; x <= 7; x++)
                {
                    if (gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] == 0)
                    {
                        if (gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] == 1)
                        {
                            int サーバーに通知するようにする = 0;
                            // 撒菱
                            gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] = 3;
                        }
                    }
                }
            }
        }
        else if (self.characters[selected_character_index].unique_id == 6 && attack_number == 0)
        {
            for (int y = 0; y <= 4; y++)
            {
                for (int x = 0; x <= 7; x++)
                {
                    if (gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] == 0)
                    {
                        if (gridDataforOnline.grid_attack_position_y[y].grid_attack_position_x[x] == 1)
                        {
                            // 地雷
                            gridDataforOnline.grid_state_y[y].grid_state_x[x] = 4;
                            gridDataforOnline.sub_grid_state_y[y].sub_grid_state_x[x] = 4;
                        }
                    }
                }
            }
        }
    }

    // 3. 攻撃状態の解除とクリーンアップ（ヒットの有無に関わらず実行）
    is_attacking = false;
    ClearAttackRange();
    
    if (BackButton != null) BackButton.gameObject.SetActive(false);
    int サーバー側にキャラ選択解除を通知 = 0;
    int DeselectAllではだめなのか = 0;
    // for (int i = 0; i <= 5; i++) battleDataforOnline.character_isSelected[i] = false;

    // コスト消費
    int コスト消費はサーバーに通知するように書き換える = 0;
    /*
    if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[2])
    {
        battleDataforOnline.now_my_cost -= (cost - 5);
    }
    else if (battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[6])
    {
        battleDataforOnline.now_my_cost -= (cost + 5);
    }
    else
    {
        battleDataforOnline.now_my_cost -= cost;
    }

    Debug.Log($"<color=orange>[ConfirmAttack] 処理完了: 残りコスト={battleDataforOnline.now_my_cost}, 拠点HP={battleDataforOnline.opponent_base_hp}</color>");
    */

    // すべての攻撃送信（awaitしたもの）が終わってからグリッド同期
    int 必要かどうか要検討 = 0;
    await SendGridData();
    }

    private async Task SendAttackInfo(int targetIdx)
    {
        // 作成方法は後で考える
        /*
        uint attackerUid = (uint)self.characters[selected_character_index].unique_id;
        uint targetUid = (targetIdx != -1) ? battleDataforOnline.charactersBattleDatas[targetIdx].unique_id : 0;
        uint targetNewHp = (targetIdx != -1) ? (uint)battleDataforOnline.charactersBattleDatas[targetIdx].now_character_hp : 0;

        // サーバー側の BaseHp1, BaseHp2 に正しくマッピングする
        bool is1p = (battleDataforOnline.my_player_id == 0);
        int sendBaseHp1 = is1p ? battleDataforOnline.base_hp : battleDataforOnline.opponent_base_hp;
        int sendBaseHp2 = is1p ? battleDataforOnline.opponent_base_hp : battleDataforOnline.base_hp;

        Debug.Log($"<color=orange><b>[SendAttackInfo] 攻撃送信</b>: Side={(is1p ? "1P" : "2P")} Attacker={attackerUid}, Target={targetUid}, NewHP={targetNewHp}, Cost={battleDataforOnline.now_my_cost}</color>");

        await gameConnector.SendAttack(
            roomData.room_id, 
            userData.user_id, 
            (int)attackerUid, 
            attack_number, 
            true, 
            sendBaseHp1, 
            sendBaseHp2, 
            (int)targetUid, 
            (int)targetNewHp,
            battleDataforOnline.now_my_cost
        );
        */
    }

    private int ApplyDamage(int targetId, int damage)
    {
        // 与えるダメージを計算する関数にしてSendAttackInfoで呼び出す？
        /*
        float multiplier = 1.0f;
        if(damage >= 0)// 攻撃or回復を分岐
        {
        SEManager.instance?.PlayDamageSE();
        // 攻撃側のデバフ/バフ状態を確認
        bool hasDown = battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[7];
        bool hasUp = battleDataforOnline.charactersBattleDatas[selected_character_id].debuffs[0];
        if (hasDown && hasUp) multiplier = 1.0f;
        else if (hasDown)     multiplier = 0.75f;
        else if (hasUp)       multiplier = 1.25f;
        }

        else// 回復
        {
            SEManager.instance?.PlayHealSE();
        }

        // 計算後のダメージをローカル変数に格納
        int finalDamage = Mathf.RoundToInt(damage * multiplier);
        
        // HPを減らす
        battleDataforOnline.charactersBattleDatas[targetId].now_character_hp -= finalDamage;


        Debug.Log($"<color=orange><b>[ApplyDamage]</b> Target={targetId}, Base={damage}, Mult={multiplier}, Final={finalDamage}, RemainingHP={battleDataforOnline.charactersBattleDatas[targetId].now_character_hp}</color>");
        return finalDamage;
        */
        return 0;// 仮
    }

    private void ProcessDeath(int targetId)
    {
        // サーバーから死亡通知を受け取ってUnity側での死亡処理をする場所にしたい
        /*
        Debug.Log($"キャラ {targetId} は倒れた！");
        // オブジェクトを非表示にする、または墓標にするなどの演出
        // 死亡時の音もここに流してね
        characters[targetId].gameObject.SetActive(false);
        // グリッド上の存在情報を消す
        int cx = battleDataforOnline.charactersBattleDatas[targetId].now_character_position.x;
        int cy = battleDataforOnline.charactersBattleDatas[targetId].now_character_position.y;
        UpdateGridState(cx, cy, 0);
        */
    }

    private void DeselectAll()
    {
        // キャラの選択を全てリセットして、攻撃画面を開いていたらそれも閉じる。攻撃選択中のブール、グリッド変化も消す

        for (int i = 0; i <= 2; i++)
        {
            self.characters[i].character_isSelected = false;
        }
        battleViewManager.HideAttackWindow();
        is_attacking = false;// 攻撃選択中ブールをfalseに
        int gameConnectorの攻撃範囲表示グリッドを消す処理を叩く = 0;
    }

    public void BuffDebuff(int index)
    {
        // キャラと攻撃番号からバフデバフをつける。
        /*
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
        */
    }

/* 移動時にSendMoveを送れば大丈夫だと思う
    void SendBattleData()
    {
        if (userData == null || roomData == null) return;
        string pid = userData.user_id;
        int rid = roomData.room_id;

        // 移動したキャラだけ位置を送信（毎フレーム送らないよう差分チェック）
        for (int i = 0; i <= 2; i++)
        {
            int cx = battleDataforOnline.charactersBattleDatas[i].now_character_position.x;
            int cy = battleDataforOnline.charactersBattleDatas[i].now_character_position.y;
            if (cx != _lastSentX[i] || cy != _lastSentY[i])
            {
                uint uid = battleDataforOnline.charactersBattleDatas[i].unique_id;
                bool is1p = (battleDataforOnline.my_player_id == 0);
                Vector2Int converted = ConvertCoordinateForServer(cx, cy, is1p);
                Debug.Log($"<color=orange>[SendMove] idx={i}  unique_id={uid}  x={cx}({converted.x})  y={cy}({converted.y}) Cost={battleDataforOnline.now_my_cost}</color>");
                _ = gameConnector.SendMove(rid, pid, (int)uid, converted.x, converted.y, battleDataforOnline.now_my_cost);
                _lastSentX[i] = cx;
                _lastSentY[i] = cy;
            }
        }
    }
    */
    

    // BattleOnlineManagerのEndMyTurnから呼び出す用の機能
    public void NotifyTurnEnd()
    {
        if (userData == null || roomData == null) return;
        _ = battleConnector.SendTurnEnd(roomData.room_id, userData.user_id);
    }
}
