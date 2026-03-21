using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using Grpc.Core;
using Room;
using System.Threading;
using System.Threading.Tasks;

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
    public GameObject renameRoomUI;
    public TMP_InputField renameRoomText;

    private TextMeshProUGUI startBattleButtonText;
    private TextMeshProUGUI roomNameText; // 部屋名表示用
    private GameObject editButton;        // ホスト用編集ボタン

    private AsyncServerStreamingCall<ListRoomResponse> _roomStream;
    private bool _isStreaming;
    private bool _pendingRoomUpdate;
    private CancellationTokenSource _cts;

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
        
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        if (_roomStream != null)
        {
            _roomStream.Dispose();
            _roomStream = null;
        }
    }

    public void StartRealtimeSync()
    {
        if (_isStreaming) return;
        _isStreaming = true;
        _cts = new CancellationTokenSource();

        // 別スレッドで受信を継続する
        _ = Task.Run(async () =>
        {
            while (_isStreaming && !_cts.IsCancellationRequested)
            {
                try 
                {
                    _roomStream = gameConnector.StreamRoom(new RoomStreamRequest { RoomId = roomData.room_id, UserId = playerData.user_id });
                    Debug.Log($"<color=cyan>[StreamRoom] サーバーのリアルタイム同期に接続しました！ (RoomID: {roomData.room_id})</color>");

                    while (_isStreaming && !_cts.IsCancellationRequested && await _roomStream.ResponseStream.MoveNext(_cts.Token))
                    {
                        var response = _roomStream.ResponseStream.Current;
                        Debug.Log($"<color=yellow>[StreamRoom] サーバーから更新がプッシュされました！UIを再描画します。</color>");
                        _pendingRoomUpdate = true;
                    }

                    if (_isStreaming && !_cts.IsCancellationRequested)
                    {
                        Debug.Log("<color=red>[StreamRoom] サーバーからストリームが切断されました。再接続します...</color>");
                        await Task.Delay(1000, _cts.Token);
                    }
                }
                catch (RpcException e)
                {
                    if (_isStreaming && !_cts.IsCancellationRequested)
                    {
                        Debug.LogError($"StreamRoom Error: {e.Status.Detail}");
                        await Task.Delay(1000, _cts.Token);
                    }
                }
                catch (System.OperationCanceledException)
                {
                    // 画面切り替え等による正常な切断
                    break;
                }
                catch (System.Exception ex)
                {
                    if (_isStreaming && !_cts.IsCancellationRequested)
                    {
                        Debug.LogError(ex);
                        await Task.Delay(1000, _cts.Token);
                    }
                }
            }
        }, _cts.Token);
    }

    void Awake()
    {
        renameRoomUI.SetActive(false);
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

        // 部屋名テキストと編集ボタンを自動探索
        var roomNameObj = GameObject.Find("RoomName") ?? GameObject.Find("RoomNameText") ?? GameObject.Find("TitleText");
        if (roomNameObj != null) roomNameText = roomNameObj.GetComponent<TextMeshProUGUI>();

        editButton = GameObject.Find("EditButton") ?? GameObject.Find("RoomEditButton") ?? GameObject.Find("SettingButton");
    }
    
    public async void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                SEManager.instance?.PlayBackSE();
                await gameConnector.LeaveRoom(roomData.room_id, playerData.user_id);
                sceneData.next_scene_number = 3;
                break;
            case "StartBattle":
                SEManager.instance?.PlayToNextSE();
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
                        // 1Pと2Pのユーザーを特定してGameDataを作成する
                        // joiner_list は別メソッドのローカル変数のため roomData.usersData を使用
                        string p1Id = "";
                        string p2Id = "";
                        var roomList = await gameConnector.ListRoom(roomData.room_id);
                        if (roomList != null)
                        {
                            foreach (var r in roomList)
                            {
                                if (r.State == 1) p1Id = r.UserId;
                                if (r.State == 2) p2Id = r.UserId;
                            }
                        }
                        if (!string.IsNullOrEmpty(p1Id) && !string.IsNullOrEmpty(p2Id))
                        {
                            await gameConnector.CreateGameData((uint)roomData.room_id, p1Id, p2Id);
                        }

                        // サーバー上でホストもready状態にしておく
                        await gameConnector.UpdateRoomState(roomData.room_id, playerData.user_id, roomData.usersData[roomData.room_my_number].user_state, true);
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
                SEManager.instance?.PlaySelectSE();
                UpDateRoom();
                break;
            case "Spectator":
                SEManager.instance?.PlaySelectSE();
                await gameConnector.UpdateRoomState(roomData.room_id, playerData.user_id, 0, false);
                UpDateRoom();
                break;
            case "RenameRoom":
                if(roomData.usersData[roomData.room_my_number].is_host)
                {
                renameRoomUI.SetActive(true);
                }
                break;
            case "AplyRenameRoom":
                roomData.room_name = renameRoomText.text;
                renameRoomUI.SetActive(false);
                await gameConnector.UpdateRoomName(roomData.room_id, roomData.room_name, playerData.user_id, false);
                UpDateRoom();
                break;
            case "RenameRoomBack":
                renameRoomUI.SetActive(false);
                break;
            case "changeStatus":
                {
                    int myIdx = roomData.room_my_number;
                    int currentState = roomData.usersData[myIdx].user_state;
                    int newState = 0;

                    if (currentState == 0) // 現在観戦者なら1Pか2Pへの変更を試みる
                    {
                        bool p1Occupied = false;
                        bool p2Occupied = false;
                        for (int i = 0; i < roomData.usersData.Length; i++)
                        {
                            if (roomData.usersData[i].user_state == 1) p1Occupied = true;
                            if (roomData.usersData[i].user_state == 2) p2Occupied = true;
                        }

                        if (!p1Occupied) newState = 1;
                        else if (!p2Occupied) newState = 2;
                        else newState = 0; // 空きがなければ観戦者のまま
                    }
                    else // 現在プレイヤーなら観戦者へ
                    {
                        newState = 0;
                    }

                    await gameConnector.UpdateRoomState(roomData.room_id, playerData.user_id, newState, false);
                    UpDateRoom();
                }
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
        
        if (this == null) return; // シーン移動で破棄されていた場合の安全策
        if (joiner_list == null || rooms == null || joiner_list.Count == 0) return;

        var owner = new Game.Network.UserResponse();
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].RoomId == roomData.room_id)
            {
                owner = await gameConnector.GetUser(rooms[i].OwnerId);
                // Debug.Log($"owner: {owner}");
                roomData.room_name = rooms[i].RoomName;
            }
        }
        for (int i = 0; i < joiner_list.Count; i++)
        {
            var user = await gameConnector.GetUser(joiner_list[i].UserId);
            
            // 通信待ちの間にシーン移動などでオブジェクトが破棄されていた場合は即時中断する
            if (this == null) return;

            if (user.Id == playerData.user_id) roomData.room_my_number = i;
 
            roomData.usersData[i].user_name = user.Name;
            roomData.usersData[i].user_rate = user.Rate;
            roomData.usersData[i].is_host = (owner.Id == user.Id) ? true : false;
            roomData.usersData[i].user_state = joiner_list[i].State;
            roomData.usersData[i].is_ready = joiner_list[i].IsReady;

            // UIテキストを即座に反映
            if (i < userName.Length) userName[i].text = user.Name;
            if (i < userRate.Length) userRate[i].text = "レート：" + user.Rate;
        }

        // 部屋名を反映
        if (roomNameText != null) roomNameText.text = roomData.room_name;

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

            // ホストのみ編集ボタンを表示
            if (editButton != null) editButton.SetActive(amIHost);
        }

        // 全てのステータス更新後にゲーム中か判定してゲスト・ホスト全員を遷移
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].RoomId == roomData.room_id && rooms[i].IsGaming)
            {
                // 自分のstate（1P,2P,観戦者）に応じてisPlayerを設定してから遷移
                int myState = roomData.usersData[roomData.room_my_number].user_state;
                sceneData.next_scene_number = 10;
                return;
            }
        }
    }
}
