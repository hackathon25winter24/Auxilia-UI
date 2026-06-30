using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Threading;

public class BattleOnlineManager : MonoBehaviour
{
    [Header("Data & ScriptableObjects")]
    public CharacterData characterData;
    public InputData inputData;
    public SceneData sceneData;
    public UserData userData;
    public BattleDataForOnline battleDataforOnline;
    public BattleDataforLocal battleDataforLocal;
    public GridDataforOnline gridDataforOnline;
    public RoomData roomData;

    [Header("Managers & Views")]
    public BattleViewManager battleViewManager;
    public GridManager gridManager;
    private CharacterManager characterManager;

    [Header("UI Elements")]
    public TextMeshProUGUI gametext;
    public RectTransform gameTextObject;
    public Slider timerSlider; 

    [Header("Timer Settings")]
    public float maxTime = 60f; 
    private float currentTime;
    private bool isTimerRunning = false;

    [Header("Text Animation Settings")]
    Vector2 startPosition = new Vector2(1000, 0);
    Vector2 destination = new Vector2(-1000, 0);
    public float duration = 2.0f;
    public float elapsed = 0f;
    public bool is_text_moving;
    private uint consumed_attack_id = 0;

    private NetworkManager Net => NetworkManager.Instance;
    private AuthenticationConnector authenticationConnector => Net?.Auth;
    private BattleConnector battleConnector => Net?.Battle;
    private float _turnTransitionTime = 0f;
    private CancellationTokenSource _battleCts;


    private T GetSo<T>(T existing) where T : ScriptableObject
    {
        if (existing != null) return existing;
        var targets = Resources.FindObjectsOfTypeAll<T>();
        if (targets.Length > 0) return targets[0];
        return null;
    }

