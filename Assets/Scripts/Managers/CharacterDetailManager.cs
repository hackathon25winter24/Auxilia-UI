using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterDetailManager : MonoBehaviour
{
    [Header("パネル")]
    public GameObject charSelectPanel;
    public GameObject charProfilePanel;
    public GameObject charDetailPanel;

    [Header("選択パネルのUI")]
    public Button backToTitle;
    public Transform scrollViewContent;
    public GameObject characterButtonPrefab;

    [Header("パネル諸々のUI")]
    public Button closePanelButton;
    public TMP_Text characterName;
    public Button changeButton;

    [Header("プロフィールパネルのUI")]
    public Image charDetail;
    public TMP_Text charName;
    public TMP_Text charGender;
    public TMP_Text charAge;
    public TMP_Text charBirthday;
    public TMP_Text charAffiliation;
    public TMP_Text charBirthplace;
    public TMP_Text charBloodType;
    public TMP_Text charHeight;
    public TMP_Text charWeight;
    public TMP_Text charHobby;

    [Header("詳細パネルのUI")]
    public Image charMini;
    public Image charBig;
    public TMP_Text hpText;
    public TMP_Text moveCostText;
    public AttackData[] attacks;
    public TMP_Text passiveName;
    public TMP_Text passiveExplanation;
    public Image passiveRange;

    [Header("演出用UI")]
    public CanvasGroup faderCanvasGroup; // FaderのCanvasGroup
    public float fadeDuration = 0.4f;     // 暗転にかかる時間
    public float scaleDuration = 0.1f;    // 詳細パネルの展開時間

    private int currentViewingCharIndex = -1;
    private bool ProfileOpening = false;
    private bool isAnimating = false; // アニメーション中の連打防止フラグ

    [Header("キャラクターデータ (ScriptableObject)")]
    public CharacterData characterDataAsset;

    [Header("キャラクタープロフィール（ScriptableObject）")]
    public CharacterProfiles charProfs;

    [Header("シーンデータ（ScriptableObject）")]
    public SceneData sceneData;


    [System.Serializable]
    public class AttackData
    {
        public TMP_Text attackName;
        public TMP_Text attackDamage;
        public TMP_Text attackCost;
        // public TMP_Text attackSpecial;
        public Image attackRange;
    }

    void Start()
    {
        charSelectPanel.SetActive(true);
        charProfilePanel.SetActive(false);
        charDetailPanel.SetActive(false);

        // Faderを透明にして非表示にしておく
        if (faderCanvasGroup != null)
        {
            faderCanvasGroup.alpha = 0f;
            faderCanvasGroup.gameObject.SetActive(false);
        }

        // 通常画面操作ボタンのイベント登録
        backToTitle.onClick.AddListener(BackToTitle);

        // パネル操作ボタンのイベント登録
        closePanelButton.onClick.AddListener(ClosePanel);
        changeButton.onClick.AddListener(ChangePanel);

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
            btnImage.sprite = characters[i].select_image;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OpenPanel(charIndex));
        }
    }

    void OpenPanel(int charIndex)
    {
        Debug.Log("パネルを開くボタンが押されました");
        if (isAnimating) return;

        currentViewingCharIndex = charIndex;

        // 既存データのプロパティから詳細用画像を取得
        charMini.sprite = charProfs.char_profs[charIndex].image_mini;
        charBig.sprite = charProfs.char_profs[charIndex].image_big;
        charDetail.sprite = charProfs.char_profs[charIndex].image_detail;

        int hp = characterDataAsset.characters[charIndex].default_hp;
        int moveCost = characterDataAsset.characters[charIndex].default_move_cost;
        hpText.text = hp.ToString();
        moveCostText.text = moveCost.ToString();
        characterName.text = charProfs.char_profs[charIndex].name;

        for (int i = 0; i < 3; i++)
        {
            attacks[i].attackName.text = characterDataAsset.characters[charIndex].attacks[i].default_attack_name;
            attacks[i].attackDamage.text = characterDataAsset.characters[charIndex].attacks[i].default_attack_power.ToString();
            attacks[i].attackCost.text = characterDataAsset.characters[charIndex].attacks[i].default_attack_cost.ToString();
            attacks[i].attackRange.sprite = characterDataAsset.characters[charIndex].attacks[i].attack_range_image;
        }

        charName.text = charProfs.char_profs[charIndex].name;
        charGender.text = charProfs.char_profs[charIndex].gender;
        charAge.text = charProfs.char_profs[charIndex].age;
        charBirthday.text = charProfs.char_profs[charIndex].birthday;
        charAffiliation.text = charProfs.char_profs[charIndex].affiliation;
        charBirthplace.text = charProfs.char_profs[charIndex].birthplace;
        charBloodType.text = charProfs.char_profs[charIndex].blood_type;
        charHeight.text = charProfs.char_profs[charIndex].height;
        charWeight.text = charProfs.char_profs[charIndex].weight;
        charHobby.text = charProfs.char_profs[charIndex].hobby;

        // アニメーション開始
        StartCoroutine(FadeSequence(true));
    }

    void ClosePanel()
    {
        if (isAnimating) return;
        StartCoroutine(FadeSequence(false)); // パネルを閉じるフェード
    }

    void ChangePanel()
    {
        if (isAnimating) return;
        StartCoroutine(ChangingFade());
    }

    void BackToTitle()
    {
        sceneData.next_scene_number = 1;
    }

    // フェード（暗転）の本体
    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = faderCanvasGroup.alpha;
        float time = 0;

        faderCanvasGroup.gameObject.SetActive(true);

        while (time < duration)
        {
            time += Time.deltaTime;
            // イージング（滑らかな動き）のためにTime.deltaTimeを正規化
            faderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null; // 1フレーム待つ
        }

        faderCanvasGroup.alpha = targetAlpha;
        if (targetAlpha <= 0f)
        {
            faderCanvasGroup.gameObject.SetActive(false);
        }
    }

    // 演出シーケンス
    IEnumerator FadeSequence(bool opening)
    {
        isAnimating = true;

        // 1. 暗くする
        yield return StartCoroutine(Fade(1f, fadeDuration));

        // 2. 暗転中にパネルの開閉を切り替える
        if (opening)
        {
            charSelectPanel.SetActive(false);
            charDetailPanel.SetActive(true);
        }
        else
        {
            charSelectPanel.SetActive(true);
            charProfilePanel.SetActive(false);
            charDetailPanel.SetActive(false);
            ProfileOpening = false;
        }

        // 3. 明るくする
        yield return StartCoroutine(Fade(0f, fadeDuration));

        isAnimating = false;
    }

    IEnumerator ChangingFade()
    {
        isAnimating = true;

        // 1. 暗くする
        yield return StartCoroutine(Fade(1f, fadeDuration));

        // 2. 暗転中にパネルの開閉を切り替える
        if (ProfileOpening)
        {
            charProfilePanel.SetActive(false);
            ProfileOpening = false;
        }
        else
        {
            charProfilePanel.SetActive(true);
            ProfileOpening = true;
        }

        // 3. 明るくする
        yield return StartCoroutine(Fade(0f, fadeDuration));

        isAnimating = false;
    }
}