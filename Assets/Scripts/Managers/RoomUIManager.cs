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
    public GameConnector gameConnector;

    public string room_name;

    void Awake()
    {
        gameConnector = FindFirstObjectByType<GameConnector>().GetComponent<GameConnector>();
        UpDateRoom();
    }
    
    public async void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                sceneData.next_scene_number = 3;
                break;
            case "StartBattle":
                await gameConnector.StartMatch(roomData.room_id);
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

    public async void UpDateRoom()
    {
        Debug.Log("UpdateRoom実行");
        for (int i = 0; i <= 7; i++)
        {
        roomData.usersData[i].user_state = -1;
        joinnersUI[i].sprite = joinnersUIImage[roomData.usersData[i].user_state + 1];
        userName[i].text = "募集中...";
        userRate[i].text = "レート： - " ;
        }

        //バックエンドから部屋の情報を取得してください
        var joiner_list = await gameConnector.ListRoom(roomData.room_id);
        var rooms = await gameConnector.GetAllRoomMatch();
        var owner = new Game.Network.UserResponse();
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].RoomId == joiner_list[0].RoomId)
            {
                owner = await gameConnector.GetUser(rooms[i].OwnerId);
                Debug.Log($"owner: {owner}");
                roomData.room_name = owner.Name + "の部屋";
            }
        }
        for (int i = 0; i < joiner_list.Count; i++)
        {
            var user = await gameConnector.GetUser(joiner_list[i].UserId);
            roomData.usersData[i].user_name = user.Name;
            roomData.usersData[i].user_rate = user.Rate;
            roomData.usersData[i].is_host = (owner.Id == user.Id) ? true : false;
            roomData.usersData[i].user_state = joiner_list[i].State;
        }

        for (int i = 0; i <= 7; i++)
        {
        joinnersUI[i].sprite = joinnersUIImage[roomData.usersData[i].user_state + 1];
        }
    }
}
