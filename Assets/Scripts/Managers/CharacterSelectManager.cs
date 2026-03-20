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
    public PlayerData playerData;

    [Header("シーンデータ（ScriptableObject）")]
    public SceneData sceneData;

    [System.Serializable]
    public class AttackData
    {
        public TMP_Text attackName;
        public TMP_Text attackDamage;
        public TMP_Text attackCost;
        public Image attackRange;
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
            case 0: return playerData.character_formation_one;
            case 1: return playerData.character_formation_two;
            case 2: return playerData.character_formation_three;
            default: return -1;
        }
    }

    void SaveCharacterId(int slotIndex, int id)
    {
        switch (slotIndex)
        {
            case 0: playerData.character_formation_one = id; break;
            case 1: playerData.character_formation_two = id; break;
            case 2: playerData.character_formation_three = id; break;
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

    void OpenSelectPanelWithFade(int slotIndex)
    {
        if (isAnimating) return;
        currentSelectingSlotIndex = slotIndex;
        StartCoroutine(FadeSequence(true));
    }

    void CloseSelectPanelWithFade()
    {
        if (isAnimating) return;
        StartCoroutine(FadeSequence(false));
    }

    void ConfirmSelectionWithFade()
    {
        if (isAnimating || currentViewingCharIndex < 0) return;
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
            attacks[i].attackName.text = character.attacks[i].default_attack_name;
            attacks[i].attackDamage.text = character.attacks[i].default_attack_power.ToString();
            attacks[i].attackCost.text = character.attacks[i].default_attack_cost.ToString();
            attacks[i].attackRange.sprite = character.attacks[i].attack_range_image;
        }

        StartCoroutine(ScaleAnimation(true));
    }

    void CloseDetailPanelWithAnimation()
    {
        if (isAnimating) return;
        StartCoroutine(ScaleAnimation(false));
    }

    void RandomFormation()
    {
        CharactersData[] characters = characterDataAsset.characters;
        if (characters.Length == 0) return; // データが存在しない場合のエラー回避

        // インデックスではなく、キャラクターのIDを保存するように修正
        playerData.character_formation_one = characters[UnityEngine.Random.Range(0, characters.Length)].default_id;
        playerData.character_formation_two = characters[UnityEngine.Random.Range(0, characters.Length)].default_id;
        playerData.character_formation_three = characters[UnityEngine.Random.Range(0, characters.Length)].default_id;

        // UIの更新処理（共通化したメソッドを使用）
        for (int i = 0; i < teamSlotButtons.Length; i++)
        {
            UpdateSlotUI(i);
        }

        // ステータス更新は全スロットの書き換えが終わった後に1回だけ実行する
        PartyHPandMOV();
    }

    void BackToTitle()
    {
        sceneData.next_scene_number = 1;
        // 実際のシーン遷移処理が必要であれば、ここに記述します
        // SceneManager.LoadScene(sceneData.next_scene_number);
    }

    void PartyHPandMOV()
    {
        int partyHP = characterDataAsset.characters[playerData.character_formation_one].default_hp
                    + characterDataAsset.characters[playerData.character_formation_two].default_hp
                    + characterDataAsset.characters[playerData.character_formation_three].default_hp;

        int partyMov = characterDataAsset.characters[playerData.character_formation_one].default_move_cost
                     + characterDataAsset.characters[playerData.character_formation_two].default_move_cost
                     + characterDataAsset.characters[playerData.character_formation_three].default_move_cost;

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

        // データ保存とUI更新
        SaveCharacterId(currentSelectingSlotIndex, selectedChar.default_id);
        UpdateSlotUI(currentSelectingSlotIndex); // 共通化したメソッドを使用
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