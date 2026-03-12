using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ResultUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "BackToHome":
                sceneData.next_scene_number = 1;
                break;
            case "BackToMatchingRoom":
                sceneData.next_scene_number = 9;
                break;
            case "BackToMatching":
                sceneData.next_scene_number = 3;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
