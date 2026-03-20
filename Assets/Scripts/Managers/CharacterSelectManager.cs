using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

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

    [Header("キャラクターデータ (ScriptableObject)")]
    public CharacterData characterDataAsset;

    private int currentSelectingSlotIndex = -1;
    private int currentViewingCharIndex = -1;

    [Header("プレイヤーデータ（ScriptableObject）")]
    public PlayerData playerData;

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
        // public TMP_Text attackSpecial;
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

        // 編成枠ボタンのイベント登録
        for (int i = 0; i < teamSlotButtons.Length; i++)
        {
            int slotIndex = i;
            teamSlotButtons[i].onClick.AddListener(() => OpenSelectPanel(slotIndex));
        }

        // 通常画面操作ボタンのイベント登録
        randomFormation.onClick.AddListener(RandomFormation);
        backToTitle.onClick.AddListener(BackToTitle);

        // パネル操作ボタンのイベント登録
        closeSelectPanelButton.onClick.AddListener(CloseSelectPanel);
        closeDetailButton.onClick.AddListener(CloseDetailPanel);
        confirmButton.onClick.AddListener(ConfirmSelection);

        // リストを生成
         GenerateCharacterList();

        // シーン開始時に編成を読み込む
        LoadFormation();
    }

    void GenerateCharacterList()
    {
        // 既存データの characters 配列を取得
        CharactersData[] characters = characterDataAsset.characters;

        for (int i = 0; i < characters.Length; i++)
        {
            int charIndex = i;

            GameObject btnObj = Instantiate(characterButtonPrefab, scrollViewContent);
            Image btnImage = btnObj.GetComponent<Image>();

            // 既存データのプロパティ（default_sprite）から画像を取得して適用
            btnImage.sprite = characters[i].select_image;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnCharacterListButtonClicked(charIndex));
        }
    }

    // PlayerDataから編成を読み込む
    void LoadFormation()
    {
        for(int i = 0; i < teamSlotButtons.Length; i++)
        {
            // スロットのインデックス(0~2)に応じたIDを取得
            int savedCharId = GetSavedCharacterId(i);

            // 保存されているIDからキャラクターデータを検索
            CharactersData savedChar = GetCharacterById(savedCharId);

            if (savedChar != null)
            {
                teamSlotImages[i].sprite = savedChar.select_image;
                teamSlotImages[i].gameObject.SetActive(true);
            }
            else
            {
                // データがない場合は画像を非表示にする
                teamSlotImages[i].sprite = defaultImage;
                teamSlotImages[i].gameObject.SetActive(true);
            }
        }

        PartyHPandMOV();
    }

    // スロット番号に応じた PlayerData の変数を読み込むヘルパーメソッド
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

    // スロット番号に応じた PlayerData の変数に保存するヘルパーメソッド
    void SaveCharacterId(int slotIndex, int id)
    {
        switch (slotIndex)
        {
            case 0: playerData.character_formation_one = id; break;
            case 1: playerData.character_formation_two = id; break;
            case 2: playerData.character_formation_three = id; break;
        }
    }

    // IDからキャラクターデータを検索するヘルパーメソッド
    CharactersData GetCharacterById(int id)
    {
        foreach (var charData in characterDataAsset.characters)
        {
            // キャラクターが見つかったらそのデータを返す
            if (charData.default_id == id)
            {
                return charData;
            }
        }
        return null; // 見つからなかった場合
    }

    async void OpenSelectPanel(int slotIndex)
    {
        currentSelectingSlotIndex = slotIndex;
        characterSelectPanel.SetActive(true);
        if (gameConnector != null) await gameConnector.UpdateUser();
    }

    async void CloseSelectPanel()
    {
        characterSelectPanel.SetActive(false);
        if (gameConnector != null) await gameConnector.UpdateUser();
    }

    void OnCharacterListButtonClicked(int charIndex)
    {
        currentViewingCharIndex = charIndex;

        // 既存データのプロパティから詳細用画像を取得
        detailCharacterImage.sprite = characterDataAsset.characters[charIndex].detail_image;

        int hp = characterDataAsset.characters[charIndex].default_hp;
        int moveCost = characterDataAsset.characters[charIndex].default_move_cost;
        hpText.text = hp.ToString();
        moveCostText.text = moveCost.ToString();
        characterName.text = characterDataAsset.characters[charIndex].default_name_japanese;

        var characterAttacks = characterDataAsset.characters[charIndex].attacks;
        for(int i = 0; i < 3; i++)
        {
            // UI側のスロット自体のチェック
            if (attacks == null || i >= attacks.Length || attacks[i] == null)
            {
                Debug.LogError($"CharacterSelectManager: UI 'attacks' array at index {i} is null or unassigned in inspector.");
                continue;
            }

            // キャラクターデータ側のチェック
            if (characterAttacks == null || i >= characterAttacks.Length || characterAttacks[i] == null)
            {
                if (attacks[i].attackName != null) attacks[i].attackName.text = "---";
                if (attacks[i].attackDamage != null) attacks[i].attackDamage.text = "0";
                if (attacks[i].attackCost != null) attacks[i].attackCost.text = "0";
                if (attacks[i].attackRange != null) attacks[i].attackRange.sprite = null;
                continue;
            }

            if (attacks[i].attackName != null) attacks[i].attackName.text = characterAttacks[i].default_attack_name;
            if (attacks[i].attackDamage != null) attacks[i].attackDamage.text = characterAttacks[i].default_attack_power.ToString();
            if (attacks[i].attackCost != null) attacks[i].attackCost.text = characterAttacks[i].default_attack_cost.ToString();
            if (attacks[i].attackRange != null) attacks[i].attackRange.sprite = characterAttacks[i].attack_range_image;
        }

        characterDetailPanel.SetActive(true);
    }

    void CloseDetailPanel()
    {
        characterDetailPanel.SetActive(false);
    }

    async void ConfirmSelection()
    {
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
        CharactersData[] characters = characterDataAsset.characters;
        System.Random random = new System.Random();

        playerData.character_formation_one = random.Next(0, characters.Length);
        playerData.character_formation_two = random.Next(0, characters.Length);
        playerData.character_formation_three = random.Next(0, characters.Length);
        // Debug.Log("無作為な編成が作製されました");

        for (int i = 0; i < teamSlotButtons.Length; i++)
        {
            // スロットのインデックス(0~2)に応じたIDを取得
            int savedCharId = GetSavedCharacterId(i);

            // 保存されているIDからキャラクターデータを検索
            CharactersData savedChar = GetCharacterById(savedCharId);

            if (savedChar != null)
            {
                teamSlotImages[i].sprite = savedChar.select_image;
                teamSlotImages[i].gameObject.SetActive(true);
            }
            else
            {
                // データがない場合は画像を非表示にする
                teamSlotImages[i].sprite = defaultImage;
                teamSlotImages[i].gameObject.SetActive(true);
            }

            PartyHPandMOV();
        }

        if (gameConnector != null) await gameConnector.UpdateUser();
    }

    void BackToTitle()
    {
        sceneData.next_scene_number = 1;
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
}