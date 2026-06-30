using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Grpc.Core;
using Room;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks; // 💡 UniTaskの活用

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
    public AuthenticationConnector authenticationConnector;
    public MatchingConnector matchingConnector;
    public BattleConnector battleConnector;
    public GameObject renameRoomUI;
    public TMP_InputField renameRoomText;

    private TextMeshProUGUI startBattleButtonText;
    private TextMeshProUGUI roomNameText; // 部屋名表示用
    private GameObject editButton;        // ホスト用編集ボタン

    private bool _isStreaming;

    void Start()
    {
        // 💡 ボタンやUIコンポーネントの初期キャッシュ処理
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

        // 💡 部屋に入った瞬間に双方向ストリームの同期を開始
        if (matchingConnector != null && roomData != null && userData != null)
        {
            _isStreaming = true;
            
            // マッチングコネクター側で定義した新しいストリームAPIを叩く
            matchingConnector.StartRoomStream(roomData.room_id, userData.user_id, (response) =>
            {
                // 💡 サーバーから部屋の更新通知が届くたびに、この中身がメインスレッドで安全に実行されます
                if (_isStreaming && response?.Rooms != null)
                {
                    OnRoomStreamUpdated(new List<Room.Room>(response.Rooms));
                }
            });
        }
    }

    private void OnDestroy()
    {
        _isStreaming = false;
        
        // 💡 シーン遷移やコンポーネント消滅時にストリームを確実に切断する
        if (matchingConnector != null)
        {
            _ = matchingConnector.StopRoomStream();
        }
    }

    /// <summary>
    /// ストリームから部屋の最新データを受信した時のハンドラー
    /// </summary>
    private void OnRoomStreamUpdated(List<Room.Room> rooms)
    {
        Debug.Log($"[RoomUIManager] リアルタイムルーム更新を受信。参加者数: {rooms.Count}");
        
        // 1. 受信データを元に、RoomData(ScriptableObject等)の中身を書き換える
        UpdateRoomDataModel(rooms);

        // 2. 書き換えたデータモデルをUIコンポーネントへレンダリング（描画反映）
        UpDateRoom(rooms);
    }

    private void UpdateRoomDataModel(List<Room.Room> rooms)
    {
        if (roomData == null) return;

        // 1. 全枠（UI表示用の4席）を一旦初期化
        for (int i = 0; i < 4; i++)
        {
            roomData.usersData[i].user_state = -1;
            roomData.usersData[i].is_ready = false;
            roomData.usersData[i].user_id = "";
        }

        int uiIndex = 0;

        // 2. ファーストパス：対戦プレイヤー（State = 1 や 2）を優先してUI枠（前方）に配置
        foreach (var r in rooms)
        {
            if (r.State == 1 || r.State == 2) // 1Pまたは2P
            {
                if (uiIndex < 4)
                {
                    roomData.usersData[uiIndex].user_id = r.UserId;
                    roomData.usersData[uiIndex].user_state = r.State;
                    roomData.usersData[uiIndex].is_ready = r.IsReady;
                    roomData.usersData[uiIndex].is_host = (r.State == 1); // 1Pをホスト扱い
                    uiIndex++;
                }
            }
        }

        // 3. セカンドパス：空いた枠に観戦者（State = 0など）を順番に詰める
        foreach (var r in rooms)
        {
            if (r.State != 1 && r.State != 2) // 観戦者など
            {
                if (uiIndex < 4)
                {
                    roomData.usersData[uiIndex].user_id = r.UserId;
                    roomData.usersData[uiIndex].user_state = r.State;
                    roomData.usersData[uiIndex].is_ready = false; // 観戦者は準備完了不要
                    roomData.usersData[uiIndex].is_host = false;
                    uiIndex++;
                }
            }
        }

        // 4. 自分のインデックスを確定させる（何人入っていても自分がどこにいるかを探す）
        roomData.room_my_index = -1;
        for (int i = 0; i < 4; i++)
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
    public void UpDateRoom(List<Room.Room> rooms)
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
            userName[i].text = "ID: " + roomData.usersData[i].user_id;
            userRate[i].text = "Rate: 1500"; // 仮値

            // 準備完了状態の枠を光らせる（Outline制御）
            var optOutline = joinnersUI[i].GetComponent<Outline>();
            if (optOutline == null) 
            {
                optOutline = joinnersUI[i].gameObject.AddComponent<Outline>();
                optOutline.effectDistance = new Vector2(3, -3);
            }
            
            if (roomData.usersData[i].is_ready)
            {
                optOutline.enabled = true;
                optOutline.effectColor = Color.green; // 準備完了はわかりやすく緑などに
            }
            else
            {
                optOutline.enabled = false;
            }
        }

        // 対戦ボタン・テキストまわりの文言制御
        if (startBattleButtonText != null && roomData.room_my_index >= 0 && roomData.room_my_index < roomData.usersData.Length)
        {
            bool amIHost = roomData.usersData[roomData.room_my_index].is_host;
            bool amIReady = roomData.usersData[roomData.room_my_index].is_ready;
            
            if (amIHost)
            {
                startBattleButtonText.text = "対戦開始";
            }
            else
            {
                startBattleButtonText.text = amIReady ? "ホストの開始を待っています..." : "準備完了";
            }

            if (editButton != null) editButton.SetActive(amIHost);
        }

        // 💡 全員の準備状態またはサーバー側フラグを監視して、試合が開始されたか判定して遷移
        // (注: room_match.proto 側の IsGaming 状態の変更をここで検知してシーン遷移させるロジック等へ繋げてください)
        CheckAndTransitionToBattle(rooms);
    }

    private void CheckAndTransitionToBattle(List<Room.Room> rooms)
    {
        // 試合シーンへの移行条件チェックをここに記述します
        // 例: サーバー側から特定のState（ゲーム中）がプッシュされたら、
        // sceneData.battle_online にシーン名を詰めてロード画面に遷移させるなど
    }

    // =================================================================
    // ボタン等から呼ばれる各種パブリックアクション
    // =================================================================

    public async void OnClickReadyOrStart()
    {
        if (roomData == null || matchingConnector == null || userData == null) return;

        int myIdx = roomData.room_my_index;
        if (myIdx < 0 || myIdx >= roomData.usersData.Length) return;

        bool amIHost = roomData.usersData[myIdx].is_host;

        if (amIHost)
        {
            Debug.Log("[RoomUIManager] ホストとして対戦開始リクエストを送信");
            // 例: await matchingConnector.StartMatch(roomData.room_id);
        }
        else
        {
            Debug.Log("[RoomUIManager] ゲストとして準備完了切り替えを送信");
            // 例: await matchingConnector.SetReady(roomData.room_id, userData.user_id);
        }
    }

    public async void OnClickLeaveRoom()
    {
        if (matchingConnector == null || roomData == null || userData == null) return;

        var result = await matchingConnector.LeaveRoom(roomData.room_id, userData.user_id);
        if (result != null)
        {
            // 部屋から正常に抜けたら、ストリームを止めてルーム選択画面へ遷移
            _isStreaming = false;
            await matchingConnector.StopRoomStream();
            
        }
    }
}