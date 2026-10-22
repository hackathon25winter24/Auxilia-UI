using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public class MatchingUIManager : MonoBehaviour
{
    public UserData userData;
    public MatchingData matchingData;
    public RoomData roomData;

    public GameObject roomButton;
    public Transform contentParent;
    public GameObject playerName;
    public Transform contentParentJoinner;
    public GameObject kensakuButton;
    public TMP_InputField kensakuInput;
    public bool is_roading;
    public GameObject nowLoadingText;

    private bool enteringRoom;
    private int previewGeneration;
    public string kensaku_room_name;

    async void Awake()
    {
        nowLoadingText.SetActive(false);
        kensakuButton.SetActive(false);
        is_roading = true;
        nowLoadingText.SetActive(true);
        await UpDateRoomInformation();
        if (this != null) nowLoadingText.SetActive(false);
        is_roading = false;
    }

    void Start()
    {
        // 追加：InputFieldに文字が入るたびにUpdateSearchが走るようにする
        if (kensakuInput != null)
        {
            kensakuInput.onValueChanged.AddListener(delegate { UpdateSearch(); });
        }
    }

    public async void OnButtonClick(string buttonName)
    {
        if (enteringRoom) return;
        switch (buttonName)
        {
            case "Back":
                SEManager.instance?.PlayBackSE();
                SceneChangeManager.MoveScene(1);
                break;
            case "newmake":
                SEManager.instance?.PlayToNextSE();
                //ここに新しく部屋をつくってそこに入る関数を書いてください
                OnClick_CreateRoomMatch();
                break;
            case "ReRoad":
                SEManager.instance?.PlaySelectSE();
                if (is_roading)break;
                is_roading = true;
                nowLoadingText.SetActive(true);
                await UpDateRoomInformation();
                if (this != null) nowLoadingText.SetActive(false);
                is_roading = false;
                break;
            case "kensaku":
                SEManager.instance?.PlaySelectSE();
                kensakuButton.SetActive(true);
                break;
            case "kensakuEnter":
                SEManager.instance?.PlaySelectSE();
                kensaku_room_name = kensakuInput.text;
    
                // 【修正】現在の入力内容で即座に検索を実行する
                UpdateSearch();
                kensakuButton.SetActive(false);
                break;
            case "Backfromkensaku":
                SEManager.instance?.PlayBackSE();
                kensakuButton.SetActive(false);
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    public void CreateRoomButtons(int roomCount)
    {
        Debug.Log("CreateRoomButtons起動");
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < roomCount; i++)
        {
            GameObject newButton = Instantiate(roomButton, contentParent);
            Debug.Log("roomButtonが生成された");

            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = matchingData.rooms[i].room_name;

            ShowRoom entity = newButton.GetComponent<ShowRoom>();

        if (entity != null)
        {
            entity.SetRoomData(matchingData.rooms[i].room_name,
            matchingData.rooms[i].owner_name,
            matchingData.rooms[i].room_is_gamestarted,
            matchingData.rooms[i].num_room_joiner);
        }

            int roomIndex = i; 
            newButton.GetComponent<Button>().onClick.AddListener(() => OnRoomSelected(roomIndex));
        }
    }

    public async void CreateJoinnerNames(int roomNumber)
    {
        int generation = ++previewGeneration;

        foreach (Transform child in contentParentJoinner)
        {
            Destroy(child.gameObject);
        }

        // 部屋をクリックした時にも部屋情報を取得（ローカルに保存しておいて読み込むでも良いが、matchingDataが多くの情報を保管しすぎても見ずらいかなと思ったり。なるべくDB側とデータの構造を合わせた方が管理しやすいと思うので。）
        var joinners = await NetworkManager.Instance.Matching.ListRoom(matchingData.rooms[roomNumber].room_id);
        if (joinners == null || this == null || generation != previewGeneration) return;
        for (int i = 0; i < joinners.Count; i++)
        {
            GameObject newButton = Instantiate(playerName, contentParentJoinner);

            ShowJoinner entity = newButton.GetComponent<ShowJoinner>();

        if (entity != null)
        {
            var joinner_info = await NetworkManager.Instance.Auth.GetUser(joinners[i].UserId);// ユーザーの名前とレート情報を引き出すため
            if (this == null || generation != previewGeneration) return;
            string joinner_name = joinner_info?.Name ?? joinners[i].UserId;
            int joinner_rate = joinner_info?.Rate ?? 0;
            entity.SetJoinnerData(joinner_name, joinner_rate, joinners[i].State);
        }

        }
    }

    async void OnRoomSelected(int index)
    {
        if (enteringRoom || is_roading) return;
        SEManager.instance?.PlayToNextSE();
        if (matchingData.selected_room_id == matchingData.rooms[index].room_id)
        {
            Debug.Log($"部屋 {index + 1} に入室します 部屋ID: {matchingData.rooms[index].room_id}, ユーザーID: {userData.user_id}");
            //ここに部屋に入る関数を書いてください
            if (enteringRoom) return;
            enteringRoom = true;
            try
            {
                var useCase = NetworkManager.Instance.ConfigureRoom(roomData);
                await useCase.Join(checked((uint)matchingData.rooms[index].room_id));
                if (this != null) SceneChangeManager.MoveScene(9);
            }
            catch (System.Exception e) { Debug.LogError($"入室に失敗しました: {e.Message}"); }
            finally { enteringRoom = false; }
        }else
        {
            for (int i = 0; i < matchingData.num_room; i++)
            {
            matchingData.selected_room_id = matchingData.rooms[index].room_id;
            }
            CreateJoinnerNames(index);
        }
    }

    //部屋の情報を更新したいときはこのメソッドをたたいてください
    public async Task UpDateRoomInformation()
    {
        try
        {
            Debug.Log("UpdateRoomInformation実行中");
            //ここに部屋の数を取得する関数を書いてください
            // List<RoomMatch> room_list に全部屋の情報が入ってます
            var room_list = await NetworkManager.Instance.Matching.GetAllRoomMatch();
            //データはmatchingData.num_roomに格納してください
            if (room_list == null || this == null) return;
            matchingData.num_room = room_list.Count;
            SetupRooms(matchingData.num_room);
            matchingData.selected_room_id = -1;// どの部屋も選んでいない時、-1にします。
            for (int i = 0; i < room_list.Count; i++)
            {
                int room_id = room_list[i].RoomId;
                var joinners = await NetworkManager.Instance.Matching.ListRoom(room_id);// 各部屋の参加者数取得のため
                string owner_id = room_list[i].OwnerId;
                var owner = await NetworkManager.Instance.Auth.GetUser(owner_id);// 各部屋のオーナー情報を取得

                if (this == null) return;
                // matchingDataをDBの最新状態に更新
                matchingData.rooms[i].num_room_joiner = joinners?.Count ?? 0;
                matchingData.rooms[i].room_id = room_id;
                matchingData.rooms[i].room_name = room_list[i].RoomName;
                matchingData.rooms[i].owner_id = owner_id;
                matchingData.rooms[i].owner_name = (owner != null) ? owner.Name : owner_id;
                matchingData.rooms[i].room_is_gamestarted = room_list[i].IsGaming;
            }

            if (this != null) CreateRoomButtons(matchingData.num_room);
        }
        catch (System.Exception e) { Debug.LogError($"部屋一覧の取得に失敗しました: {e.Message}"); }
        finally
        {
            is_roading = false;
            if (this != null) nowLoadingText.SetActive(false);
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
        string room_name = userData.user_name + "の部屋"; // ここを書き換えれば最初の部屋名が変わります
        if (string.IsNullOrEmpty(room_name) || string.IsNullOrEmpty(userData.user_id))
        {
            Debug.Log("部屋名が入力されていないかユーザーIDが登録されていません");
            return;
        }
        if (enteringRoom) return;
        enteringRoom = true;
        try
        {
            var useCase = NetworkManager.Instance.ConfigureRoom(roomData);
            await useCase.Create(room_name);
            if (this != null) SceneChangeManager.MoveScene(9);
        }
        catch (System.Exception e) { Debug.LogError($"部屋の作成に失敗しました: {e.Message}"); }
        finally { enteringRoom = false; }
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
                matchingData.rooms[index].owner_name,
                matchingData.rooms[index].room_is_gamestarted,
                matchingData.rooms[index].num_room_joiner);
        }

        // ボタンクリック時のイベント登録（元のindexを保持）
        newButton.GetComponent<Button>().onClick.AddListener(() => OnRoomSelected(index));
    }
}
}
