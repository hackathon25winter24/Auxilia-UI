using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class SelectUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;

    public TextMeshProUGUI party_move_cost;
    public TextMeshProUGUI FormTime;
    public TextMeshProUGUI start_game_text;
    public RectTransform player_ui;

    void Awake()
    {
        start_game_text.gameObject.SetActive(false);
    }

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Decided":
                sceneData.next_scene_number = 5;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
