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
                sceneData.next_scene_number = 9;
                break;
            case "join":
                sceneData.next_scene_number = 9;
                break;
            case "koushin":
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

        matchingData.room_is_selected = new bool[roomCount];

        for (int i = 0; i < roomCount; i++)
        {
            GameObject newButton = Instantiate(roomButton, contentParent);

            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"部屋 {i + 1}";

            int roomIndex = i; 
            matchingData.room_is_selected[i] = false;
            newButton.GetComponent<Button>().onClick.AddListener(() => OnRoomSelected(roomIndex));
        }
    }

    public void CreateJoinnerNames(int joinnerCount)
    {
        foreach (Transform child in contentParentJoinner)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < joinnerCount; i++)
        {
            GameObject newButton = Instantiate(playerName, contentParentJoinner);

            ShowJoinner entity = newButton.GetComponent<ShowJoinner>();

        if (entity != null)
        {
            entity.SetRoomData(matchingData.joinners[i].name, matchingData.joinners[i].rate, matchingData.joinners[i].state);
        }

        }
    }

    void OnRoomSelected(int index)
    {
        if (matchingData.room_is_selected[index])
        {
            Debug.Log($"部屋 {index + 1} に入室します");
        }else
        {
            for (int i = 0; i < matchingData.num_room; i++)
            {
            matchingData.room_is_selected[i] = false;
            }
            matchingData.room_is_selected[index] = true;
            CreateJoinnerNames(matchingData.num_room_joiner);
        }
    }
}
