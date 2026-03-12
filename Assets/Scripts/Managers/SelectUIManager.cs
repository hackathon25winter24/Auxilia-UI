using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class SelectUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Decided":
                sceneData.next_scene_number = 5;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
