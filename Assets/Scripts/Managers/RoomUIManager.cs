using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class RoomUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public RoomData roomData;

    public Image[] joinnersUI;
    public Sprite[] joinnersUIImage;
    
    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                sceneData.next_scene_number = 3;
                break;
            case "StartBattle":
                sceneData.next_scene_number = 10;
                break;
            case "ReRoad":
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    public void UpDateRoom()
    {
        //バックエンドから部屋の情報を取得してください
        for (int i = 0; i <= 8; i++)
        {
        joinnersUI[i].sprite = joinnersUIImage[roomData.usersData[i].user_state];
        }
    }
}
