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
        // ターン開始時にコストを50にリセット
        battleDataforOnline.now_my_cost = 50;
        StartCoroutine(MoveRoutine());
        TimerStart();
    }

    private CharacterManager characterManager;

    public void EndMyTurn()
    {
        gametext.text = "turn end";
        if(battleDataforOnline.now_moving_player == 0)
        {
            battleDataforOnline.now_moving_player = 1;
        }else
        {
            battleDataforOnline.now_moving_player = 0;
        }
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
        _turnTransitionTime = 2.0f;

        // ターン終了時にコストを50まで回復
        battleDataforOnline.now_my_cost = 50;

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
}
