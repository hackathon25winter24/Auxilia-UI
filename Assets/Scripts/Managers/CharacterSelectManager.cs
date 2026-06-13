using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // 辞書用に追加

public class CharacterSelectManager : MonoBehaviour
{
    [Header("編成スロットUI (3枠)")]
    public Button[] teamSlotButtons = new Button[3];
    public Image[] teamSlotImages = new Image[3];
    public Sprite defaultImage;

    [Header("通常画面")]
    public Button randomFormation;
    public Button backToTitle;
    public TMP_Text partyHPText;
    public TMP_Text partyMovText;

    [Header("パネル")]
    public GameObject characterSelectPanel;
    public GameObject characterDetailPanel;

    [Header("選択パネルのUI")]
    public Button closeSelectPanelButton;
    public Transform scrollViewContent;
    public GameObject characterButtonPrefab;

    [Header("詳細パネルのUI")]
    public Image detailCharacterImage;
    public Button confirmButton;
    public Button closeDetailButton;
    public TMP_Text hpText;
    public TMP_Text moveCostText;
    public TMP_Text characterName;
    public AttackData[] attacks;
    public TMP_Text passiveName;
    public TMP_Text passiveExplanation;
    public Image passiveRange;

    [Header("演出用UI")]
    public CanvasGroup faderCanvasGroup;
    public RectTransform detailPanelRect;
    public float fadeDuration = 0.4f;
    public float scaleDuration = 0.1f;

    private int currentSelectingSlotIndex = -1;
    private int currentViewingCharIndex = -1;
    private bool isAnimating = false;

    [Header("キャラクターデータ (ScriptableObject)")]
    public CharacterData characterDataAsset;

    [Header("プレイヤーデータ（ScriptableObject）")]
    public UserData userData;

    [Header("シーンデータ（ScriptableObject）")]
    public SceneData sceneData;

    [Header("Network")]
    public GameConnector gameConnector;



    [System.Serializable]
    public class AttackData
    {
        public TMP_Text attackName;
        public TMP_Text attackDamage;
        public TMP_Text attackCost;
        public Image attackRange;
    }

    void Awake()
    {
        if (gameConnector == null) gameConnector = FindFirstObjectByType<GameConnector>();
    }

    void Start()
    {
        characterSelectPanel.SetActive(false);
        characterDetailPanel.SetActive(false);

        if (faderCanvasGroup != null)
        {
            faderCanvasGroup.alpha = 0f;
            faderCanvasGroup.gameObject.SetActive(false);
        }

        // イベント登録
        for (int i = 0; i < teamSlotButtons.Length; i++)
        {
            int slotIndex = i;
            teamSlotButtons[i].onClick.AddListener(() => OpenSelectPanelWithFade(slotIndex));
        }

        randomFormation.onClick.AddListener(RandomFormation);
        backToTitle.onClick.AddListener(BackToTitle);

        closeSelectPanelButton.onClick.AddListener(CloseSelectPanelWithFade);
        closeDetailButton.onClick.AddListener(CloseDetailPanelWithAnimation);
        confirmButton.onClick.AddListener(ConfirmSelectionWithFade);

        GenerateCharacterList();
        LoadFormation();
    }

    void GenerateCharacterList()
    {
        CharactersData[] characters = characterDataAsset.characters;

        for (int i = 0; i < characters.Length; i++)
        {
            int charIndex = i;
            GameObject btnObj = Instantiate(characterButtonPrefab, scrollViewContent);

            // TryGetComponentを使用して安全に取得
            if (btnObj.TryGetComponent<Image>(out Image btnImage))
            {
                btnImage.sprite = characters[i].select_image;
            }

            if (btnObj.TryGetComponent<Button>(out Button btn))
            {
                btn.onClick.AddListener(() => OpenDetailPanelWithAnimation(charIndex));
            }
        }
    }

    // PlayerDataから編成を読み込む
    void LoadFormation()
    {
        for (int i = 0; i < teamSlotButtons.Length; i++)
        {
            UpdateSlotUI(i);
        }
        PartyHPandMOV();
    }

    // --- 新規追加: スロットのUI更新を共通化 ---
    void UpdateSlotUI(int slotIndex)
    {
        int savedCharId = GetSavedCharacterId(slotIndex);
        CharactersData savedChar = GetCharacterById(savedCharId);

        if (savedChar != null)
        {
            teamSlotImages[slotIndex].sprite = savedChar.select_image;
        }
        else
        {
            teamSlotImages[slotIndex].sprite = defaultImage;
        }
        teamSlotImages[slotIndex].gameObject.SetActive(true);
    }

