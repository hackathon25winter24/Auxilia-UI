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

    async void Awake()
    {
        gameConnector = FindFirstObjectByType<GameConnector>().GetComponent<GameConnector>();
        characterManager = FindFirstObjectByType<CharacterManager>();
        battleDataforLocal.is_myturn = false;
        //ここに先行プレイヤーかどうかを受け取る関数を書いてください
        is_move_player = await GetFirstMovePlayer();// 自身が行動できるターンの時にtrueを返すはず。動作未検証
    }

    void Start()
    {
        gametext.text = "battle start!";
        StartCoroutine(MoveRoutine());
    }

    void Update()
    {
        if (is_text_moving) return;
        if (battleDataforOnline.now_moving_player == battleDataforOnline.my_player_id)
        {
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
        StartCoroutine(MoveRoutine());
        battleDataforOnline.now_my_cost = 50;
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

        StartOpponentTurn();
    }

    public void StartOpponentTurn()
    {
        TimerStart();
    }

    public void EntOpponentTurn()
    {
        StartMyTurn();
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
        }else
        {
            EntOpponentTurn();
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
