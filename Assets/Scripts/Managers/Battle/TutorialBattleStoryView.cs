using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialBattleStoryView : MonoBehaviour
{
    [SerializeField] private GameObject storyObject;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Image downArrow;
    [SerializeField] private TextMeshProUGUI autoText;
    private string[] selifs;
    private StoryManagerData storyData;
    private float downArrowFlashElapsed;
    private float autoTextFlashElapsed;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float autoWaitTime = 1.0f;
    [SerializeField] private float flashSpeed = 5.0f; // 点滅速度

    public void Init(string[] _selifs, StoryManagerData _storyData)
    {
        selifs = _selifs;
        storyData = _storyData;
    }

    public IEnumerator StoryFlowRoutine()
    {
        // 配列から現在の文字列を取得
        string currentText = selifs[storyData.serif_number];

        // タイピング演出
        storyData.serif_loading = true;
        storyText.text = "";
        foreach (char letter in currentText.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        storyData.serif_loading = false;

        // オートモードなら待機して次へ
        if (storyData.is_auto)
        {
            yield return new WaitForSeconds(autoWaitTime);
            //AdvanceToNextSerif();
        }
    }

    public IEnumerator AutoWaitOnly(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }


    public void ShowAutoText(bool isShow)
    {
        autoText.gameObject.SetActive(isShow);
    }

    public void FlashUIs()
    {
        downArrowFlashElapsed += Time.deltaTime;
        autoTextFlashElapsed += Time.deltaTime;

        SetAlpha(downArrow, GetFlashAlpha(downArrowFlashElapsed));
        SetAlpha(autoText, GetFlashAlpha(autoTextFlashElapsed));
    }

    private float GetFlashAlpha(float elapsedTime)
    {
        return (Mathf.Cos(elapsedTime * flashSpeed) + 1.0f) * 0.5f;
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    public void ResetUIsAlpha(bool downArrow, bool autoText)
    {
        if (downArrow)
        {
            downArrowFlashElapsed = 0.0f;
            SetAlpha(this.downArrow, 1.0f);
        }
        if (autoText)
        {
            autoTextFlashElapsed = 0.0f;
            SetAlpha(this.autoText, 1.0f);
        }
    }


    public void ShowStory(bool isShow)
    {
        storyObject.SetActive(isShow);
    }
}
