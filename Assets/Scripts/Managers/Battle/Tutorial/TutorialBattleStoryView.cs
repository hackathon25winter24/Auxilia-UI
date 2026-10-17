using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialBattleStoryView : MonoBehaviour
{
    private TutorialBattleStoryManager storyManager;
    [SerializeField] private GameObject storyObject;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Image downArrow;
    [SerializeField] private TextMeshProUGUI autoText;
    private string[] selifs;
    private StoryManagerData storyData;
    private float downArrowFlashElapsed;
    private float autoTextFlashElapsed;

    [Header("Settings")]
    [SerializeField] private float flashSpeed = 5.0f; // 点滅速度

    public void Init(string[] _selifs, StoryManagerData _storyData, TutorialBattleStoryManager _storyManager)
    {
        selifs = _selifs;
        storyData = _storyData;
        storyManager = _storyManager;
    }

    public void SetStoryText(string text)
    {
        storyText.text = text;
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
        downArrow.gameObject.SetActive(isShow);
        if(storyManager != null)
        {
            storyManager.isStoryHidden = !isShow;
        }
    }
}
