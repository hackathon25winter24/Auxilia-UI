using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class HomeUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerRate;
    public RectTransform uiContainer;
    public RectTransform uiContainer_left;
    public RectTransform backBottun;
    public RectTransform HomeCharacterSetUI;
    public RectTransform BattleUI;
    public Vector2 exitPosition = new Vector2(600, 0);
    public Vector2 exitPosition_left = new Vector2(-600, 0);
    public Vector2 enterPosition = new Vector2(300, 0);
    public Vector2 enterBackBottunPosition = new Vector2(300, 200);
    public Vector2 enterPosition_left = new Vector2(-300, 0);
    public Vector2 exitBackBottunPosition = new Vector2(600, 200);
    public Vector2 enterBattleUIPosition = new Vector2(300, 0);
    public Vector2 exitBattleUIPosition = new Vector2(600, 0);
    public float duration = 0.5f;
    private bool isExiting = false;
    private bool isHomeCharacterSelecting = false;
    public float scrollSpeed = 500f;
    public bool isPlayerNameRemain = false;
    public Sprite Sophie_ui;
    public Sprite Shincho_ui;
    public Sprite Aoi_ui;
    public Sprite Berenice_ui;
    public Sprite Chiyo_ui;
    public Sprite Jude_ui;
    public Sprite Nadia_ui;
    public Sprite Sena_ui;
    public Sprite Tsukiha_ui;
    public Sprite Zina_ui;
    public Image SophieUI;
    public Image ShinchoUI;
    public Image AoiUI;
    public Image BereniceUI;
    public Image ChiyoUI;
    public Image JudeUI;
    public Image NadiaUI;
    public Image SenaUI;
    public Image TsukihaUI;
    public Image ZinaUI;

    void Awake()
    {
        SophieUI.sprite = Sophie_ui;
        ShinchoUI.sprite = Shincho_ui;
        SenaUI.sprite = Sena_ui;
        AoiUI.sprite = Aoi_ui;
        BereniceUI.sprite = Berenice_ui;
        ChiyoUI.sprite = Chiyo_ui;
        JudeUI.sprite = Jude_ui;
        NadiaUI.sprite = Nadia_ui;
        TsukihaUI.sprite = Tsukiha_ui;
        ZinaUI.sprite = Zina_ui;
        playerName.text = playerData.player_name;
        playerRate.text = "Rate:" + playerData.player_rate.ToString();
        HomeCharacterSetUI.gameObject.SetActive(false);
        StartCoroutine(AnimateEnter());
    }
    IEnumerator AnimateExit(int nextScene)
    {
        Vector2 startPos = uiContainer.anchoredPosition;
        Vector2 startPos_left = uiContainer_left.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            uiContainer.anchoredPosition = Vector2.Lerp(startPos, exitPosition, t);
            if (isPlayerNameRemain ==  false)
            {
            uiContainer_left.anchoredPosition = Vector2.Lerp(startPos_left, exitPosition_left, t);
            }
            yield return null;
        }
        Debug.Log("すべてのUIが退場しました");
        if (nextScene != -1) 
        {
        sceneData.next_scene_number = nextScene;
        }
    }
    IEnumerator AnimateEnter()
    {
        Vector2 startPos = uiContainer.anchoredPosition;
        Vector2 startPos_left = uiContainer_left.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            uiContainer.anchoredPosition = Vector2.Lerp(startPos, enterPosition, t);
            if (isPlayerNameRemain ==  false)
            {
            uiContainer_left.anchoredPosition = Vector2.Lerp(startPos_left, enterPosition_left, t);
            }
            yield return null;
        }
        isExiting = false;
    }
    IEnumerator AnimateBackKeyEnter()
    {
        Vector2 startPos_backButton = backBottun.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            backBottun.anchoredPosition = Vector2.Lerp(startPos_backButton, enterBackBottunPosition, t);
            yield return null;
        }
        isExiting = false;
    }
    IEnumerator AnimateBackKeyExit()
    {
        Vector2 startPos_backButton = backBottun.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            backBottun.anchoredPosition = Vector2.Lerp(startPos_backButton, exitBackBottunPosition, t);
            yield return null;
        }
    }
    IEnumerator AnimateBattleUIEnter()
    {
        Vector2 startPos_BattleUI = BattleUI.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            BattleUI.anchoredPosition = Vector2.Lerp(startPos_BattleUI, enterBattleUIPosition, t);
            yield return null;
        }
        isExiting = false;
    }
    IEnumerator AnimateBattleUIExit()
    {
        Vector2 startPos_BattleUI = BattleUI.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            BattleUI.anchoredPosition = Vector2.Lerp(startPos_BattleUI, exitBattleUIPosition, t);
            yield return null;
        }
    }
    public void OnButtonClick(string buttonName)
    {
        if (isExiting) return; // 退場中なら何もしない
        isExiting = true;
        switch (buttonName)
        {
            case "Battle":
                isPlayerNameRemain = true;
                StartCoroutine(AnimateExit(-1));
                StartCoroutine(AnimateBattleUIEnter());
                Debug.Log("battle button was plessed");
                break;
            case "Story":
                StartCoroutine(AnimateExit(-1));
                Debug.Log("story button was plessed");
                break;
            case "Character":
                StartCoroutine(AnimateExit(7));
                break;
            case "HomeCharacter":
                isPlayerNameRemain = false;
                StartCoroutine(AnimateExit(-1));
                StartCoroutine(AnimateBackKeyEnter());
                HomeCharacterSetUI.gameObject.SetActive(true);
                isHomeCharacterSelecting = true;
                break;
            case "Back":
                StartCoroutine(AnimateEnter());
                StartCoroutine(AnimateBackKeyExit());
                HomeCharacterSetUI.gameObject.SetActive(false);
                isHomeCharacterSelecting = false;
                break;
            case "BackfromBattleUI":
                StartCoroutine(AnimateEnter());
                StartCoroutine(AnimateBattleUIExit());
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
    public void Update()
    {
        if (isHomeCharacterSelecting) 
    {
        float scrollInput = inputData.mouse_wheel.y;

        if (scrollInput != 0)
        {
            // 時間経過(Time.deltaTime)を考慮して滑らかに動かす
            float moveAmount = scrollInput * scrollSpeed * Time.deltaTime * -1;
            HomeCharacterSetUI.anchoredPosition += new Vector2(0, moveAmount);
            if (HomeCharacterSetUI.anchoredPosition.y < -250)
            {
                HomeCharacterSetUI.anchoredPosition = new Vector2(0, -250);
            }
            if (HomeCharacterSetUI.anchoredPosition.y > 0)
            {
                HomeCharacterSetUI.anchoredPosition = new Vector2(0, 0);
            }
        }
    }
    }
}
