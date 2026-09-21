using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class TutorialBattleStoryView : MonoBehaviour
{
    [SerializeField] private GameObject storyObject;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Image downArrow;
    [SerializeField] private TextMeshProUGUI autoText;
    private string[] selifs;
    private StoryManagerData storyData;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float autoWaitTime = 1.0f;
    [SerializeField] private float flashSpeed = 5.0f; // 点滅速度

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


    public void ShowAutoText()
    {
        autoText.gameObject.SetActive(true);
    }

    public void HideAutoText()
    {
        autoText.gameObject.SetActive(false);
    }

    public void FlashUIs()
    {
        float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1.0f) / 2.0f;
        downArrow.color = new Color(downArrow.color.r, downArrow.color.g, downArrow.color.b, alpha);
        if (storyData.is_auto) autoText.color = new Color(autoText.color.r, autoText.color.g, autoText.color.b, alpha);
    }

    public void ShowStory()
    {
        storyObject.SetActive(true);
    }

    public void HideStory()
    {
        storyObject.SetActive(false);
    }
}