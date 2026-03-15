using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("編成スロットUI (3枠)")]
    public Button[] teamSlotButtons = new Button[3];
    public Image[] teamSlotImages = new Image[3];

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

    [Header("キャラクターデータ (ScriptableObject)")]
    // 既存の CharacterData (ScriptableObject) を受け取る変数
    public CharacterData characterDataAsset;

    private int currentSelectingSlotIndex = -1;
    private int currentViewingCharIndex = -1;

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

        // パネル操作ボタンのイベント登録
        closeSelectPanelButton.onClick.AddListener(CloseSelectPanel);
        closeDetailButton.onClick.AddListener(CloseDetailPanel);
        confirmButton.onClick.AddListener(ConfirmSelection);

        // リストを生成
         GenerateCharacterList();
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
            // 後から変更予定
            btnImage.sprite = characters[i].default_sprite;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnCharacterListButtonClicked(charIndex));
        }
    }

    void OpenSelectPanel(int slotIndex)
    {
        currentSelectingSlotIndex = slotIndex;
        characterSelectPanel.SetActive(true);
    }

    void CloseSelectPanel()
    {
        characterSelectPanel.SetActive(false);
    }

    void OnCharacterListButtonClicked(int charIndex)
    {
        currentViewingCharIndex = charIndex;

        // 既存データのプロパティから詳細用画像を取得
        // 後から変更予定
        detailCharacterImage.sprite = characterDataAsset.characters[charIndex].default_sprite;

        int hp = characterDataAsset.characters[charIndex].default_hp;
        int moveCost = characterDataAsset.characters[charIndex].default_move_cost;
        hpText.text = hp.ToString();
        moveCostText.text = moveCost.ToString();

        characterDetailPanel.SetActive(true);
    }

    void CloseDetailPanel()
    {
        characterDetailPanel.SetActive(false);
    }

    void ConfirmSelection()
    {
        if (currentSelectingSlotIndex >= 0 && currentViewingCharIndex >= 0)
        {
            // 既存データのプロパティから選択枠用の画像を取得
            // 後から変更予定
            teamSlotImages[currentSelectingSlotIndex].sprite = characterDataAsset.characters[currentViewingCharIndex].default_sprite;
            teamSlotImages[currentSelectingSlotIndex].gameObject.SetActive(true);
        }

        characterDetailPanel.SetActive(false);
        characterSelectPanel.SetActive(false);
    }
}