    int GetSavedCharacterId(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return userData.deck1;
            case 1: return userData.deck2;
            case 2: return userData.deck3;
            default: return -1;
        }
    }

    void SaveCharacterId(int slotIndex, int id)
    {
        switch (slotIndex)
        {
            case 0: userData.deck1 = id; break;
            case 1: userData.deck2 = id; break;
            case 2: userData.deck3 = id; break;
        }
    }

    CharactersData GetCharacterById(int id)
    {
        foreach (var charData in characterDataAsset.characters)
        {
            if (charData.default_id == id) return charData;
        }
        return null;
    }


    async void OpenSelectPanelWithFade(int slotIndex)
    {
        if (isAnimating) return;
        currentSelectingSlotIndex = slotIndex;
         SEManager.instance?.PlaySelectSE();
        currentSelectingSlotIndex = slotIndex;
        characterSelectPanel.SetActive(true);
        if (gameConnector != null) await gameConnector.UpdateUser();
         StartCoroutine(FadeSequence(true));
    }

    void CloseSelectPanelWithFade()
    {
        if (isAnimating) return;
        StartCoroutine(FadeSequence(false));
    }

    async void ConfirmSelectionWithFade()
    {
        if (isAnimating || currentViewingCharIndex < 0) return;
              SEManager.instance?.PlayBackSE();
        characterSelectPanel.SetActive(false);
        if (gameConnector != null) await gameConnector.UpdateUser();
        StartCoroutine(ConfirmSequence());

    }

    void OpenDetailPanelWithAnimation(int charIndex)
    {
        if (isAnimating) return;

        currentViewingCharIndex = charIndex;

        // 対象のキャラクターデータをキャッシュして可読性を向上
        CharactersData character = characterDataAsset.characters[charIndex];

        detailCharacterImage.sprite = character.detail_image;
        hpText.text = character.default_hp.ToString();
        moveCostText.text = character.default_move_cost.ToString();
        characterName.text = character.default_name_japanese;
        // 設定されている攻撃データ数とUIの枠数の少ない方に合わせてループ（エラー防止）
        int attackCount = Mathf.Min(attacks.Length, character.attacks.Length);
        for (int i = 0; i < attackCount; i++)
        {
            // UI側のスロット自体のチェック
            if (attacks == null || i >= attacks.Length || attacks[i] == null)
            {
                Debug.LogError($"CharacterSelectManager: UI 'attacks' array at index {i} is null or unassigned in inspector.");
                continue;
            }

            // キャラクターデータ側のチェック
            if (character.attacks == null || i >= character.attacks.Length || character.attacks[i] == null)
            {
                if (attacks[i].attackName != null) attacks[i].attackName.text = "---";
                if (attacks[i].attackDamage != null) attacks[i].attackDamage.text = "0";
                if (attacks[i].attackCost != null) attacks[i].attackCost.text = "0";
                if (attacks[i].attackRange != null) attacks[i].attackRange.sprite = null;
                continue;
            }

            if (attacks[i].attackName != null) attacks[i].attackName.text = character.attacks[i].default_attack_name;
            if (attacks[i].attackDamage != null) attacks[i].attackDamage.text = character.attacks[i].default_attack_power.ToString();
            if (attacks[i].attackCost != null) attacks[i].attackCost.text = character.attacks[i].default_attack_cost.ToString();
            if (attacks[i].attackRange != null) attacks[i].attackRange.sprite = character.attacks[i].attack_range_image;

        }

        StartCoroutine(ScaleAnimation(true));
    }

    void CloseDetailPanelWithAnimation()
    {

        if (isAnimating) return;
        StartCoroutine(ScaleAnimation(false));

        SEManager.instance?.PlayBackSE();
        characterDetailPanel.SetActive(false);
    }

    async void ConfirmSelection()
    {
        SEManager.instance?.PlaySelectSE();
        if (currentSelectingSlotIndex >= 0 && currentViewingCharIndex >= 0)
        {
            CharactersData selectedChar = characterDataAsset.characters[currentViewingCharIndex];

            // 既存データのプロパティから選択枠用の画像を取得
            teamSlotImages[currentSelectingSlotIndex].sprite = characterDataAsset.characters[currentViewingCharIndex].select_image;
            teamSlotImages[currentSelectingSlotIndex].gameObject.SetActive(true);

            // PlayerDataへの保存
            SaveCharacterId(currentSelectingSlotIndex, selectedChar.default_id);
        }

        characterDetailPanel.SetActive(false);
        characterSelectPanel.SetActive(false);

        PartyHPandMOV();

        if (gameConnector != null) await gameConnector.UpdateUser();
    }
    async void RandomFormation()
    {
    SEManager.instance?.PlaySelectSE();
    
    // 1. 全キャラクターのリストをコピーして作成
    List<CharactersData> availableCharacters = new List<CharactersData>(characterDataAsset.characters);
    
    // キャラクターが3体未満の場合は重複を許容せざるを得ないのでチェック
    if (availableCharacters.Count < 3)
    {
        Debug.LogWarning("キャラクター総数が3体未満です。重複が発生します。");
    }

    // 2. 各スロットに対して抽選を行う
    int[] selectedIds = new int[3];

    for (int i = 0; i < 3; i++)
    {
        if (availableCharacters.Count > 0)
        {
            // 残っているキャラからランダムに1人選ぶ
            int randomIndex = UnityEngine.Random.Range(0, availableCharacters.Count);
            selectedIds[i] = availableCharacters[randomIndex].default_id;

            // 選んだキャラをリストから削除（これで次は選ばれなくなる）
            availableCharacters.RemoveAt(randomIndex);
        }
        else
        {
            // キャラが足りなくなった場合の予備（通常は通らない）
            selectedIds[i] = -1; 
        }
    }

    // 3. PlayerDataに反映
    userData.deck1 = selectedIds[0];
    userData.deck2 = selectedIds[1];
    userData.deck3 = selectedIds[2];

    // 4. UIの更新処理
    for (int i = 0; i < teamSlotButtons.Length; i++)
    {
        UpdateSlotUI(i);
    }

    PartyHPandMOV();

    if (gameConnector != null) await gameConnector.UpdateUser();
    }

    void BackToTitle()
    {
        SEManager.instance?.PlayToNextSE();
        sceneData.next_scene_number = 1;
        // 実際のシーン遷移処理が必要であれば、ここに記述します
        // SceneManager.LoadScene(sceneData.next_scene_number);
    }

    void PartyHPandMOV()
    {
        int partyHP = characterDataAsset.characters[userData.deck1].default_hp
                    + characterDataAsset.characters[userData.deck2].default_hp
                    + characterDataAsset.characters[userData.deck3].default_hp;

        int partyMov = characterDataAsset.characters[userData.deck1].default_move_cost
                     + characterDataAsset.characters[userData.deck2].default_move_cost
                     + characterDataAsset.characters[userData.deck3].default_move_cost;

        partyHPText.text = partyHP.ToString();
        partyMovText.text = partyMov.ToString();
    }

    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = faderCanvasGroup.alpha;
        float time = 0;

        faderCanvasGroup.gameObject.SetActive(true);

        while (time < duration)
        {
            time += Time.deltaTime;
            faderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        faderCanvasGroup.alpha = targetAlpha;
        if (targetAlpha <= 0f)
        {
            faderCanvasGroup.gameObject.SetActive(false);
        }
    }

    IEnumerator FadeSequence(bool opening)
    {
        isAnimating = true;
        yield return StartCoroutine(Fade(1f, fadeDuration));

        characterSelectPanel.SetActive(opening);

        yield return StartCoroutine(Fade(0f, fadeDuration));
        isAnimating = false;
    }

    IEnumerator ConfirmSequence()
{
    isAnimating = true;
    yield return StartCoroutine(Fade(1f, fadeDuration));

    CharactersData selectedChar = characterDataAsset.characters[currentViewingCharIndex];
    int newCharId = selectedChar.default_id;

    // --- 入れ替え（スワップ）ロジックの追加 ---
    for (int i = 0; i < teamSlotButtons.Length; i++)
    {
        // 1. 自分以外のスロットをループ
        if (i == currentSelectingSlotIndex) continue;

        // 2. 他のスロットに同じキャラIDがすでに設定されているかチェック
        if (GetSavedCharacterId(i) == newCharId)
        {
            // 3. 元々このスロット（currentSelectingSlotIndex）にいたキャラIDを、
            //    重複が見つかったスロット（i）に移動させる（入れ替え）
            int previousCharId = GetSavedCharacterId(currentSelectingSlotIndex);
            SaveCharacterId(i, previousCharId);
            UpdateSlotUI(i); // 移動先のUIも更新
            break; // 重複は1つしかないのでループを抜ける
        }
    }
    // ----------------------------------------

    // 現在のスロットに新しいキャラを保存
    SaveCharacterId(currentSelectingSlotIndex, newCharId);
    UpdateSlotUI(currentSelectingSlotIndex);
    
    PartyHPandMOV();

    characterDetailPanel.SetActive(false);
    characterSelectPanel.SetActive(false);

    yield return StartCoroutine(Fade(0f, fadeDuration));
    isAnimating = false;
}

    IEnumerator ScaleAnimation(bool opening)
    {
        isAnimating = true;

        Vector3 startScale = opening ? Vector3.zero : Vector3.one;
        Vector3 targetScale = opening ? Vector3.one : Vector3.zero;

        if (opening) characterDetailPanel.SetActive(true);

        detailPanelRect.localScale = startScale;
        float time = 0;

        while (time < scaleDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, time / scaleDuration);
            detailPanelRect.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        detailPanelRect.localScale = targetScale;

        if (!opening) characterDetailPanel.SetActive(false);

        isAnimating = false;
    }
}