using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Threading.Tasks;

public class BattleOnlineManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public BattleDataforOmline battleDataforOnline;
    public BattleDataforLocal battleDataforLocal;
    public RoomData roomData;
    public CharacterData characterData;

    public TextMeshProUGUI gametext;
    public RectTransform gameTextObject;

    public GameConnector gameConnector;

    public Slider timerSlider; 
    public float maxTime = 60f; 
    private float currentTime;
    private bool isTimerRunning = false;

    Vector2 startPosition = new Vector2(1000, 0);
    Vector2 destination = new Vector2(-1000, 0);
    public float duration = 2.0f;
    public float elapsed = 0f;
    public bool is_text_moving;
    public bool is_move_player;

    private T GetSo<T>(T existing) where T : ScriptableObject
    {
        if (existing != null) return existing;
        var targets = Resources.FindObjectsOfTypeAll<T>();
        if (targets.Length > 0) return targets[0];
        return null;
    }

    async void Awake()
    {
        roomData = GetSo(roomData);
        playerData = GetSo(playerData);
        gameConnector = FindFirstObjectByType<GameConnector>().GetComponent<GameConnector>();
        characterManager = FindFirstObjectByType<CharacterManager>();
        battleDataforLocal.is_myturn = false;

        // サーバーからゲームデータを取得して初期化する
        if (roomData == null) {
            Debug.LogError("[BattleOnlineManager] roomDataが見つかりません。");
            return;
        }
        var gameData = await gameConnector.GetGameData(roomData.room_id);
        if (gameData == null)
        {
            Debug.LogError("[BattleOnlineManager] ゲームデータの取得に失敗しました。");
            return;
        }

        bool is1p = (playerData.user_id == gameData.Player1Id);

        // 自分のプレイヤーIDをScriptableObjectに保存
        battleDataforOnline.my_player_id = is1p ? 0 : 1;

        // ターン順：1Pターンなら now_moving_player=0(1P), 2Pターンなら1
        battleDataforOnline.now_moving_player = gameData.Is1PTurn ? 0 : 1;

        // 拠点HP
        battleDataforOnline.base_hp          = is1p ? (int)gameData.BaseHp1 : (int)gameData.BaseHp2;
        battleDataforOnline.opponent_base_hp = is1p ? (int)gameData.BaseHp2 : (int)gameData.BaseHp1;

        // スタート時はどちらもコスト50に初期化
        battleDataforOnline.now_my_cost = 50;
        battleDataforOnline.now_enemy_cost = 50;

        // 相手の名前を取得してScriptableObjectに保存
        string opponentId = is1p ? gameData.Player2Id : gameData.Player1Id;
        var opponentUser = await gameConnector.GetUser(opponentId);
        if (opponentUser != null)
        {
            battleDataforLocal.enemy_name         = opponentUser.Name;
            battleDataforOnline.opponent_name     = opponentUser.Name;
        }

        // キャラクターデータを振り分ける
        // character_id[0..2] = 自分、[3..5] = 相手
        int myIdx = 0, opIdx = 3;
        foreach (var c in gameData.Characters)
        {
            bool charIsMine = (is1p == c.Is1P);
            if (charIsMine && myIdx < 3)
            {
                battleDataforLocal.character_id[myIdx] = (int)c.CharacterId;
                myIdx++;
            }
            else if (!charIsMine && opIdx < 6)
            {
                battleDataforLocal.character_id[opIdx] = (int)c.CharacterId;
                opIdx++;
            }
        }

        // 先行判定（ローカルフラグ）
        is_move_player = is1p ? gameData.Is1PTurn : !gameData.Is1PTurn;

        // 全体のコストやHPなどを CharacterManager を通して更新・ログ表示
        characterManager.GetBattleData(gameData);

        // すべてのデータが揃ったので、UIを初期化する
        characterManager.InitCharacterUI();
        var battleUI = FindFirstObjectByType<BattleUIManager>();
        if (battleUI != null) battleUI.InitUI();

        // バトル開始演出を開始
        gametext.text = "battle start!";
        StartCoroutine(MoveRoutine());
    }
    private float _turnTransitionTime = 0f; // ターン切り替え時の猶予時間
    
    void Update()
    {
        if (is_text_moving) return;

        if (_turnTransitionTime > 0)
        {
            _turnTransitionTime -= Time.deltaTime;
        }
        if (battleDataforOnline.now_moving_player == battleDataforOnline.my_player_id)
        {
            // ターン終了直後（猶予時間中）であれば、サーバーからの自ターン継続情報を無視する
            if (_turnTransitionTime > 0) return;
            if (battleDataforLocal.is_myturn != true)
            {
            Debug.Log($"<color=cyan>[BattleOnlineManager] 自分のターン開始 (now_moving_player={battleDataforOnline.now_moving_player})</color>");
            battleDataforLocal.is_myturn = true;
            StartMyTurn();
            }
        }else
        {
            if(battleDataforLocal.is_myturn != false)
            {
                Debug.Log($"<color=silver>[BattleOnlineManager] 相手のターン開始 (now_moving_player={battleDataforOnline.now_moving_player})</color>");
                battleDataforLocal.is_myturn = false;
                // 自分のターンではないので、相手のターンを勝手に終わらせないようローカルタイマーを停止する
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

        if (battleDataforOnline.game_end)
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

    public void StartMyTurn()
    {
        gametext.text = "your turn";
        // ターン開始時にコストを50回復（最大値などは現状設けていないため加算）
        battleDataforOnline.now_my_cost += 50;
        Debug.Log($"<color=cyan>[BattleOnlineManager] ターン開始: コストが {battleDataforOnline.now_my_cost} になりました</color>");
        StartCoroutine(MoveRoutine());
        TimerStart();
    }

    private CharacterManager characterManager;

    public void EndMyTurn()
    {
        Debug.Log("<color=silver>[BattleOnlineManager] EndMyTurn 実行</color>");
        gametext.text = "turn end";
        // 次のターンを算出する
        int nextPlayerId = CalculateNextTurn();
        battleDataforOnline.now_moving_player = nextPlayerId;
        Debug.Log($"<color=cyan>[BattleOnlineManager] フロント側で次ターンを算出: {nextPlayerId}</color>");

        StartCoroutine(MoveRoutine());

        for (int i = 0; i <= 2; i++)
        {
            if(battleDataforOnline.charactersBattleDatas[i].debuffs[3])
        {
            battleDataforOnline.charactersBattleDatas[i].now_character_hp -= 20;
        }
        }

        // サーバーにターン終了を通知する
        if (characterManager != null) characterManager.NotifyTurnEnd();

        // ターン終了直後にサーバーからの古い「自ターンのまま」のデータで上書きされないよう猶予を作る
        // (通信ラグを考慮して3秒に延長)
        _turnTransitionTime = 3.0f;

        // ターン終了時の自動回復は廃止（開始時のみにする）
        // battleDataforOnline.now_my_cost = 50;

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
        // テキストを画面外から中央へ、そして中央から反対側へ移動させる
        yield return StartCoroutine(MoveTextRoutine(new Vector2(-1500, 0), new Vector2(0, 0), 1.5f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(MoveTextRoutine(new Vector2(0, 0), new Vector2(1500, 0), 1.5f));
    }

    private IEnumerator MoveTextRoutine(Vector2 startPosition, Vector2 destination, float duration)
    {
        is_text_moving = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float slowingT = t + Mathf.Sin(t * Mathf.PI * 2f) * 0.15f; 
            gameTextObject.anchoredPosition = Vector2.Lerp(startPosition, destination, slowingT);
            yield return null;
        }
        gameTextObject.anchoredPosition = destination;
        is_text_moving = false;
    }

    public async Task<bool> GetFirstMovePlayer()
    {
        var game_data = await gameConnector.GetGameData(roomData.room_id);
        string p1 = game_data.Player1Id;
        string p2 = game_data.Player2Id;
        bool is_1p_turn = game_data.Is1PTurn;
        if (playerData.user_id == p1 && is_1p_turn || playerData.user_id == p2 && !is_1p_turn)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int CalculateNextTurn()
    {
        // バックエンドの determineNextActor と同じロジックを実装
        // 1Pと2Pの生存キャラクターの中で、最も基本コストが低い値を探す
        int p1Min = GetMinAliveMoveCost(true);
        int p2Min = GetMinAliveMoveCost(false);

        Debug.Log($"[CalculateNextTurn] P1Min={p1Min}, P2Min={p2Min}, current={battleDataforOnline.my_player_id}");

        if (p1Min < p2Min) return 0;
        if (p2Min < p1Min) return 1;

        // 同値の場合は反転させる (強制交代)
        return (battleDataforOnline.now_moving_player == 0) ? 1 : 0;
    }

    private int GetMinAliveMoveCost(bool is1P)
    {
        int minCost = 999;
        bool found = false;
        int start = is1P ? 0 : 3;
        int end = is1P ? 2 : 5;

        for (int i = start; i <= end; i++)
        {
            if (battleDataforOnline.charactersBattleDatas[i].now_character_hp > 0)
            {
                int cost = characterData.characters[battleDataforLocal.character_id[i]].attacks[0].default_attack_cost; // FIXME: use move cost if available
                // 実際は characterData.characters[id].default_move_cost を使うべき
                int moveCost = characterData.characters[battleDataforLocal.character_id[i]].default_move_cost;
                if (!found || moveCost < minCost)
                {
                    minCost = moveCost;
                    found = true;
                }
            }
        }
        return found ? minCost : 999;
    }
}
