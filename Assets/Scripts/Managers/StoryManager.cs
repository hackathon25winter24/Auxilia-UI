using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StoryManager : MonoBehaviour
{
    [Header("Data References")]
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public StoryData storyData;
    public StoryManagerData storyManagerData;
    public StoryCharacterData storyCharacterData;

    [Header("UI Elements")]
    public Image backImage;
    public Sprite back_image;
    public Image CharacterImage;
    public RectTransform character;
    public Image DownArrow;
    public TextMeshProUGUI TellingCharacterName;
    public TextMeshProUGUI Tell;
    public TextMeshProUGUI autoText;
    public GameObject RightDownUI;
    public GameObject Shadow;
    public GameObject Texts;
    public GameObject selectionPrefab;
    public Transform selections;

    [Header("Settings")]
    public float speed = 5.0f; // 点滅速度
    public float typingSpeed = 0.05f;
    public float autoWaitTime = 2.0f;

    private Coroutine activeRoutine; // 現在実行中のコルーチンを保持

    void Awake()
    {
        // データの初期化
        storyManagerData.serif_number = 0;
        storyManagerData.is_auto = false;
        storyManagerData.serif_loading = false;
        
        RightDownUI.SetActive(true);
        autoText.gameObject.SetActive(false);
        selections.gameObject.SetActive(false);

        // 最初のセリフを開始
        StartNewSerif();
    }

    void Update()
    {
        // 1. UIの点滅処理（DownArrowとAutoText）
        float alpha = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        SetUIAlpha(DownArrow, alpha);
        if (storyManagerData.is_auto) SetUIAlpha(autoText, alpha);

        // 2. 入力判定（Spaceキー、左クリック、Aキー、Sキー）
        // ※ inputData の変数が「押した瞬間」だけ true になると想定しています
        if (inputData.space_key_ispressed || inputData.left_mouse_button_ispressed)
        {
            OnPlayerClick();
        }

        if (inputData.a_key_ispressed)
        {
            ToggleAutoMode();
        }

        if (inputData.s_key_ispressed)
        {
            SkipStory();
        }
    }

    // 次のセリフに進む準備
    void StartNewSerif()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(StoryFlowRoutine());
    }

    // 文字表示からオート待機までの一連の流れを管理
    IEnumerator StoryFlowRoutine()
    {
        var currentSerif = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number];
        
        // 名前の更新
        TellingCharacterName.text = currentSerif.name;

        // 選択肢がある場合
        if (currentSerif.is_selection)
        {
            Texts.SetActive(false);
            selections.gameObject.SetActive(true);
            CreateSelectionButtons();
            yield break; // 選択されるまでここで終了
        }

        // 通常のセリフ表示
        Texts.SetActive(true);
        selections.gameObject.SetActive(false);
        
        // キャラクター表示設定
        UpdateCharacterUI(currentSerif);

        // タイピング演出
        storyManagerData.serif_loading = true;
        Tell.text = "";
        foreach (char letter in currentSerif.serif.ToCharArray())
        {
            Tell.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        storyManagerData.serif_loading = false;

        // オートモードなら一定時間待って次へ
        if (storyManagerData.is_auto)
        {
            yield return new WaitForSeconds(autoWaitTime);
            AdvanceToNextSerif();
        }
    }

    // プレイヤーが画面をクリックした時の処理
    void OnPlayerClick()
    {
        // 選択肢が出ている時はクリックを無効化
        if (storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].is_selection) return;

        if (storyManagerData.serif_loading)
        {
            // 文字送り中なら強制終了して全表示
            StopCoroutine(activeRoutine);
            Tell.text = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].serif;
            storyManagerData.serif_loading = false;
            
            // オート中ならここでも待機コルーチンを開始
            if (storyManagerData.is_auto) activeRoutine = StartCoroutine(AutoWaitOnly());
        }
        else
        {
            // 読み終わっていれば次へ
            AdvanceToNextSerif();
        }
    }

    // セリフ番号を1進めて次を表示
    void AdvanceToNextSerif()
    {
        if (storyManagerData.serif_number < storyData.stories[storyManagerData.now_story_number].serifs.Length - 1)
        {
            storyManagerData.serif_number++;
            StartNewSerif();
        }
        else
        {
            // ストーリー終了
            EndStory();
        }
    }

    // キャラクター画像の表示更新
    void UpdateCharacterUI(SerifData current)
    {
        backImage.sprite = back_image;
        CharacterImage.gameObject.SetActive(current.is_character_exist);
        if (current.is_character_exist)
        {
            var charData = storyCharacterData.charactersData[current.characterID];
            CharacterImage.sprite = charData.character_face[current.character_face];
            character.localScale = Vector3.one * current.character_size;
        }
        Shadow.SetActive(current.is_shadowed);
    }

    public void CreateSelectionButtons()
    {
        foreach (Transform child in selections) Destroy(child.gameObject);

        var current = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number];
        for (int i = 0; i < current.num_selection; i++)
        {
            GameObject newButton = Instantiate(selectionPrefab, selections);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = current.selection_text[i];
            int index = i;
            newButton.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(index));
        }

        if (storyManagerData.is_auto && current.num_selection > 0)
        {
        // 少し待ってから選ぶ演出を入れたい場合はコルーチンを呼ぶ
        StartCoroutine(AutoSelectFirstChoice());
        }
    }

    // オート時に一番上を勝手に選ぶコルーチン
IEnumerator AutoSelectFirstChoice()
{
    // 即座に選ぶとプレイヤーが何が起きたか分からないので、1秒ほど待機
    yield return new WaitForSeconds(1.0f);

    // まだオートモードが継続中であれば、0番目（一番上）を選択実行
    if (storyManagerData.is_auto)
    {
        Debug.Log("オートモード：一番上の選択肢を自動選択しました");
        OnChoiceSelected(0);
    }
}

    void OnChoiceSelected(int index)
    {
        // 選択後の処理（必要に応じて index に基づく分岐を追加）
        AdvanceToNextSerif();
    }

    public void ToggleAutoMode()
    {
        storyManagerData.is_auto = !storyManagerData.is_auto;
        autoText.gameObject.SetActive(storyManagerData.is_auto);
        RightDownUI.SetActive(!storyManagerData.is_auto);

        // オートONにした時に文字を読み終えていたら、即座に待機開始
        if (storyManagerData.is_auto && !storyManagerData.serif_loading)
        {
            activeRoutine = StartCoroutine(AutoWaitOnly());
        }
    }

    IEnumerator AutoWaitOnly()
    {
        yield return new WaitForSeconds(autoWaitTime);
        AdvanceToNextSerif();
    }

    void EndStory()
    {
        sceneData.next_scene_number = storyManagerData.is_tutorial ? 1 : 11;
    }

    public void SkipStory()
    {
        EndStory();
    }

    void SetUIAlpha(Graphic ui, float alpha)
    {
        Color c = ui.color;
        c.a = alpha;
        ui.color = c;
    }
}