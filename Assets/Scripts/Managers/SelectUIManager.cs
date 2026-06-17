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
    public UserData userData;
    public CharacterData characterData;
    public BattleDataForOnline battleDataforOnline;
    public RoomData roomData;

    public TextMeshProUGUI party_move_cost;
    public TextMeshProUGUI start_game_text;
    public TextMeshProUGUI timertext;

    public GameObject SelectedTub;
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

    public AuthenticationConnector authenticationConnector;
    public MatchingConnector matchingConnector;
    public BattleConnector battleConnector;
    
    public int selectedCharacterId;

    // 「決定」ボタンを押した後、相手の準備完了を待っている状態かどうか
    private bool _waitingForOpponent = false;
    // 「決定」ボタン上のテキスト（相手待機中メッセージの表示に使う）
    public TextMeshProUGUI decidedButtonText;

    public float maxTime = 100f; 
    private float currentTime;
    private bool isTimerRunning = false;

    private int selectedCharacter1;// デッキ1枠目の選択キャラ保存用変数。以下同様
    private int selectedCharacter2;
    private int selectedCharacter3;

    private int selectedUI;// 1~3のどの枠が押されたか

    Game.Network.UserResponse p1 = new Game.Network.UserResponse();
    Game.Network.UserResponse p2 = new Game.Network.UserResponse();

    async void Awake()
    {
        matchingConnector = FindFirstObjectByType<MatchingConnector>();
        authenticationConnector = FindFirstObjectByType<AuthenticationConnector>();
        battleConnector = FindFirstObjectByType<BattleConnector>();

        SelectedTub.SetActive(false);
        // 1P/2Pの情報を取得
        await GetPlayerInfo();

        // 自分の状態を確認してプレイヤーかどうか判定
        var roomList = await matchingConnector.ListRoom(roomData.room_id);
        int myState = 0;
        if (roomList != null)
        {
            foreach (var r in roomList)
            {
                if (r.UserId == userData.user_id)
                {
                    myState = r.State;
                    break;
                }
            }
        }

        if (myState == 1 || myState == 2)
        {
            playerUI.SetActive(true);
            SpectatorUI.SetActive(false);
            selectedCharacter1 = userData.deck1;
            selectedCharacter2 = userData.deck2;
            selectedCharacter3 = userData.deck3;
            characterTub.SetActive(false);
            start_game_text.gameObject.SetActive(false);
            UpDateCharacterUI();
            
            // 決定ボタンの初期化
            if (decidedButtonText != null) decidedButtonText.text = "決定";

            // プレイヤーも準備完了監視を開始（自分が決定した後のため。または自動遷移のため）
            StartCoroutine(WaitForBothPlayersReady());
        }
        else
        {
            playerUI.SetActive(false);
            SpectatorUI.SetActive(true);
            ready.SetActive(false); 
            ready2.SetActive(false);
            nameText.text = p1.Name;
            nameText2.text = p2.Name;
            
            // 観戦者もバトル開始を待つ
            StartCoroutine(WaitForBothPlayersReady());
        }
        TimerStart();
    }

    private async Task GetPlayerInfo()
    {
        var battle_player = await matchingConnector.GetBattlePlayer(roomData.room_id);
        if (battle_player != null && battle_player.Count >= 2)
        {
            if (battle_player[0] != null)
            {
                p1 = await authenticationConnector.GetUser(battle_player[0].UserId);
            }
            if (battle_player[1] != null)
            {
                p2 = await authenticationConnector.GetUser(battle_player[1].UserId);
            }
        }
    }

    public async void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Select1":
                selectedUI = 1;
                characterTub.SetActive(true);
                CharacterButtons();
                break;
            case "Select2":
                selectedUI = 2;
                characterTub.SetActive(true);
                CharacterButtons();
                break;
            case "Select3":
                selectedUI = 3;
                characterTub.SetActive(true);
                CharacterButtons();
                break;
            case "Decided":
                if (!_waitingForOpponent)
                {
                    await SendDatas();
                    SelectedTub.SetActive(true);
                    _waitingForOpponent = true;
                    if (decidedButtonText != null)
                        decidedButtonText.text = "相手の準備を待っています...";
                    // 両者が準備できるまでポーリングで待機し、完了次第バトルシーンへ自動遷移
                    StartCoroutine(WaitForBothPlayersReady());
                }
                break;
            case "BackShadow":
                characterTub.SetActive(false);
                break;
            case "Random":
            RandomizeFormation();
            break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    public void CharacterClick(int ButtonNum)
{
    // 現在の枠（selectedUI）に元々いたキャラを一時保存
    int previousChar = 0;
    if (selectedUI == 1)
    {
        previousChar = selectedCharacter1;
    }
    else if (selectedUI == 2)
    {
        previousChar = selectedCharacter2;
    }
    else if (selectedUI == 3)
    {
        previousChar = selectedCharacter3;
    }

    // 重複チェック
    if (selectedCharacter1 == ButtonNum)
    {
        // スロット0に「元いたキャラ」を移動させる
        selectedCharacter1 = previousChar;
    }
    else if (selectedCharacter2 == ButtonNum)
    {
        // スロット1に「元いたキャラ」を移動させる
        selectedCharacter2 = previousChar;
    }
    else if (selectedCharacter3 == ButtonNum)
    {
        // スロット2に「元いたキャラ」を移動させる
        selectedCharacter3 = previousChar;
    }

    // 最後に、今選んだ枠に新しいキャラを入れる
    if (selectedUI == 1)
    {
        selectedCharacter1 = ButtonNum;
    }
    else if (selectedUI == 2)
    {
        selectedCharacter2 = ButtonNum;
    }
    else if (selectedUI == 3)
    {
        selectedCharacter3 = ButtonNum;
    }

    characterTub.SetActive(false);
    UpDateCharacterUI();
}
    public void CharacterLongClick(int LongButtonNum)
    {}

    private float _pollTimer = 0f;
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

        // 定期的にサーバーから状態を取得してUIに反映
        _pollTimer -= Time.deltaTime;
        if (_pollTimer <= 0)
        {
            _pollTimer = 1.0f; // 1秒おきに更新
            await SyncRoomStatus();
        }
        // これ何？
        // costText.text = "cost:" + battleDataforOnline.palyer1_cost;
        // costText2.text = "cost:" + battleDataforOnline.palyer2_cost;
    }

    public void RandomizeFormation()
    {
    // 1. 全キャラクターのインデックス(ID)をリストにコピー
    List<int> availableIndices = new List<int>();
    for (int i = 0; i < characterData.characters.Length; i++)
    {
        availableIndices.Add(i);
    }

    // 2. 3つの枠に対して抽選
    for (int i = 0; i < 3; i++)
    {
        if (availableIndices.Count > 0)
        {
            // リストからランダムに1つ選ぶ
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            int selectedId = availableIndices[randomIndex];

            // 自分の編成データに代入
            if (i == 0) selectedCharacter1 = selectedId;
            if (i == 1) selectedCharacter2 = selectedId;
            if (i == 2) selectedCharacter3 = selectedId;
            // 選んだIDをリストから削除（これで二度と選ばれない）
            availableIndices.RemoveAt(randomIndex);
        }
    }

    // 3. UIを更新して合計コストなどを再計算
    UpDateCharacterUI();
    }

    private async Task SyncRoomStatus()
    {
        var data = await GetDatas();
        if (data == null) return;

        int p1Count = 0;
        int p2Count = 0;
        int p1Cost = 0;
        int p2Cost = 0;

        foreach (var c in data.Characters)
        {
            int cost = 0;
            if (c.CharacterId < characterData.characters.Length)
            {
                cost = characterData.characters[c.CharacterId].default_move_cost;
            }

            if (c.Is1P)
            {
                p1Count++;
                p1Cost += cost;
            }
            else
            {
                p2Count++;
                p2Cost += cost;
            }
        }

        // おそらくやりたいことはこういうことだと思う。観戦者用の双方コスト表示テキストと予想
        // ↑Updateのテキスト表示処理のこと
        costText.text = "cost:" + p1Cost;
        costText2.text = "cost:" + p2Cost;

        // 準備完了インジケータ（3体登録されていたら表示）
        if (ready != null) ready.SetActive(p1Count >= 3);
        if (ready2 != null) ready2.SetActive(p2Count >= 3);

        // キャラ選択中にプレイヤーの名前が変わることはないと思うのだけど、この処理は何？
        /*
        if (!battleDataforOnline.isPlayer)
        {
            // 観戦者用：名前を更新
            nameText.text = battleDataforOnline.player1_name;
            nameText2.text = battleDataforOnline.player2_name;
        }
        */
    }

    void TimerStart()
    {
        currentTime = maxTime;
        isTimerRunning = true;
    }

    private IEnumerator WaitForBothPlayersReady()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);

            // GetGameData は async なので Task として実行して待機
            var task = gameConnector.GetGameData(roomData.room_id);
            yield return new WaitUntil(() => task.IsCompleted);

            if (this == null) yield break; // シーン移動で破棄されていたら中断

            var data = task.Result;
            if (data == null) continue;

            // 両プレイヤーのキャラが3体以上登録されていれば準備완了とみなす
            int p1count = 0, p2count = 0;
            foreach (var c in data.Characters)
            {
                if (c.Is1P) p1count++;
                else p2count++;
            }

            if (p1count >= 3 && p2count >= 3)
            {
                // 両方の準備が完了したのでバトルシーンへ遷移
                sceneData.next_scene_number = 5;
                yield break;
            }
        }
    }

    void UpDateCharacterUI()
    {
        int allMoveCost = 0;
        allMoveCost
        = characterData.characters[selectedCharacter1].default_move_cost
        + characterData.characters[selectedCharacter2].default_move_cost
        + characterData.characters[selectedCharacter3].default_move_cost;
        party_move_cost.text = "cost : " + allMoveCost;
        SelecuUI[0].sprite = characterData.characters[selectedCharacter1].select_image;
        SelecuUI[1].sprite = characterData.characters[selectedCharacter2].select_image;
        SelecuUI[2].sprite = characterData.characters[selectedCharacter3].select_image;
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

    public async Task SendDatas()
    {
        //ここに自分の編成とコストを送る関数を書いてください

        int[] charas = {selectedCharacter1, selectedCharacter2, selectedCharacter3};
        bool is1p = false;

        var room = await matchingConnector.ListRoom(roomData.room_id);
        for (int i = 0; i < room.Count; i++)
        {
            if (room[i].UserId == userData.user_id && room[i].State == 1)
            {
                is1p = true;
            }
        }
        await battleConnector.RegisterCharacters(roomData.room_id, is1p, charas);
    }

    public async Task<List<int>> GetOpponentDatas()
    {
        //ここに相手の編成とコストを受け取る関数を書いてください
        var data = await battleConnector.GetGameData(roomData.room_id);
        var room = await matchingConnector.ListRoom(roomData.room_id);
        bool is1p = false;
        var opponent_characters = new List<int>(3);
        for (int i = 0; i < room.Count; i++)
        {
            if (room[i].UserId == userData.user_id && room[i].State == 1)
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
        var data = await battleConnector.GetGameData(roomData.room_id);
        return data;
    }
}
