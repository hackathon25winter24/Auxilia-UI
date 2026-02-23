using UnityEngine;
using TMPro;
using System.Collections;

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
    public Vector2 exitPosition = new Vector2(600, 0);
    public Vector2 exitPosition_left = new Vector2(-600, 0);
    public Vector2 enterPosition = new Vector2(300, 0);
    public Vector2 enterBackBottunPosition = new Vector2(300, 200);
    public Vector2 enterPosition_left = new Vector2(-300, 0);
    public Vector2 exitBackBottunPosition = new Vector2(600, 200);
    public float duration = 0.5f;
    private bool isExiting = false;

    void Start()
    {
        playerName.text = playerData.player_name;
        playerRate.text = "Rate:" + playerData.player_rate.ToString();
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
            uiContainer_left.anchoredPosition = Vector2.Lerp(startPos_left, exitPosition_left, t);
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
            uiContainer_left.anchoredPosition = Vector2.Lerp(startPos_left, enterPosition_left, t);
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
    public void OnButtonClick(string buttonName)
    {
        if (isExiting) return; // 退場中なら何もしない
        isExiting = true;
        switch (buttonName)
        {
            case "Battle":
                StartCoroutine(AnimateExit(3));
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
                StartCoroutine(AnimateExit(-1));
                StartCoroutine(AnimateBackKeyEnter());
                break;
            case "Back":
                StartCoroutine(AnimateEnter());
                StartCoroutine(AnimateBackKeyExit());
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
