using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Threading.Tasks;

public class HomeUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerRate;
    public TextMeshProUGUI messageText;
    public RectTransform uiContainer;
    public RectTransform uiContainer_left;
    public RectTransform backBottun;
    public RectTransform HomeCharacterSetUI;
    public RectTransform BattleUI;
    public RectTransform CharacterUI;
    public RectTransform BigBackButton;
    public RectTransform settingUI;
    public Vector2 exitPosition = new Vector2(600, 0);
    public Vector2 exitPosition_left = new Vector2(-600, 0);
    public Vector2 enterPosition = new Vector2(300, 0);
    public Vector2 enterBackBottunPosition = new Vector2(300, 200);
    public Vector2 enterPosition_left = new Vector2(-300, 0);
    public Vector2 exitBackBottunPosition = new Vector2(600, 200);
    public Vector2 enterBattleUIPosition = new Vector2(300, 0);
    public Vector2 exitBattleUIPosition = new Vector2(600, 0);
    public float duration = 0.5f;
    public bool isExiting = false;
    private bool isHomeCharacterSelecting = false;
    public float scrollSpeed = 500f;
    public bool isPlayerNameRemain = false;

    void Awake()
    {
        playerName.text = playerData.player_name;
        playerRate.text = "レート：" + playerData.player_rate.ToString();
        HomeCharacterSetUI.gameObject.SetActive(false);
        StartCoroutine(AnimateEnter());
        BigBackButton.gameObject.SetActive(false);
        settingUI.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
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
        isExiting = false;
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
    IEnumerator AnimateBattleUIExit(int nextScene)
    {
        Vector2 startPos_BattleUI = BattleUI.anchoredPosition;
        Vector2 startPos_left = uiContainer_left.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            BattleUI.anchoredPosition = Vector2.Lerp(startPos_BattleUI, exitBattleUIPosition, t);
            if (isPlayerNameRemain ==  false)
            {
            uiContainer_left.anchoredPosition = Vector2.Lerp(startPos_left, exitPosition_left, t);
            }
            yield return null;
        }
        if (nextScene != -1) 
        {
        sceneData.next_scene_number = nextScene;
        }
    }

    IEnumerator AnimateCharacterUIEnter()
    {
        Vector2 startPos_CharacterUI = CharacterUI.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            CharacterUI.anchoredPosition = Vector2.Lerp(startPos_CharacterUI, enterBattleUIPosition, t);
            yield return null;
        }
        isExiting = false;
    }
    IEnumerator AnimateCharacterUIExit(int nextScene)
    {
        Vector2 startPos_CharacterUI = CharacterUI.anchoredPosition;
        Vector2 startPos_left = uiContainer_left.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t; 

            CharacterUI.anchoredPosition = Vector2.Lerp(startPos_CharacterUI, exitBattleUIPosition, t);
            if (isPlayerNameRemain ==  false)
            {
            uiContainer_left.anchoredPosition = Vector2.Lerp(startPos_left, exitPosition_left, t);
            }
            yield return null;
        }
        if (nextScene != -1) 
        {
        sceneData.next_scene_number = nextScene;
        }
    }

    public async void OnButtonClick(string buttonName)
    {
        if (isExiting) return; // 退場中なら何もしない
        isExiting = true;
        switch (buttonName)
        {
            case "Battle":
                SEManager.instance?.PlayToNextSE();
                isPlayerNameRemain = true;
                StartCoroutine(AnimateExit(-1));
                StartCoroutine(AnimateBattleUIEnter());
                break;
            case "Story":
                SEManager.instance?.PlayToNextSE();
                StartCoroutine(AnimateExit(11));
                break;
            case "Character":
                SEManager.instance?.PlayToNextSE();
                isPlayerNameRemain = true;
                StartCoroutine(AnimateExit(-1));
                StartCoroutine(AnimateCharacterUIEnter());
                break;
            case "HomeCharacter":
                SEManager.instance?.PlaySelectSE();
                isPlayerNameRemain = false;
                StartCoroutine(AnimateExit(-1));
                // StartCoroutine(AnimateBackKeyEnter()); // Removed as we use the modal's close button
                HomeCharacterSetUI.gameObject.SetActive(true);
                isHomeCharacterSelecting = true;
                break;
            case "Back":
                SEManager.instance?.PlayBackSE();
                StartCoroutine(AnimateEnter());
                StartCoroutine(AnimateBackKeyExit());
                HomeCharacterSetUI.gameObject.SetActive(false);
                isHomeCharacterSelecting = false;
                break;
            case "BackfromBattleUI":
                SEManager.instance?.PlayBackSE();
                StartCoroutine(AnimateEnter());
                StartCoroutine(AnimateBattleUIExit(-1));
                break;
            case "RandomMatch":
                SEManager.instance?.PlayToNextSE();
                messageText.text = "開発中...";
                messageText.gameObject.SetActive(true);
                await Task.Delay(1000);
                messageText.gameObject.SetActive(false);
                isExiting = false;
                break;
            case "RateMatch":
                SEManager.instance?.PlayToNextSE();
                messageText.text = "開発中...";
                messageText.gameObject.SetActive(true);
                await Task.Delay(1000);
                messageText.gameObject.SetActive(false);
                isExiting = false;
                break;
            case "BackFromCharacter":
                SEManager.instance?.PlayBackSE();
                StartCoroutine(AnimateEnter());
                StartCoroutine(AnimateCharacterUIExit(-1));
                break;
            case "RoomMatch":
                SEManager.instance?.PlayToNextSE();
                isPlayerNameRemain = false;
                StartCoroutine(AnimateBattleUIExit(3));
                break;
            case "ToAllImage":
                SEManager.instance?.PlaySelectSE();
                isPlayerNameRemain = false;
                StartCoroutine(AnimateExit(-1));
                BigBackButton.gameObject.SetActive(true);
                isExiting = false;
                break;
            case "BackButton":
                SEManager.instance?.PlayBackSE();
                StartCoroutine(AnimateEnter());
                BigBackButton.gameObject.SetActive(false);
                break;
            case "CharacterStates":
                SEManager.instance?.PlaySelectSE();
                isPlayerNameRemain = false;
                StartCoroutine(AnimateCharacterUIExit(7));
                break;
            case "CharacterFormation":
                SEManager.instance?.PlaySelectSE();
                isPlayerNameRemain = false;
                StartCoroutine(AnimateCharacterUIExit(4));
                break;
            case "Setting":
                SEManager.instance?.PlaySelectSE();
                isExiting = false;
                settingUI.gameObject.SetActive(true);
                break;
            case "BackFromSetting":
                SEManager.instance?.PlayBackSE();
                isExiting = false;
                settingUI.gameObject.SetActive(false);
                break;
            case "EndGame":
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
            break;
            case "GotoTitle":
            SEManager.instance?.PlayToNextSE();
            sceneData.next_scene_number = 0;
            break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    /* 
    public void Update()
    {
        if (isHomeCharacterSelecting) 
        {
            // Custom scroll logic replaced by ScrollRect
        }
    }
    */}
