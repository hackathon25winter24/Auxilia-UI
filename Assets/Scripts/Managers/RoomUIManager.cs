using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class RoomUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    
    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                sceneData.next_scene_number = 3;
                break;
            case "StartBattle":
                sceneData.next_scene_number = 5;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
