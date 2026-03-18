using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class BattleOnlineManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public BattleDataforOmline battleDataforOnline;
    public BattleDataforLocal battleDataforLocal;
    public TextMeshProUGUI gametext;
    public RectTransform gameTextObject;

    public Slider timerSlider; 
    public float maxTime = 60f; 
    private float currentTime;
    private bool isTimerRunning = false;

    Vector2 startPosition = new Vector2(1000, 0);
    Vector2 destination = new Vector2(-1000, 0);
    public float duration = 2.0f;
    public float elapsed = 0f;

    void Start()
    {
        gametext.text = "battle start!";
        StartCoroutine(MoveRoutine());
    }

    void Update()
    {
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
        battleDataforLocal.is_myturn = true;
        battleDataforOnline.now_my_cost = 50;
        TimerStart();
    }

    public void EndMyTurn()
    {
        battleDataforLocal.is_myturn = false;
        for (int i = 0; i <= 2; i++)
        {
            if(battleDataforOnline.charactersBattleDatas[i].debuffs[3])
        {
            battleDataforOnline.charactersBattleDatas[i].now_character_hp -= 20;
        }
        }
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
    Vector2 startPosition = gameTextObject.anchoredPosition;
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
}
}
