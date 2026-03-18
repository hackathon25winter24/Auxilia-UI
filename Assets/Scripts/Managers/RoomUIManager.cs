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
    public TextMeshProUGUI[] userName;
    public TextMeshProUGUI[] userRate;

    void Awake()
    {
        UpDateRoom();
    }
    
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
                UpDateRoom();
                break;
            case "Spectator":
                roomData.usersData[roomData.room_my_number].user_state = 0;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    public void UpDateRoom()
    {
        for (int i = 0; i <= 7; i++)
        {
        roomData.usersData[i].user_state = -1;
        joinnersUI[i].sprite = joinnersUIImage[roomData.usersData[i].user_state + 1];
        userName[i].text = "募集中...";
        userRate[i].text = "レート： - " ;
        }

        //バックエンドから部屋の情報を取得してください

        for (int i = 0; i <= 7; i++)
        {
        joinnersUI[i].sprite = joinnersUIImage[roomData.usersData[i].user_state + 1];
        }
    }
}
