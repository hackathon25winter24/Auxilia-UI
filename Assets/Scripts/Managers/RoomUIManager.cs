using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using Grpc.Core;
using Room;

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
    private TextMeshProUGUI startBattleButtonText;

    private AsyncDuplexStreamingCall<RoomStreamRequest, ListRoomResponse> _roomStream;
    private bool _isStreaming;
    private bool _pendingRoomUpdate;

    void Update()
    {
        if (_pendingRoomUpdate)
        {
            _pendingRoomUpdate = false;
            UpDateRoom();
        }
    }

    private void OnDestroy()
    {
        _isStreaming = false;
        if (_roomStream != null)
        {
            _roomStream.RequestStream.CompleteAsync();
            _roomStream.Dispose();
            _roomStream = null;
        }
    }

    public async void StartRealtimeSync()
    {
        if (_isStreaming) return;
        _isStreaming = true;

        try 
        {
            _roomStream = gameConnector.StreamRoom();
            await _roomStream.RequestStream.WriteAsync(new RoomStreamRequest { RoomId = roomData.room_id, UserId = playerData.user_id });

            while (_isStreaming && await _roomStream.ResponseStream.MoveNext(System.Threading.CancellationToken.None))
            {
                var response = _roomStream.ResponseStream.Current;
                _pendingRoomUpdate = true;
            }
        }
        catch (RpcException e)
        {
            Debug.LogError($"StreamRoom Error: {e.Status.Detail}");
        }
    }

    void Awake()
    {
        gameConnector = FindFirstObjectByType<GameConnector>().GetComponent<GameConnector>();
        UpDateRoom();
        StartRealtimeSync();

        // 開発環境でシーン上のOnClick未設定によるボタン無反応を防ぐため、動的にイベントを付与
        var reloadButtonObj = GameObject.Find("ReRoadButton");
        if (reloadButtonObj != null)
        {
            var btn = reloadButtonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnButtonClick("ReRoad"));
            }
        }

        var startBtnObj = GameObject.Find("StartBattleButton") ?? GameObject.Find("StartButton") ?? GameObject.Find("StartBattle");
        if (startBtnObj != null) 
        {
            startBattleButtonText = startBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        }
        else 
        {
            var buttons = Resources.FindObjectsOfTypeAll<Button>();
            foreach(var b in buttons) {
                if(b.name.Contains("Start")) {
                    startBattleButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                    if (startBattleButtonText != null) break;
                }
            }
        }
    }
    
    public async void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                sceneData.next_scene_number = 3;
                break;
            case "StartBattle":
                if (roomData.usersData[roomData.room_my_number].is_host)
                {
                    bool allReady = true;
                    int activeCount = 0;
                    for (int i = 0; i <= 7; i++)
                    {
                        // ホスト以外のプレイ中のユーザー（観戦者以外）が全員readyかどうか判定
                        if (roomData.usersData[i].user_state != -1 && !roomData.usersData[i].is_host && roomData.usersData[i].user_state != 0)
                        {
                            activeCount++;
                            if (!roomData.usersData[i].is_ready) allReady = false;
                        }
                    }

                    if (allReady && activeCount > 0)
                    {
                        await gameConnector.StartMatch(roomData.room_id);
                        sceneData.next_scene_number = 10;
                    }
                    else
                    {
                        Debug.Log("全員が準備完了していないか、対戦相手がいません。");
                    }
                }
                else
                {
                    // ゲスト（親以外）は自分のready状態を切り替える
                    bool newReady = !roomData.usersData[roomData.room_my_number].is_ready;
                    await gameConnector.UpdateRoomState(roomData.room_id, playerData.user_id, roomData.usersData[roomData.room_my_number].user_state, newReady);
                    
                    if (startBattleButtonText != null)
                    {
                        startBattleButtonText.text = newReady ? "対戦開始を待っています..." : "準備完了";
                    }
                    UpDateRoom();
                }
                break;
            case "ReRoad":
                UpDateRoom();
                break;
            case "Spectator":
                await gameConnector.UpdateRoomState(roomData.room_id, playerData.user_id, 0, false);
                UpDateRoom();
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
        
        if (joiner_list == null || rooms == null || joiner_list.Count == 0) return;

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
            if (user.Id == playerData.user_id) roomData.room_my_number = i;

            roomData.usersData[i].user_name = user.Name;
            roomData.usersData[i].user_rate = user.Rate;
            roomData.usersData[i].is_host = (owner.Id == user.Id) ? true : false;
            roomData.usersData[i].user_state = joiner_list[i].State;
            roomData.usersData[i].is_ready = joiner_list[i].IsReady;
        }

        for (int i = 0; i <= 7; i++)
        {
            joinnersUI[i].sprite = joinnersUIImage[roomData.usersData[i].user_state + 1];

            // readyがtrueのユーザーを白く囲む
            var outline = joinnersUI[i].gameObject.GetComponent<Outline>();
            if (outline == null) 
            {
                outline = joinnersUI[i].gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(3, -3);
            }
            
            if (roomData.usersData[i].user_state != -1 && roomData.usersData[i].is_ready)
            {
                outline.enabled = true;
                outline.effectColor = Color.white;
            }
            else
            {
                outline.enabled = false;
            }
        }

        if (startBattleButtonText != null)
        {
            bool amIHost = roomData.usersData[roomData.room_my_number].is_host;
            bool amIReady = roomData.usersData[roomData.room_my_number].is_ready;
            
            if (amIHost)
            {
                startBattleButtonText.text = "対戦開始";
            }
            else
            {
                startBattleButtonText.text = amIReady ? "対戦開始を待っています..." : "準備完了";
            }
        }
    }
}
