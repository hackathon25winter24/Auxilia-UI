using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialStoryManager : MonoBehaviour
{
    [Header("Data References")]
    public InputData inputData;
    public SceneData sceneData;
    public StoryManagerData storyManagerData;
    
    [Header("Tutorial Sentences")]
    public string[] serif_tutorial; // インスペクターでセリフを入力

    [Header("UI Elements")]
    public Image DownArrow;
    public TextMeshProUGUI Tell;
    public TextMeshProUGUI autoText;
    public GameObject Texts;
    public GameObject Back;


    [Header("Settings")]
    public float speed = 5.0f; // 点滅速度
    public float typingSpeed = 0.05f;
    public float autoWaitTime = 2.0f;

    private Coroutine activeRoutine;

    void Awake()
    {
        // データの初期化
        storyManagerData.serif_number = 0;
        storyManagerData.is_auto = false;
        storyManagerData.serif_loading = false;
        
        autoText.gameObject.SetActive(false);
        Texts.SetActive(true);
        Back.SetActive(true);

        // 最初のセリフを開始
        StartNewSerif();
    }

    void Update()
    {
        // 1. UIの点滅処理（DownArrowとAutoText）
        float alpha = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        SetUIAlpha(DownArrow, alpha);
        if (storyManagerData.is_auto) SetUIAlpha(autoText, alpha);

        // 2. 入力判定
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

    void StartNewSerif()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(StoryFlowRoutine());
    }

    IEnumerator StoryFlowRoutine()
    {
        // 配列から現在の文字列を取得
        string currentText = serif_tutorial[storyManagerData.serif_number];

        // タイピング演出
        storyManagerData.serif_loading = true;
        Tell.text = "";
        foreach (char letter in currentText.ToCharArray())
        {
            Tell.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        storyManagerData.serif_loading = false;

        // オートモードなら待機して次へ
        if (storyManagerData.is_auto)
        {
            yield return new WaitForSeconds(autoWaitTime);
            AdvanceToNextSerif();
        }
    }

    void OnPlayerClick()
    {
        if (storyManagerData.serif_loading)
        {
            // 文字送り中なら強制終了して全表示
            StopCoroutine(activeRoutine);
            Tell.text = serif_tutorial[storyManagerData.serif_number];
            storyManagerData.serif_loading = false;
            
            if (storyManagerData.is_auto) activeRoutine = StartCoroutine(AutoWaitOnly());
        }
        else
        {
            AdvanceToNextSerif();
        }
    }

    void AdvanceToNextSerif()
    {
        // 配列の要素数を超えないようにチェック
        if (storyManagerData.serif_number < serif_tutorial.Length - 1)
        {
            storyManagerData.serif_number++;
            StartNewSerif();
        }
        else
        {
            EndStory();
        }
    }

    public void ToggleAutoMode()
    {
        storyManagerData.is_auto = !storyManagerData.is_auto;
        autoText.gameObject.SetActive(storyManagerData.is_auto);

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
        // ストーリー番号に応じて遷移先を変更
        sceneData.next_scene_number = (storyManagerData.now_story_number == 0) ? 12 : 11;
    }

    public void SkipStory()
    {
        EndStory();
    }

    void SetUIAlpha(Graphic ui, float alpha)
    {
        if (ui == null) return;
        Color c = ui.color;
        c.a = alpha;
        ui.color = c;
    }
}