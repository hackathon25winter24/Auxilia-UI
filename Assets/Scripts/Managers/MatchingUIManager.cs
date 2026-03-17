using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MatchingUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public MatchingData matchingData;

    public GameObject roomButton;
    public Transform contentParent;
    public GameObject playerName;
    public Transform contentParentJoinner;
    public GameObject kensakuButton;
    public TMP_InputField kensakuInput;

    public string kensaku_room_name;

    void Awake()
    {
        kensakuButton.SetActive(false);
        UpDateRoomInformation();
    }

    void Start()
    {
        CreateRoomButtons(matchingData.num_room);
    }

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                sceneData.next_scene_number = 1;
                break;
            case "newmake":
                //ここに新しく部屋をつくってそこに入る関数を書いてください
                sceneData.next_scene_number = 9;
                break;
            case "ReRoad":
                UpDateRoomInformation();
                break;
            case "kensaku":
                kensakuButton.SetActive(true);
                break;
            case "kensakuEnter":
                kensaku_room_name = kensakuInput.text;
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

    void OnRoomSelected(int index)
    {
        if (matchingData.rooms[index].room_is_selected)
        {
            Debug.Log($"部屋 {index + 1} に入室します");
            //ここに部屋に入る関数を書いてください
            sceneData.next_scene_number = 9;
        }else
        {
            for (int i = 0; i < matchingData.num_room; i++)
            {
            matchingData.rooms[i].room_is_selected = false;
            }
            matchingData.rooms[index].room_is_selected = true;
            CreateJoinnerNames(matchingData.rooms[index].num_room_joiner);
        }
    }

    //部屋の情報を更新したいときはこのメソッドをたたいてください
    void UpDateRoomInformation()
    {
        //ここに部屋の数を取得する関数を書いてください
        //データはmatchingData.num_roomに格納してください
        SetupRooms(matchingData.num_room);
        //ここに部屋の情報を取得する関数を書いてください
        for (int i = 0; i < matchingData.num_room; i++)
        {
        SetupJoinners(matchingData.rooms[i].num_room_joiner);
        }
        //ここに部屋の参加者の情報を取得する関数を書いてください
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
}