    void Awake()
    {
        Debug.Log("[BattleOnlineManager] Awake Started");
        try
        {
            roomData = GetSo(roomData);
            userData = GetSo(userData);

            characterManager = FindFirstObjectByType<CharacterManager>();
            gridManager = FindFirstObjectByType<GridManager>();

            if (Net != null)
            {
                Net.battleOnlineManager = this;
            }
            else
            {
                Debug.LogError("[BattleOnlineManager] NetworkManager.Instance が存在しません。インスペクターか初期化順を確認してください。");
                return;
            }

            // --- 重要: 通信開始前にイベントだけ先に購読する ---
            var battleView = FindFirstObjectByType<BattleViewManager>();
            if (battleView != null)
            {
                battleView.characterManager = characterManager;
                battleView.SubscribeToEvents();
                Debug.Log("[BattleOnlineManager] Early event subscription successful.");
            }

            // サーバーからゲームデータを取得して初期化する
            SetFirstGameData();

            // バトル開始演出を開始
            gametext.text = "battle start!";
            StartCoroutine(MoveRoutine());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BattleOnlineManager] Awake Exception: {e.Message}\n{e.StackTrace}");
        }
    }
    void Start()
    {
        if (roomData != null && userData != null && Net != null && Net.Battle != null)
        {
            Debug.Log("[BattleOnlineManager] StartStream starting in Start()");
            Net.Battle.StartStream((uint)roomData.room_id, userData.user_id);
        }
        else
        {
            Debug.LogError("BattleOnlineManager: 必要なデータまたは NetworkManager.Battle が見つからないため StartStream をスキップしました。");
        }
    }

    
    void Update()
    {
        if (is_text_moving) return;

        if (_turnTransitionTime > 0)
        {
            _turnTransitionTime -= Time.deltaTime;
        }
        int タイマー設定もサーバー側との調整をする = 0;
        if (battleDataforOnline.is_1p_turn == (userData.user_id == battleDataforOnline.player1.player_id)) // 自分のターンを検知
        {
            // ターン終了直後（猶予時間中）であれば、サーバーからの自ターン継続情報を無視する
            if (_turnTransitionTime > 0) return;
            if (battleDataforLocal.is_myturn != true)
            {
            battleDataforLocal.is_myturn = true;
            StartMyTurn();
            }
        }else
        {
            if(battleDataforLocal.is_myturn != false)
            {
                battleDataforLocal.is_myturn = false;
                // 自分のターンではないので、相手のターンを勝手に終わらせないようローカルタイマーを停止する
                isTimerRunning = false;
                // テキスト表示も相手の番であることを示す
                gametext.text = "opponent turn";
            }
        }

        if (inputData.space_key_ispressed)
        {
            EndMyTurn();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            sceneData.next_scene_number = 6;
        }

        if (battleDataforOnline.is_finished)
        {
            sceneData.next_scene_number = 6;
        }

        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                // 前のフレームからの経過時間を引く
                currentTime -= Time.deltaTime;
                // Sliderに反映
                timerSlider.value = currentTime;
            }
            else
            {
                Debug.Log("タイムアップ！");
                currentTime = 0;
                isTimerRunning = false;
                OnTimeUp();
            }
        }
    }


    public async void SetFirstGameData()// Awake時初期化用関数
    {
        if (roomData == null) {
                Debug.LogError("[BattleOnlineManager] roomDataが見つかりません。");
                return;
            }
            if (battleConnector == null) {
                Debug.LogError("[BattleOnlineManager] battleConnectorが見つかりません。");
                return;
            }

            Debug.Log($"[BattleOnlineManager] Calling GetGameData for room_id: {roomData.room_id}");
            var gameData = await battleConnector.GetGameData(roomData.room_id);// ここで初期化用データ取得
            if (gameData == null)
            {
                Debug.LogError("[BattleOnlineManager] ゲームデータの取得に失敗しました。");
                return;
            }
            Debug.Log("[BattleOnlineManager] GameData received successfully. Player1Id=" + gameData.Player1Id);

            // プレイヤー情報やコスト、HP初期値はゲームデータ作成時にサーバー側で代入済み
            // ここではデータを受け取ってbattleDataForOnlineを更新するだけ
            // レート情報はサーバー側にいつ代入されるのか？

            // 1p2pのユーザーネームを取得して反映（初回のみ実行のためここに記述）
            var user1 = await authenticationConnector.GetUser(gameData.Player1Id);
            var user2 = await authenticationConnector.GetUser(gameData.Player2Id);
            battleDataforOnline.player1.player_name = user1?.Name ?? "1P";
            battleDataforOnline.player2.player_name = user2?.Name ?? "2P";
            battleDataforOnline.player1.player_id = user1?.Id ?? "unknown";
            battleDataforOnline.player2.player_id = user2?.Id ?? "unknown";

            // キャラクターデータを振り分ける（初回のみ実行のためここに記述）
            int player1Idx = 0;
            int player2Idx = 0;// インデックスは両方0..2
            foreach (var c in gameData.Characters)
            {
                if (c.Is1P)
                {
                    battleDataforOnline.player1.characters[player1Idx].unique_id = (int)c.CharacterId;
                    player1Idx++;
                }
                else if (!c.Is1P)
                {
                    battleDataforOnline.player2.characters[player2Idx].unique_id = (int)c.CharacterId;
                    player2Idx++;
                }
            }


        // 全体のコストやHPなどを更新・ログ表示
        ReceiveBattleData(gameData);

        // キャラの配置・モデル表示を初期化
        battleViewManager.SetupCharacters(gameData.Characters);
        Debug.Log("[BattleOnlineManager] CharacterManager.InitCharacterUI finished.");

        // UI（スライダーや名前）を最新データで更新
        if (battleViewManager != null)
        {
            battleViewManager.InitUI();
            // battleView.SubscribeToEvents();いらない気がする。不具合生じたら戻す
            Debug.Log("[BattleOnlineManager] BattleViewManager.InitUI finished.");
        }

    }


    public void ReceiveBattleData(Game.Network.GameDataResponse gameData)// バトル中の相互ストリーミング用関数
    {
        // サーバーの情報をBattleDataForOnlineに代入する。基本的にbattleDataForOnlineのデータはここでのみ代入され、サーバーと一致している状態を維持する
        SetBattleDataForOnline(gameData);

        // ゲーム終了時の処理
        if (gameData.IsFinished)
        {
            Debug.Log($"<color=yellow>[GetBattleData] Game Finished! WinnerID={gameData.WinnerPlayerId} (P1={gameData.Player1Id}, P2={gameData.Player2Id})</color>");
            
            // レート更新情報の反映
            battleDataforOnline.player1.rate_updown = gameData.P1RateDelta;
            battleDataforOnline.player2.rate_updown = gameData.P2RateDelta;
            battleDataforOnline.player1.rate = gameData.P1Rate;
            battleDataforOnline.player2.rate = gameData.P2Rate;
            Debug.Log($"[RateSync] P1: {gameData.P1Rate} (+{gameData.P1RateDelta}), P2: {gameData.P2Rate} (+{gameData.P2RateDelta})");
            int 自分のレートはどこで更新されるのか要確認 = 0;
            //Debug.Log($"<color=yellow>[GetBattleData] Game End: MyRate={battleDataforOnline.self.rate}({battleDataforOnline.self.rate_updown}) OppRate={battleDataforOnline.opponent.rate}({battleDataforOnline.opponent.rate_updown})</color>");
        }


        // 攻撃情報の受け取り
        if (gameData.GameActionLog?.AttackDetail != null && gameData.GameActionLog.AttackDetail.TargetCharacterUniqueIds.Count > 0)
        {
            foreach (var tcuid in gameData.GameActionLog.AttackDetail.TargetCharacterUniqueIds)
            {
                // すでに処理した情報だった場合は受け取らない
                if (gameData.GameActionLog.Sequence <= consumed_attack_id) break;
                consumed_attack_id = gameData.GameActionLog.Sequence;
                Debug.Log($"<color=red><b>[GetBattleData] 攻撃情報を受信</b>: FromSide={gameData.GameActionLog.PlayerId}, AttackerID={gameData.GameActionLog.ActorCharacterUniqueId}, Type={gameData.GameActionLog.AttackDetail.AttackType}</color>");
                

                // イベントの発火 (攻撃ログ?)
                /*
                OnAttackExecuted?.Invoke(new AttackEventData {
                    attackerUniqueId = ai.AttackerCharacterId,
                    targetUniqueId = 0, // 不明
                    finalDamage = 0,    // 不明（HP同期側で検知可能）
                    attackType = ai.AttackType,
                    isPlayerAttack = false
                });
                */

                // ここで攻撃されたキャラに演出をする
            }
        }
        // 【デバッグ】受信データをコンソールに出力（UIには反映しない）
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<color=cyan>[GetBattleData] サーバーからゲームデータ受信</color>");
        sb.AppendLine($"  1PTurn={gameData.Is1PTurn}  BaseHp1={gameData.BaseHp1}  BaseHp2={gameData.BaseHp2}  IsFinished={gameData.IsFinished}  Winner={gameData.WinnerPlayerId}");
        foreach (var c in gameData.Characters)
        {
            sb.AppendLine($"  Chara: UniqueId={c.Id}  Is1P={c.Is1P}  CharaId={c.CharacterId}  HP={c.Hp}  PosX={c.PositionX}  PosY={c.PositionY}");
        }
        Debug.Log(sb.ToString());
    }

    // 同期時にサーバーから受け取った情報は、この関数を通してSOにセットする。するのはセットだけでイベント通知や具体的な処理は書かない
    public async void SetBattleDataForOnline(Game.Network.GameDataResponse gameData)
    {
        if (gameData == null) return;

        battleDataforOnline.is_finished = gameData.IsFinished;
        battleDataforOnline.winner_player_id = gameData.WinnerPlayerId;
        
        // ターン順
        battleDataforOnline.is_1p_turn = gameData.Is1PTurn;

        // 拠点HP
        battleDataforOnline.player1.base_hp = (int)gameData.BaseHp1;
        battleDataforOnline.player2.base_hp = (int)gameData.BaseHp2;

        // コストをサーバーから反映
        battleDataforOnline.player1.current_cost_remaining = (int)gameData.Cost1P;
        battleDataforOnline.player2.current_cost_remaining = (int)gameData.Cost2P;

        // キャラクターのデータを反映（UniqueIdによるマッチング）
        foreach (var c in gameData.Characters)
        {
            bool is_1p = (userData.user_id == gameData.Player1Id);
            // SetCharacterDataはここでのみ呼び出せばOKなように作ってます
            SetCharacterData(c, is_1p);
        }

        // デバフマスの受け取り
        // 毎回上書きする
        battleDataforOnline.debuffGrids.Clear();
        foreach (var g in gameData.Grids)
        {
            DebuffGrid debuffGrid = new DebuffGrid();
            debuffGrid.position = new Vector2Int((int)g.PositionX, (int)g.PositionY);
            debuffGrid.debuffType = g.DebuffType;
            battleDataforOnline.debuffGrids.Add(debuffGrid);
            Debug.Log($"DebuffGrid added. {debuffGrid.position} Type: {debuffGrid.debuffType}");
        }
    }

    // battleDataForOnlineの汚染を防ぐため、基本的に↑のみで用いる関数
    void SetCharacterData(Game.Network.UniqueCharacter c, bool is_1p)
    {
        // 1pと2pの処理分岐用。同IDキャラの混線を防止する役割も
        PlayerState player = c.Is1P ? battleDataforOnline.player1 : battleDataforOnline.player2;
        // unique_idはAwake時に代入されているので、これを用いてマッチング
        for (int i = 0; i <= 2; i++)
        {
            if (player.characters[i].unique_id == c.CharacterId)// 各プレイヤーのキャラ3枠でIDが一致したキャラ
            {
                int oldHp = player.characters[i].now_character_hp;
                int newHp = (int)c.Hp;
                if (oldHp != newHp)
                {
                    Debug.Log($"<color=red>[GetBattleData] HP同期: idx={i} uniqueId={c.CharacterId} {oldHp} -> {newHp}</color>");
                }

                // hpの同期
                player.characters[i].now_character_hp = newHp;

                // キャラ座標の同期（自分が2pなら反転して管理）
                Vector2Int converted = gridManager.ConvertCoordinateForServer((int)c.PositionX, (int)c.PositionY, is_1p);
                player.characters[i].now_character_position = converted;

                // 選択状態の同期
                // キャラ選択状態はバックは持たず、自環境での処理のみに用います

                // 移動コストの同期（デフォルトコストを代入しデバフによる増減を計算。デバフがないときはこれがそのまま移動コストになる。）
                player.characters[i].now_character_move_cost = characterData.characters[c.CharacterId].default_move_cost;
                // デバフの同期
                if (c.Conditions != null && c.Conditions.Count > 0)
                {
                    foreach (Game.Network.CharacterCondition cond in c.Conditions)
                    {
                        for (int j = 0; j <= 7; i++)
                        {
                            // ConditionIdはbattleDataForOnlineのindexと一致している想定です
                            if (cond.ConditionId == j)
                            {
                                battleDataforOnline.player1.characters[i].debuffs[j] = true;

                                if(cond.ConditionId == 1)// 俊足
                                {
                                    player.characters[i].now_character_move_cost = characterData.characters[c.CharacterId].default_move_cost - 2;
                                }
                                else if(cond.ConditionId == 5)// 鈍足
                                {
                                    player.characters[i].now_character_move_cost = characterData.characters[c.CharacterId].default_move_cost + 2;
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public void StartMyTurn()
    {
        gametext.text = "your turn";
        StartCoroutine(MoveRoutine());
        TimerStart();
    }

    public void EndMyTurn()
    {
        // character_isSelectedだけはサーバーに送らないフロントのみのブールなので直接書き換えます
        for (int i = 0; i < 3; i++) battleDataforOnline.player1.characters[i].character_isSelected = false;
        for (int i = 0; i < 3; i++) battleDataforOnline.player2.characters[i].character_isSelected = false;
        gametext.text = "turn end";
        StartCoroutine(MoveRoutine());

        int 自ターン終了時のデバフ処理はどこでやるか要検討 = 0;
        // 例えば1pはこのように処理する
        for (int i = 0; i <= 2; i++)
        {
            if(battleDataforOnline.player1.characters[i].debuffs[3])
            {
                // サーバーに毒ダメージでのhp20減少を通知する処理
            }
        }

        // サーバーにターン終了を通知する
        if (characterManager != null) characterManager.NotifyTurnEnd();
        // サーバーではターン終了時にターン数増加、1p2pターン切り替え（Is1pTurn）、1p2pコストなどが自動更新される。その情報が次のUpdate時に自身に入ってくる

        // ターン終了直後にサーバーからの古い「自ターンのまま」のデータで上書きされないよう猶予を作る
        int この猶予も必要か要検討 = 0;
        _turnTransitionTime = 2.0f;

        // タイマーを念のため止める
        isTimerRunning = false;
    }

    void TimerStart()
    {
        currentTime = maxTime;
        timerSlider.maxValue = maxTime;
        timerSlider.value = maxTime;
        isTimerRunning = true;
    }

    void OnTimeUp()
    {
        if (battleDataforLocal.is_myturn)
        {
            EndMyTurn();
        }
    }

    IEnumerator MoveRoutine()
    {
        SEManager.instance?.PlayStartTurnSE();
        is_text_moving = true;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; // 0.0 ～ 1.0

            // 【ここがポイント！】中間で緩やかになるカスタム曲線
            // 3次関数を使って「S字を横に倒したような形」を作ります
            float easedT = t * t * (3f - 2f * t); // 基本のスムーズ曲線
        
            // もしもっと極端に「中間で止まりそう」にしたいなら、
            // サイン波を使って t の進み具合を調整します
            // 下記は「0.5付近で時間の進みが遅くなる」計算の一例です
            float slowingT = t + Mathf.Sin(t * Mathf.PI * 2f) * 0.15f; 
            // ※ 0.15f の値を大きくすると、中間での減速がより強くなります

            gameTextObject.anchoredPosition = Vector2.Lerp(startPosition, destination, slowingT);

            yield return null;
        }
        gameTextObject.anchoredPosition = destination;

        is_text_moving = false;
    }

}
