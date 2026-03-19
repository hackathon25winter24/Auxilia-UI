using Game.Network;
using UnityEngine;
using System.Threading.Tasks; // Taskを使うために必要

public class TestConnection : MonoBehaviour
{
    public GameConnector GC;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await TestJoinRoom();
        await TestEnterRing();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Test1()
    {
        if (GC != null)
        {
            var testroom = GC.CreateRoomMatch("TestRoom","f0f5f407-e3ee-4c5a-a1b0-05ec76465d8f", true);
            Debug.Log("TestConnection: CreateRoomMatch called");
            Debug.Log("TestConnection: RoomMatch result: " + (testroom != null ? "Success" : "Failed"));
        }
    }

    async Task Test2()
    {
        if (GC != null)
        {
            var roomMatches = await GC.GetAllRoomMatch();
            if (roomMatches != null)
            {
                //string res = "User List:\n";

                foreach (var u in roomMatches)
                {
                    Debug.Log($"RoomID:{u.RoomId} ,\nName: {u.RoomName} ,\nID: {u.OwnerId},\nIsPrivate: {u.IsGaming}\n");
                }
            }
        }
    }

    async Task TestJoinRoom()
    {
        if (GC != null)
        {
            var result = await GC.JoinRoom(1, "f0f5f407-e3ee-4c5a-a1b0-05ec76465d8f");
            Debug.Log("Join Room Result: " + "Success");
        }
    }
    async Task TestStartGame()
    {
        // 例: RoomID 123, プレイヤーIDを渡して作成
        var response = await GC.CreateGameData(1, "f0f5f407-e3ee-4c5a-a1b0-05ec76465d8f", "7c789797-aabb-477d-962b-6bf8796a0178");

        if (response != null)
        {
            Debug.Log($"Turn: {response.Turn}, 1P Turn?: {response.Is1PTurn}");
            
            // さきほど確認した通り、この時点では Characters は空(Count == 0)のはず
            Debug.Log($"所属キャラクター数: {response.Characters.Count}");
            
            
            if (response.Characters.Count == 0)
            {
                Debug.Log("キャラクターが未登録です。キャラクター選択画面へ移行します。");
                // ここでキャラ選択UIを表示するなどの処理
            }
        }
    }

    async Task TestGetGame()
    {
        var response = await GC.GetGameData(1);

        if (response != null)
        {
            Debug.Log($"Turn: {response.Turn}, 1P Turn?: {response.Is1PTurn}");
            
            // さきほど確認した通り、この時点では Characters は空(Count == 0)のはず
            Debug.Log($"所属キャラクター数: {response.Characters.Count}");
            
            
            if (response.Characters.Count == 0)
            {
                Debug.Log("キャラクターが未登録です。キャラクター選択画面へ移行します。");
                // ここでキャラ選択UIを表示するなどの処理
            }
        }
    }

    async Task TestEnterRing()
    {
        var response = await GC.EnterRing(1,"f0f5f407-e3ee-4c5a-a1b0-05ec76465d8f");
        foreach (var room in response.Rooms)
        {
            Debug.Log($"RoomID:{room.RoomId} ,\nUserID: {room.UserId},\nready: {room.IsReady}\n");   
        }


        

    }

}
