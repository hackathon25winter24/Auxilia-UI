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
    public Vector2 exitPosition = new Vector2(1000, 0);
    public Vector2 exitPosition_left = new Vector2(-1000, 0);
    public float duration = 0.5f;
    private bool isExiting = false;

    void Start()
    {
        playerName.text = playerData.player_name;
        playerRate.text = "Rate:" + playerData.player_rate.ToString();
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
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
