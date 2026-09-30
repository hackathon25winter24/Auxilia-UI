using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

public class SelectUIManager : MonoBehaviour
{
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

    private NetworkManager Net => NetworkManager.Instance;
    private AuthenticationConnector authenticationConnector => Net?.Auth;
    private MatchingConnector matchingConnector => Net?.Matching;
    private BattleConnector battleConnector => Net?.Battle;
    
    public int selectedCharacterId;

    // 「決定」ボタンを押した後、相手の準備完了を待っている状態かどうか
    private bool _waitingForOpponent = false;
    private bool sendingSelection;
    // 「決定」ボタン上のテキスト（相手待機中メッセージの表示に使う）
    public TextMeshProUGUI decidedButtonText;

    public float maxTime = 100f; 
    private float currentTime;
    private bool isTimerRunning = false;

    private int selectedCharacter1;// デッキ1枠目の選択キャラ保存用変数。以下同様
    private int selectedCharacter2;
    private int selectedCharacter3;

    private int selectedUI;// 1~3のどの枠が押されたか



    async void Start()
    {
        SelectedTub.SetActive(false);
        if (battleConnector == null || battleDataforOnline == null || roomData == null) return;
        battleConnector.BindData(battleDataforOnline);
        var ct = this.GetCancellationTokenOnDestroy();
        try
        {
            if (await battleConnector.GetRoomGameData(roomData.room_id, ct) == null) return;
            await battleConnector.GetDefinitions(ct);
        }
        catch (OperationCanceledException) { return; }
        if (ct.IsCancellationRequested) return;

        var state = battleDataforOnline.State;
        int myState = state.Players.ToList().FindIndex(p => p.Id == userData.user_id) + 1;

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

        }
        else
        {
            playerUI.SetActive(false);
            SpectatorUI.SetActive(true);
            ready.SetActive(false); 
            ready2.SetActive(false);
            nameText.text = battleDataforOnline.player1.player_name;
            nameText2.text = battleDataforOnline.player2.player_name;
            
            // 観戦者もバトル開始を待つ

        }
        TimerStart();
        WatchMatch().Forget();
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
                if (!_waitingForOpponent && !sendingSelection)
                {
                    sendingSelection = true;
                    bool accepted;
                    try { accepted = await SendDatas(); }
                    finally { sendingSelection = false; }
                    if (!accepted || this == null) break;
                    SelectedTub.SetActive(true);
                    _waitingForOpponent = true;
                    if (decidedButtonText != null)
                        decidedButtonText.text = "相手の準備を待っています...";
                    // WatchMatch waits for the server's Started flag.
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

    void Update()
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

    private void SyncRoomStatus()
    {
        var state = battleDataforOnline.State;
        if (state == null) return;
        if (costText != null) costText.text = "cost:" + battleDataforOnline.player1.current_cost_remaining;
        if (costText2 != null) costText2.text = "cost:" + battleDataforOnline.player2.current_cost_remaining;
        if (ready != null) ready.SetActive(state.ReadyPlayerIds.Contains(battleDataforOnline.player1.player_id));
        if (ready2 != null) ready2.SetActive(state.ReadyPlayerIds.Contains(battleDataforOnline.player2.player_id));
        if (nameText != null) nameText.text = battleDataforOnline.player1.player_name;
        if (nameText2 != null) nameText2.text = battleDataforOnline.player2.player_name;
    }

    void TimerStart()
    {
        currentTime = maxTime;
        isTimerRunning = true;
    }

    private async UniTask WatchMatch()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (await battleConnector.GetGameData(ct) != null)
                {
                    SyncRoomStatus();
                    if (battleDataforOnline.State.Started && !battleDataforOnline.State.Finished)
                    {
                        SceneChangeManager.MoveScene(5);
                        return;
                    }
                }
                await UniTask.Delay(1000, cancellationToken: ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    void UpDateCharacterUI()
    {
        var costs = new[] { selectedCharacter1, selectedCharacter2, selectedCharacter3 }
            .Select(id => battleDataforOnline.BaseMoveCost(BattleCharacterIds.ToServer(id))).ToArray();
        party_move_cost.text = costs.All(c => c.HasValue) ? "cost : " + costs.Sum(c => c.Value) : "cost : -";
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

    public async Task<bool> SendDatas()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        try
        {
            var ids = new[] { selectedCharacter1, selectedCharacter2, selectedCharacter3 }
                .Select(BattleCharacterIds.ToServer).ToArray();
            if (await battleConnector.RegisterCharacters(ids, ct) == null) return false;
            return await battleConnector.Ready(ct) != null;
        }
        catch (OperationCanceledException) { return false; }
    }

    public async Task<List<int>> GetOpponentDatas()
    {
        if (await battleConnector.GetGameData(this.GetCancellationTokenOnDestroy()) == null) return new List<int>();
        return battleDataforOnline.State.Characters.Where(c => c.OwnerId != userData.user_id)
            .Select(c => BattleCharacterIds.ToLocal(c.DefinitionId)).ToList();
    }
}
