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
    public RectTransform CharacterUI;
    public RectTransform BigBackButton;
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
        playerRate.text = "レート:" + playerData.player_rate.ToString();
        HomeCharacterSetUI.gameObject.SetActive(false);
        StartCoroutine(AnimateEnter());
        BigBackButton.gameObject.SetActive(false);
    }
    IEnumerator AnimateExit()
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

    public void OnButtonClick(string buttonName)
    {
        if (isExiting) return; // 退場中なら何もしない
        isExiting = true;
        switch (buttonName)
        {
            case "Battle":
                isPlayerNameRemain = true;
                StartCoroutine(AnimateExit());
                StartCoroutine(AnimateBattleUIEnter());
                break;
            case "Story":
                StartCoroutine(AnimateExit());
                break;
            case "Character":
                isPlayerNameRemain = true;
                StartCoroutine(AnimateExit());
                StartCoroutine(AnimateCharacterUIEnter());
                break;
            case "HomeCharacter":
                isPlayerNameRemain = false;
                StartCoroutine(AnimateExit());
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
                StartCoroutine(AnimateBattleUIExit(-1));
                break;
            case "RandomMatch":
                isPlayerNameRemain = false;
                StartCoroutine(AnimateBattleUIExit(3));
                break;
            case "BackFromCharacter":
                StartCoroutine(AnimateEnter());
                StartCoroutine(AnimateCharacterUIExit(-1));
                break;
            case "RoomMatch":
                isPlayerNameRemain = false;
                StartCoroutine(AnimateBattleUIExit(3));
                break;
            case "ToAllImage":
                isPlayerNameRemain = false;
                StartCoroutine(AnimateExit());
                BigBackButton.gameObject.SetActive(true);
                isExiting = false;
                break;
            case "BackButton":
                StartCoroutine(AnimateEnter());
                BigBackButton.gameObject.SetActive(false);
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
