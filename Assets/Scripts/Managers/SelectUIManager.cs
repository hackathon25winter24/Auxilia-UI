using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

public class SelectUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public CharacterData characterData;
    public BattleDataforOmline battleDataforOnline;
    public RoomData roomData;

    public TextMeshProUGUI party_move_cost;
    public TextMeshProUGUI start_game_text;
    public TextMeshProUGUI timertext;

    public GameObject playerUI;
    public GameObject SpectatorUI;
    public Image[] SelecuUI;
    public GameObject characterTub;
    public GameObject roomButtonPrefab; 
    public Transform contentParent;    
    public Image characterUI;
    public GameObject ready; 
    public GameObject ready2; 
    public TextMeshProUGUI costText;
    public TextMeshProUGUI costText2;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI nameText2;
    public TextMeshProUGUI timertext2;

    public GameConnector gameConnector;
    
    public int selectedUI;
    public int selectedCharacterId;

    public float maxTime = 100f; 
    private float currentTime;
    private bool isTimerRunning = false;

    Game.Network.UserResponse p1 = new Game.Network.UserResponse();
    Game.Network.UserResponse p2 = new Game.Network.UserResponse();

    async void Awake()
    {
        gameConnector = FindFirstObjectByType<GameConnector>().GetComponent<GameConnector>();
        if (battleDataforOnline.isPlayer)
        {
            playerUI.SetActive(true);
            SpectatorUI.SetActive(false);
            battleDataforOnline.selected_character[0] = playerData.character_formation_one;
            battleDataforOnline.selected_character[1] = playerData.character_formation_two;
            battleDataforOnline.selected_character[2] = playerData.character_formation_three;
            characterTub.SetActive(false);
            start_game_text.gameObject.SetActive(false);
            UpDateCharacterUI();
        }else
        {
            //ここに試合をする人の名前を受け取る関数を書いてください
            //データはそれぞれbattleDataforOnlineのpalyer1_nameとplayer2_nameに格納してください
            var battle_player = await gameConnector.GetBattlePlayer(roomData.room_id);
            if (battle_player[0] == null)
            {
                Debug.Log("1Pがいません");
            }
            else
            {
                p1 = await gameConnector.GetUser(battle_player[0].UserId);
                battleDataforOnline.palyer1_name = p1.Name;
            }
            if (battle_player[1] == null)
            {
                Debug.Log("2Pがいません");
            }
            else
            {
                p2 = await gameConnector.GetUser(battle_player[1].UserId);
                battleDataforOnline.player2_name = p2.Name;
            }        
            playerUI.SetActive(false);
            SpectatorUI.SetActive(true);
            ready.SetActive(false); 
            ready2.SetActive(false);
            nameText.text = battleDataforOnline.palyer1_name;
            nameText2.text = battleDataforOnline.player2_name;
        }
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

    async void Update()
    {
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                // 前のフレームからの経過時間を引く
                currentTime -= Time.deltaTime;
                timertext.text = Mathf.CeilToInt(currentTime).ToString();
                timertext2.text = Mathf.CeilToInt(currentTime).ToString();
            }
            else
            {
                Debug.Log("タイムアップ！");
                currentTime = 0;
                isTimerRunning = false;
            }
        }

        costText.text = "cost：" + battleDataforOnline.palyer1_cost;
        costText2.text = "cost:" + battleDataforOnline.palyer2_cost;
        if (battleDataforOnline.isPlayer)
        {
            SendDatas();
            var opponent_characters = GetOpponentDatas();
            Debug.Log($"相手のキャラID: {opponent_characters}");
        }else
        {
            // 毎フレーム動かすことができなかったので大体1秒に1回動かしてます
            float time = Mathf.CeilToInt(currentTime);
            float predicted_time = Mathf.CeilToInt(currentTime-Time.deltaTime);
            if (time != predicted_time)
            {
                var all_game_data = await GetDatas();
                // 説明できないくらい大量のデータが入ってるので必要なものだけを使ってください。以下取得例
                // all_game_data.BaseHp1,    all_game_data.BaseHp2,   all_game_data.Characters, 
                // all_game_data.FinishedAt, all_game_data.Id,        all_game_data.Is1PTurn, 
                // all_game_data.IsFinished, all_game_data.Player1Id, all_game_data.Player2Id, 
                // all_game_data.RoomId,     all_game_data.Turn,      all_game_data.TurnStartAt, 
                // all_game_data.WinnerPlayerId
                Debug.Log($"1P拠点のHP: {all_game_data.BaseHp1}, 対戦キャラリスト: {all_game_data.Characters}, 1Pのターンか: {all_game_data.Is1PTurn}");
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

    public async void SendDatas()
    {
        //ここに自分の編成とコストを送る関数を書いてください

        // 自分の編成は1P2Pに関係なくBattleDataforOmlineの0, 1, 2に入っている想定で書いてます。
        int chara1 = battleDataforOnline.selected_character[0];
        int chara2 = battleDataforOnline.selected_character[1];
        int chara3 = battleDataforOnline.selected_character[2];
        int[] charas = {chara1, chara2, chara3};
        bool is1p = false;

        var room = await gameConnector.ListRoom(roomData.room_id);
        for (int i = 0; i < room.Count; i++)
        {
            if (room[i].UserId == playerData.user_id && room[i].State == 1)
            {
                is1p = true;
            }
        }
        await gameConnector.RegisterCharacters(roomData.room_id, is1p, charas);
    }

    public async Task<List<int>> GetOpponentDatas()
    {
        //ここに相手の編成とコストを受け取る関数を書いてください
        var data = await gameConnector.GetGameData(roomData.room_id);
        var room = await gameConnector.ListRoom(roomData.room_id);
        bool is1p = false;
        var opponent_characters = new List<int>(3);
        for (int i = 0; i < room.Count; i++)
        {
            if (room[i].UserId == playerData.user_id && room[i].State == 1)
            {
                is1p = true;
            }
        }
        for (int i = 0; i < data.Characters.Count; i++)
        {
            if(data.Characters[i].Is1P != is1p)// 相手のキャラを抜き出す
            {
                opponent_characters.Add((int)data.Characters[i].CharacterId);
            }
        }
        // 相手の編成のキャラIDを返せばコストはこっちで計算できるので、IDだけ返します
        return opponent_characters;
    }

    public async Task<Game.Network.GameDataResponse> GetDatas()
    {
        //ここに試合中の全体の編成とコストを受け取る関数を書いてください
        var data = await gameConnector.GetGameData(roomData.room_id);
        return data;
    }
}
