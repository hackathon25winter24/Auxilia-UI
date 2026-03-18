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
    public TextMeshProUGUI start_game_text;
    public TextMeshProUGUI timertext;
    public RectTransform player_ui;

    public Image[] SelecuUI;
    public GameObject characterTub;
    public GameObject roomButtonPrefab; 
    public Transform contentParent;    
    public Image characterUI;
    
    public int selectedUI;
    public int selectedCharacterId;

    public float maxTime = 100f; 
    private float currentTime;
    private bool isTimerRunning = false;

    void Awake()
    {
        battleDataforOnline.selected_character[0] = playerData.character_formation_one;
        battleDataforOnline.selected_character[1] = playerData.character_formation_two;
        battleDataforOnline.selected_character[2] = playerData.character_formation_three;
        characterTub.SetActive(false);
        start_game_text.gameObject.SetActive(false);
        UpDateCharacterUI();
        TimerStart();
    }

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Select1":
                selectedUI = 0;
                characterTub.SetActive(true);
                CharacterButtons();
                break;
            case "Select2":
                selectedUI = 1;
                characterTub.SetActive(true);
                CharacterButtons();
                break;
            case "Select3":
                selectedUI = 2;
                characterTub.SetActive(true);
                CharacterButtons();
                break;
            case "Decided":
                sceneData.next_scene_number = 5;
                break;
            case "BackShadow":
                characterTub.SetActive(false);
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    public void CharacterClick(int ButtonNum)
    {
        battleDataforOnline.selected_character[selectedUI] = ButtonNum;
        characterTub.SetActive(false);
        UpDateCharacterUI();
    }

    public void CharacterLongClick(int LongButtonNum)
    {}

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

    void UpDateCharacterUI()
    {
        battleDataforOnline.all_move_cost
        = characterData.characters[battleDataforOnline.selected_character[0]].default_move_cost
        + characterData.characters[battleDataforOnline.selected_character[1]].default_move_cost
        + characterData.characters[battleDataforOnline.selected_character[2]].default_move_cost;
        party_move_cost.text = "cost : " + battleDataforOnline.all_move_cost;
        for (int i = 0; i <= 2; i++)
        {
            SelecuUI[i].sprite = characterData.characters[battleDataforOnline.selected_character[i]].select_image;
        }
    }

    public void CharacterButtons()
    {
        // 既存のリストを一度クリア（二重生成防止）
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < characterData.characters.Length; i++)
        {
            characterUI.sprite = characterData.characters[i].select_image;
            GameObject newButton = Instantiate(roomButtonPrefab, contentParent);

            // ボタンが押された時の処理をコードから登録
            int buttonIndex = i; 

            RoomButtonLongPress longPressScript = newButton.GetComponent<RoomButtonLongPress>();
            if (longPressScript != null)
            {
            longPressScript.myIndex = buttonIndex;
            // 長押しされた時に実行するメソッドを登録
            longPressScript.onLongPressWithIndex.AddListener(CharacterLongClick);
            }

            newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => CharacterClick(buttonIndex));
        }
    }
}
