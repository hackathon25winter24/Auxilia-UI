using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Grpc.Core;
using Room;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class RoomUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public UserData userData;
    public RoomData roomData;

    public Image[] joinnersUI;
    public Sprite[] joinnersUIImage;
    public TextMeshProUGUI[] userName;
    public TextMeshProUGUI[] userRate;
    private NetworkManager Net => NetworkManager.Instance;
    private MatchingConnector matchingConnector => Net?.Matching;
    private AuthenticationConnector authenticationConnector => Net?.Auth;
    public GameObject renameRoomUI;
    public TMP_InputField renameRoomText;

    private TextMeshProUGUI startBattleButtonText;
    private TextMeshProUGUI roomNameText; 
    private GameObject editButton;        

    private bool _isStreaming;

    void Start()
    {
        var startButton = GameObject.Find("StartBattleButton");
        if (startButton != null)
        {
            startBattleButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        var rNameObj = GameObject.Find("RoomNameText");
        if (rNameObj != null)
        {
            roomNameText = rNameObj.GetComponent<TextMeshProUGUI>();
        }

        editButton = GameObject.Find("EditButton");

        Debug.Log($"[RoomUIManager] Start called. " +
                $"RoomID: {roomData?.room_id}, " +
                $"UserID: {userData?.user_id}, " +
                $"Connector IsNull: {matchingConnector == null}");
        // 部屋に入った瞬間に双方向ストリームの同期を開始
        if (matchingConnector != null && roomData != null && userData != null)
        {
            _isStreaming = true;
            
            matchingConnector.StartRoomStream(roomData.room_id, userData.user_id, OnRoomStreamUpdated);
        }

        // 初期情報をUIに反映
        UpDateRoomVisuals(null);
    }

    private void OnDestroy()
    {
        _isStreaming = false;
        
        if (matchingConnector != null)
        {
            // 破棄時に安全にストリームを切断
            matchingConnector.StopRoomStream().Forget();
        }
    }

    /// <summary>
    /// ストリームから部屋の最新データを受信した時のハンドラー
    /// </summary>
    private void OnRoomStreamUpdated(ListRoomResponse response)
    {
        Debug.Log($"[RoomUIManager] OnRoomStreamUpdated called. _isStreaming: {_isStreaming}, response.Rooms.Count: {response?.Rooms?.Count ?? 0}");
        if (!_isStreaming || response?.Rooms == null) return;

        Debug.Log($"[RoomUIManager] リアルタイムルーム更新を受信。参加者数: {response.Rooms.Count}");
        
        // 1. 受信データ（gRPCのRepeatedField）を List<Room.Room> に変換してモデルを更新
        var roomsList = new List<Room.Room>(response.Rooms);
        UpdateRoomDataModel(roomsList);

        // 2. 画面のUI表示を最新状態に書き換え
        UpDateRoomVisuals(roomsList);
    }

    private void UpdateRoomDataModel(List<Room.Room> rooms)
    {
        if (roomData == null) return;

        // 1. 全枠（最大4枠など）を一旦初期化 (-1: 空席)
        for (int i = 0; i < roomData.usersData.Length; i++)
        {
            roomData.usersData[i].user_state = -1;
            roomData.usersData[i].is_ready = false;
            roomData.usersData[i].user_id = "";
            roomData.usersData[i].is_host = false;
        }

        int uiIndex = 0;

        // 2. ファーストパス：対戦プレイヤー（1P=State1, 2P=State2）を優先して前方に配置
        foreach (var r in rooms)
        {
            if (r.State == 1 || r.State == 2) 
            {
                if (uiIndex < roomData.usersData.Length)
                {
                    roomData.usersData[uiIndex].user_id = r.UserId;
                    roomData.usersData[uiIndex].user_state = r.State;
                    roomData.usersData[uiIndex].is_ready = r.IsReady;
                    roomData.usersData[uiIndex].is_host = (r.State == 1); // 1Pを厳格にホストとして扱う
                    uiIndex++;
                }
            }
        }

        // 3. セカンドパス：空いた枠に観戦者（State = 0）を順番に詰める
        foreach (var r in rooms)
        {
            if (r.State == 0) 
            {
                if (uiIndex < roomData.usersData.Length)
                {
                    roomData.usersData[uiIndex].user_id = r.UserId;
                    roomData.usersData[uiIndex].user_state = r.State;
                    roomData.usersData[uiIndex].is_ready = false; // 観戦者は準備完了不要
                    roomData.usersData[uiIndex].is_host = false;
                    uiIndex++;
                }
            }
        }

        // 4. 自分のインデックス（roomData上のどこに自分が格納されたか）を確定
        roomData.room_my_index = -1;
        for (int i = 0; i < roomData.usersData.Length; i++)
        {
            if (userData != null && roomData.usersData[i].user_id == userData.user_id)
            {
                roomData.room_my_index = i;
                break;
            }
        }
    }

    /// <summary>
    /// 部屋のビジュアル要素（テキスト・ボタン・アウトライン）を最新データに書き換える
    /// </summary>
    public async void UpDateRoomVisuals(List<Room.Room> rooms)
    {
        if (roomData == null || roomData.usersData == null) return;

        for (int i = 0; i < joinnersUI.Length; i++)
        {
            if (i >= roomData.usersData.Length) break;

            // 枠に誰もいない場合
            if (roomData.usersData[i].user_state == -1)
            {
                joinnersUI[i].sprite = joinnersUIImage[0]; // 空席画像
                userName[i].text = "枠が空いています";
                userRate[i].text = "Rate: ----";
                
                var outline = joinnersUI[i].GetComponent<Outline>();
                if (outline != null) outline.enabled = false;
                continue;
            }

            // 誰かが入っている場合
            joinnersUI[i].sprite = joinnersUIImage[1]; // プレイヤー画像
            
            string roleStr = roomData.usersData[i].user_state switch {
                1 => "[1P] ",
                2 => "[2P] ",
                _ => "[観戦] "
            };
            var userInfo = await authenticationConnector.GetUser(roomData.usersData[i].user_id);
            userName[i].text = (userInfo != null)? userInfo.Name : "???";
            userRate[i].text = "Rate: " + ((userInfo != null)? userInfo.Rate : "???"); 

            // 準備完了状態の枠を光らせる（Outline制御）
            var optOutline = joinnersUI[i].GetComponent<Outline>();
            if (optOutline == null) 
            {
                optOutline = joinnersUI[i].gameObject.AddComponent<Outline>();
                optOutline.effectDistance = new Vector2(3, -3);
            }
            
            // 1Pは常に準備完了扱い、それ以外は is_ready フラグを見る
            if (roomData.usersData[i].user_state == 1 || roomData.usersData[i].is_ready)
            {
                optOutline.enabled = true;
                optOutline.effectColor = Color.green; 
            }
            else
            {
                optOutline.enabled = false;
            }
        }

        if (startBattleButtonText != null && roomData.room_my_index >= 0)
        {
            var myData = roomData.usersData[roomData.room_my_index];
            
            if (myData.user_state == 1) // 自分が1P（ホスト）の場合
            {
                startBattleButtonText.text = "対戦開始";
                if (editButton != null) editButton.SetActive(true);
            }
            else if (myData.user_state == 2) // 自分が2Pの場合
            {
                startBattleButtonText.text = myData.is_ready ? "準備完了解除" : "準備完了";
                if (editButton != null) editButton.SetActive(false);
            }
            else // 自分が観戦者の場合
            {
                startBattleButtonText.text = "観戦中 (ホストの開始待ち)";
                if (editButton != null) editButton.SetActive(false);
            }
        }

        // 試合開始フラグなどの監視・遷移チェック
        if (rooms != null)
        {
            CheckAndTransitionToBattle(rooms);
        }
    }

    private void CheckAndTransitionToBattle(List<Room.Room> rooms)
    {
        // 必要に応じて、サーバーの RoomMatch.IsGaming などをトリガーにしたシーン遷移をここに記述します
    }

    // =================================================================
    // ボタン等から呼ばれるパブリックアクション
    // =================================================================

    /// <summary>
    /// 対戦開始、または準備完了ボタンが押された時の処理
    /// </summary>
    public async void OnClickReadyOrStart()
    {
        if (roomData == null || matchingConnector == null || userData == null) return;

        int myIdx = roomData.room_my_index;
        if (myIdx < 0 || myIdx >= roomData.usersData.Length) return;

        var myData = roomData.usersData[myIdx];

        if (myData.user_state == 1) // 自分が1P（ホスト）なら試合開始
        {
            Debug.Log("[RoomUIManager] ホストとして対戦開始リクエストを送信");
            var res = await matchingConnector.StartMatch(roomData.room_id);
            if (res != null)
            {
                Debug.Log("試合開始リクエスト成功");
            }
        }
        else if (myData.user_state == 2) // 自分が2Pなら準備完了状態のトグル切り替え
        {
            bool nextReadyState = !myData.is_ready;
            Debug.Log($"[RoomUIManager] 2Pとして準備完了状態を {nextReadyState} に更新リクエスト");
            
            // サーバーの UpdateRoomState を叩く (引数: roomId, userId, state, isReady)
            await matchingConnector.UpdateRoomState(roomData.room_id, userData.user_id, myData.user_state, nextReadyState);
        }
        else
        {
            Debug.Log("[RoomUIManager] 観戦者はボタンを操作できません");
        }
    }

    /// <summary>
    /// 部屋を退出するボタンが押された時の処理
    /// </summary>
    public async void OnClickLeaveRoom()
    {
        if (matchingConnector == null || roomData == null || userData == null) return;

        var result = await matchingConnector.LeaveRoom(roomData.room_id, userData.user_id);
        if (result != null)
        {
            _isStreaming = false;
            // 安全にストリームを止めてから画面を戻す
            await matchingConnector.StopRoomStream();
            
            // ロビーシーンなどへの遷移ロジックをここに記述
            Debug.Log("部屋を退出しました。ロビーへ戻ります。");
            sceneData.next_scene_number = 3;
        }
    }
}