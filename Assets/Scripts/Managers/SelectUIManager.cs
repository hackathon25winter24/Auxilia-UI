using UnityEngine;
using System;
using System.Linq;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
    private BattleConnector battleConnector => Net?.Battle;
    
    public int selectedCharacterId;

    // 「決定」ボタンを押した後、相手の準備完了を待っている状態かどうか
    private bool _waitingForOpponent = false;
    // 「決定」ボタン上のテキスト（相手待機中メッセージの表示に使う）
    public TextMeshProUGUI decidedButtonText;

    public float maxTime = 100f; 
    private float currentTime;
    private bool isTimerRunning;

    private int selectedCharacter1;// デッキ1枠目の選択キャラ保存用変数。以下同様
    private int selectedCharacter2;
    private int selectedCharacter3;

    private int selectedUI;// 1~3のどの枠が押されたか


    private bool submitting, initialized, navigating;
    private string matchId;

    private async void Start()
    {
        try
        {
            SelectedTub.SetActive(false);
            battleDataforOnline = battleConnector.Data;
            var snapshot = await battleConnector.GetGameData(roomData.room_id, this.GetCancellationTokenOnDestroy());
            if (snapshot?.State == null || this == null) return;
            await battleConnector.GetDefinitions(this.GetCancellationTokenOnDestroy());
            if (this == null) return;
            matchId = snapshot.State.MatchId;
            var state = battleDataforOnline.State;
            bool isPlayer = state.Players.Any(p => p.Id == userData.user_id);
            playerUI.SetActive(isPlayer);
            SpectatorUI.SetActive(!isPlayer);
            selectedCharacter1 = userData.deck1;
            selectedCharacter2 = userData.deck2;
            selectedCharacter3 = userData.deck3;
            characterTub.SetActive(false);
            start_game_text.gameObject.SetActive(false);
            if (isPlayer) UpDateCharacterUI();
            TimerStart();
            initialized = true;
            battleDataforOnline.Changed += OnBattleChanged;
            OnBattleChanged(BattleDataForOnline.StateSyncType);
        }
        catch (OperationCanceledException) { }
        catch (Exception e) { Debug.LogError($"選択情報の取得に失敗しました: {e.Message}"); }
    }

    private void OnDestroy()
    {
        if (battleDataforOnline != null) battleDataforOnline.Changed -= OnBattleChanged;
    }

    // Preserve the original local countdown as a display only. Reaching zero
    // must not submit a formation, mark ready, or start a server-owned match.
    private void TimerStart()
    {
        currentTime = Mathf.Max(0, maxTime);
        isTimerRunning = true;
        UpdateTimerText();
    }

    private void Update()
    {
        if (!isTimerRunning || navigating) return;
        currentTime = Mathf.Max(0, currentTime - Time.deltaTime);
        UpdateTimerText();
        if (currentTime <= 0)
        {
            isTimerRunning = false;
            Debug.Log("タイムアップ！");
        }
    }

    private void UpdateTimerText()
    {
        string remaining = Mathf.CeilToInt(currentTime).ToString();
        if (timertext != null) timertext.text = remaining;
        if (timertext2 != null) timertext2.text = remaining;
    }

    private void OnBattleChanged(string type)
    {
        var state = battleDataforOnline.State;
        if (!initialized || navigating || state == null || state.MatchId != matchId) return;
        if (state.Finished || state.Started)
        {
            navigating = true;
            SceneChangeManager.MoveScene(state.Finished ? 6 : 5);
            return;
        }
        _waitingForOpponent = state.ReadyPlayerIds.Contains(userData.user_id);
        SelectedTub.SetActive(_waitingForOpponent);
        if (decidedButtonText != null) decidedButtonText.text = _waitingForOpponent ? "相手の準備を待っています..." : "決定";
        if (state.Players.Count >= 2)
        {
            nameText.text = state.Players[0].Name;
            nameText2.text = state.Players[1].Name;
            ready.SetActive(state.ReadyPlayerIds.Contains(state.Players[0].Id));
            ready2.SetActive(state.ReadyPlayerIds.Contains(state.Players[1].Id));
            costText.text = "cost:" + state.Players[0].Cost;
            costText2.text = "cost:" + state.Players[1].Cost;
        }
    }

    public async void OnButtonClick(string buttonName)
    {
        if (!initialized || navigating || submitting || _waitingForOpponent) return;
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
                submitting = true;
                try { await SendDatas(); }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogError($"編成の送信に失敗しました: {e.Message}"); }
                finally { submitting = false; }
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
    if (!initialized || submitting || _waitingForOpponent || navigating) return;
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

    public void RandomizeFormation()
    {
    if (!initialized || submitting || _waitingForOpponent || navigating) return;
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
        if (!initialized || battleDataforOnline.State?.MatchId != matchId) return;
        if (!battleDataforOnline.State.Players.Any(p => p.Id == userData.user_id)) return;
        var ct = this.GetCancellationTokenOnDestroy();
        string[] ids = { BattleCharacterIds.ToServer(selectedCharacter1),
            BattleCharacterIds.ToServer(selectedCharacter2), BattleCharacterIds.ToServer(selectedCharacter3) };
        if (ids.Distinct().Count() != 3) throw new InvalidOperationException("異なるキャラクターを3体選択してください。");
        var registered = await battleConnector.RegisterCharacters(ids, ct);
        if (registered == null) return;
        await battleConnector.Ready(ct);
        if (this != null) OnBattleChanged(BattleDataForOnline.StateSyncType);
    }

    public async Task<List<int>> GetOpponentDatas()
    {
        var snapshot = await battleConnector.GetGameData(roomData.room_id, this.GetCancellationTokenOnDestroy());
        return snapshot?.State?.Characters.Where(c => c.OwnerId != userData.user_id)
            .Select(c => BattleCharacterIds.ToLocal(c.DefinitionId)).ToList() ?? new List<int>();
    }
}
