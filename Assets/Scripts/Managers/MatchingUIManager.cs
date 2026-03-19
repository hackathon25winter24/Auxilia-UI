using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public class MatchingUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public MatchingData matchingData;
    public RoomData roomData;

    public GameObject roomButton;
    public Transform contentParent;
    public GameObject playerName;
    public Transform contentParentJoinner;
    public GameObject kensakuButton;
    public TMP_InputField kensakuInput;
    public GameConnector gameConnector;
    public string ownerName;
    public string ownerId;
    public bool is_roading;
    public GameObject nowLoadingText;

    public string kensaku_room_name;

    async void Awake()
    {
        nowLoadingText.SetActive(false);
        kensakuButton.SetActive(false);
        gameConnector = FindFirstObjectByType<GameConnector>().GetComponent<GameConnector>();
        is_roading = true;
        nowLoadingText.SetActive(true);
        await UpDateRoomInformation();
        nowLoadingText.SetActive(false);
        is_roading = false;
    }

    void Start()
    {
        ownerId = playerData.user_id;
        ownerName = playerData.username;
        // 追加：InputFieldに文字が入るたびにUpdateSearchが走るようにする
        if (kensakuInput != null)
        {
            kensakuInput.onValueChanged.AddListener(delegate { UpdateSearch(); });
        }
    }

    public async void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                sceneData.next_scene_number = 1;
                break;
            case "newmake":
                //ここに新しく部屋をつくってそこに入る関数を書いてください
                OnClick_CreateRoomMatch();
                break;
            case "ReRoad":
                if (is_roading)break;
                is_roading = true;
                nowLoadingText.SetActive(true);
                await UpDateRoomInformation();
                nowLoadingText.SetActive(false);
                is_roading = false;
                break;
            case "kensaku":
                kensakuButton.SetActive(true);
                break;
            case "kensakuEnter":
                kensaku_room_name = kensakuInput.text;
    
                // 【修正】現在の入力内容で即座に検索を実行する
                UpdateSearch();
                kensakuButton.SetActive(false);
                break;
            case "Backfromkensaku":
                kensakuButton.SetActive(false);
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    public void CreateRoomButtons(int roomCount)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < roomCount; i++)
        {
            GameObject newButton = Instantiate(roomButton, contentParent);

            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = matchingData.rooms[i].room_name;

            ShowRoom entity = newButton.GetComponent<ShowRoom>();

        if (entity != null)
        {
            entity.SetRoomData(matchingData.rooms[i].room_name,
            matchingData.rooms[i].room_host,
            matchingData.rooms[i].room_is_gamestarted,
            matchingData.rooms[i].num_room_joiner);
        }

            int roomIndex = i; 
            matchingData.rooms[i].room_is_selected = false;
            newButton.GetComponent<Button>().onClick.AddListener(() => OnRoomSelected(roomIndex));
        }
    }

    public void CreateJoinnerNames(int roomNumber)
    {
        int num_joinner = matchingData.rooms[roomNumber].num_room_joiner;
        SetupJoinners(num_joinner);

        foreach (Transform child in contentParentJoinner)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < num_joinner; i++)
        {
            GameObject newButton = Instantiate(playerName, contentParentJoinner);

            ShowJoinner entity = newButton.GetComponent<ShowJoinner>();

        if (entity != null)
        {
            entity.SetJoinnerData(matchingData.rooms[roomNumber].joinners[i].name,
            matchingData.rooms[roomNumber].joinners[i].rate,
            matchingData.rooms[roomNumber].joinners[i].state);
        }

        }
    }

    async void OnRoomSelected(int index)
    {
        if (matchingData.rooms[index].room_is_selected)
        {
            Debug.Log($"部屋 {index + 1} に入室します");
            //ここに部屋に入る関数を書いてください
            var response = await gameConnector.JoinRoom(matchingData.rooms[index].room_id, playerData.user_id);
            if (response != null && response.Rooms.Count > 0)
            {
                bool has1p = false, has2p = false;
                foreach (var r in response.Rooms)
                {
                    if (r.UserId != playerData.user_id)
                    {
                        if (r.State == 1) has1p = true;
                        if (r.State == 2) has2p = true;
                    }
                }
                int newState = 0; // default Spectator
                if (!has1p) newState = 1;
                else if (!has2p) newState = 2;
                
                await gameConnector.UpdateRoomState(matchingData.rooms[index].room_id, playerData.user_id, newState, false);

                roomData.room_id = response.Rooms[0].RoomId;
                for (int i = 0; i < matchingData.num_room; i++)
                {
                    if (matchingData.rooms[i].room_id == roomData.room_id)
                    {
                        roomData.room_name = matchingData.rooms[i].room_name;
                    }
                }

                sceneData.next_scene_number = 9;
            }
        }else
        {
            for (int i = 0; i < matchingData.num_room; i++)
            {
            matchingData.rooms[i].room_is_selected = false;
            }
            matchingData.rooms[index].room_is_selected = true;
            CreateJoinnerNames(index);
        }
    }

    //部屋の情報を更新したいときはこのメソッドをたたいてください
    public async Task UpDateRoomInformation()
    {
        //ここに部屋の数を取得する関数を書いてください
        // List<RoomMatch> room_list に全部屋の情報が入ってます
        var room_list = await gameConnector.GetAllRoomMatch();
        //データはmatchingData.num_roomに格納してください
        matchingData.num_room = room_list.Count;
        SetupRooms(matchingData.num_room);
        for (int i = 0; i < room_list.Count; i++)
        {
            matchingData.rooms[i].room_id = room_list[i].RoomId;
            matchingData.rooms[i].room_name = room_list[i].RoomName;
            matchingData.rooms[i].room_host = room_list[i].OwnerId;
            matchingData.rooms[i].room_is_gamestarted = room_list[i].IsGaming;
            var joiner_list = await gameConnector.ListRoom(room_list[i].RoomId);
            matchingData.rooms[i].num_room_joiner = joiner_list.Count;
            // 移動しました
            SetupJoinners(i);
            for (int j = 0; j < joiner_list.Count; j++)
            {
                var user = await gameConnector.GetUser(joiner_list[j].UserId);
                Debug.Log($"{i}番目の部屋{j}番目のusername: {user.Name}");
                matchingData.rooms[i].joinners[j].name = user.Name;
                matchingData.rooms[i].joinners[j].state = joiner_list[j].State;
            }
            //Debug.Log($"部屋ID: {room_list[i].RoomId}, 部屋名: {room_list[i].RoomName}, オーナーID: {room_list[i].OwnerId}, 試合中: {room_list[i].IsGaming}");
        }

        CreateRoomButtons(matchingData.num_room);
    }

    public void SetupJoinners(int roomNumber)
    {
    matchingData.rooms[roomNumber].joinners.Clear();

    for (int i = 0; i < matchingData.rooms[roomNumber].num_room_joiner; i++)
    {
        matchingData.rooms[roomNumber].joinners.Add(new JoinnersData()); 
    }
    }

    public void SetupRooms(int count)
    {
    matchingData.rooms.Clear();

    for (int i = 0; i < count; i++)
    {
        matchingData.rooms.Add(new RoomsData()); 
    }
    }

    public async void OnClick_CreateRoomMatch()
    {
        string room_name = ownerName + "の部屋"; // ここを書き換えれば最初の部屋名が変わります
        if (string.IsNullOrEmpty(room_name) || string.IsNullOrEmpty(ownerId))
        {
            Debug.Log("部屋名が入力されていないかユーザーIDが登録されていません");
            return;
        }
        var response = await gameConnector.CreateRoomMatch(room_name, ownerId, false);
        if (response != null)
        {
            await gameConnector.JoinRoom(response.RoomId, response.OwnerId);
            await gameConnector.UpdateRoomState(response.RoomId, response.OwnerId, 1, false);

            // 新たにRoomDataにIDを追加
            roomData.room_id = response.RoomId;
            roomData.room_name = room_name;

            sceneData.next_scene_number = 9;
        }
    }

    // 1. UpdateSearchメソッドの修正
