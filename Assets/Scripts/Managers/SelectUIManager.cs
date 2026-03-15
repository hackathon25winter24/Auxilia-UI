using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class SelectUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public CharacterData characterData;
    public BattleDataforOmline battleDataforOnline;

    public TextMeshProUGUI party_move_cost;
    public TextMeshProUGUI FormTime;
    public TextMeshProUGUI start_game_text;
    public TextMeshProUGUI timertext;
    public RectTransform player_ui;

    public float maxTime = 100f; 
    private float currentTime;
    private bool isTimerRunning = false;

    void Awake()
    {
        start_game_text.gameObject.SetActive(false);
        battleDataforOnline.all_move_cost
        = characterData.characters[battleDataforOnline.selected_character[0]].default_move_cost
        + characterData.characters[battleDataforOnline.selected_character[1]].default_move_cost
        + characterData.characters[battleDataforOnline.selected_character[2]].default_move_cost;
        party_move_cost.text = "cost : " + battleDataforOnline.all_move_cost;
        TimerStart();
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

    void Update()
    {
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                // 前のフレームからの経過時間を引く
                currentTime -= Time.deltaTime;
                timertext.text = Mathf.CeilToInt(currentTime).ToString();
            }
            else
            {
                Debug.Log("タイムアップ！");
                currentTime = 0;
                isTimerRunning = false;
            }
        }
    }

    void TimerStart()
    {
        currentTime = maxTime;
        isTimerRunning = true;
    }
}
