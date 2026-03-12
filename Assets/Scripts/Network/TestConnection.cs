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
                    Debug.Log($"RoomID:{u.RoomId} ,\nName: {u.RoomName} ,\nID: {u.OwnerId},\nIsPrivate: {u.IsPrivate}\n");
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

}