public void UpdateSearch()
{
    string input = kensakuInput.text;

    // 入力が空なら全ルームを表示
    if (string.IsNullOrEmpty(input))
    {
        CreateRoomButtons(matchingData.num_room);
    }
    else
    {
        // ルーム名にキーワードが含まれる要素の「インデックス（番号）」を抽出
        List<int> filteredIndices = new List<int>();
        for (int i = 0; i < matchingData.rooms.Count; i++)
        {
            // 大文字小文字を区別せずに日本語含め検索
            if (matchingData.rooms[i].room_name.ToLower().Contains(input.ToLower()))
            {
                filteredIndices.Add(i);
            }
        }

        // 絞り込んだ結果でボタンを再生成
        UpdateDisplayFiltered(filteredIndices);
    }
}

// 2. 絞り込み専用の表示更新メソッドを追加
private void UpdateDisplayFiltered(List<int> indices)
{
    // 既存のボタンを削除
    foreach (Transform child in contentParent)
    {
        Destroy(child.gameObject);
    }

    // フィルタリングされたインデックスのみでボタンを作成
    foreach (int index in indices)
    {
        GameObject newButton = Instantiate(roomButton, contentParent);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = matchingData.rooms[index].room_name;

        ShowRoom entity = newButton.GetComponent<ShowRoom>();
        if (entity != null)
        {
            entity.SetRoomData(
                matchingData.rooms[index].room_name,
                matchingData.rooms[index].room_host,
                matchingData.rooms[index].room_is_gamestarted,
                matchingData.rooms[index].num_room_joiner);
        }

        // ボタンクリック時のイベント登録（元のindexを保持）
        newButton.GetComponent<Button>().onClick.AddListener(() => OnRoomSelected(index));
    }
}
}
