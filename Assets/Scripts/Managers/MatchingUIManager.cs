using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MatchingUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;

    void Start()
    {
        
    }

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                sceneData.next_scene_number = 1;
                break;
            case "newmake":
                sceneData.next_scene_number = 9;
                break;
            case "join":
                sceneData.next_scene_number = 9;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
