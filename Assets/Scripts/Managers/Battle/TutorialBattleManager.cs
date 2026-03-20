using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Threading.Tasks;

public class TutorialBattleManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public BattleDataforLocal battleDataforLocal;
    public StoryManagerData storyManagerData;
    public TextMeshProUGUI gameText;
    public RectTransform gametext;

    public Slider timerSlider; 
    public float maxTime = 60f; 
    private float currentTime;
    private bool isTimerRunning = false;
    public bool is_text_moving = false;
    Vector2 startPosition = new Vector2(1000, 0);
    Vector2 destination = new Vector2(-1000, 0);
    public float duration = 2.0f;
    public float elapsed = 0f;

    void Start()
    {
        gameText.text = "battle start!";
        StartCoroutine(MoveRoutine());
        StartMyTurn();
    }

    void Update()
    {
        if(storyManagerData.Tutorial_progress > 2)
        {
        if (inputData.space_key_ispressed)
        {
            EndMyTurn();
        }
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
                EndMyTurn();
            }
        }
    }

    public void StartMyTurn()
    {
        gameText.text = "your turn";
        StartCoroutine(MoveRoutine());
        battleDataforLocal.is_myturn = true;
        battleDataforLocal.now_my_cost = 50;
        TimerStart();
    }

    public void EndMyTurn()
    {
        gameText.text = "turn end";
        StartCoroutine(MoveRoutine());
        battleDataforLocal.is_myturn = false;
        StartMyTurn();
    }

    void TimerStart()
    {
        currentTime = maxTime;
        timerSlider.maxValue = maxTime;
        timerSlider.value = maxTime;
        isTimerRunning = false;
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

        gametext.anchoredPosition = Vector2.Lerp(startPosition, destination, slowingT);

        yield return null;
    }
    gametext.anchoredPosition = destination;

    is_text_moving = false;
}
}
