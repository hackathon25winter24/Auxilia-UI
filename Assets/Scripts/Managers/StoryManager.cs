using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public StoryData storyData;

    public Image backImage;
    public Sprite back_image;
    public Image CharacterImage;
    public Sprite sophie_story;
    public Image DownArrow;
    public TextMeshProUGUI TellingCharacterName;
    public TextMeshProUGUI Tell;
    public float speed = 5.0f;
    public float typingSpeed = 0.05f;

    void Awake()
    {
        backImage.sprite = back_image;
        CharacterImage.sprite = sophie_story;
        TellingCharacterName.text = storyData.stories[0].serifs[0].name;
        StopAllCoroutines();
        StartCoroutine(ShowText(storyData.stories[0].serifs[0].serif));
    }

    // Update is called once per frame
    void Update()
    {
        float alpha = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        Color c = DownArrow.color;
        c.a = alpha;
        DownArrow.color = c;
    }

    IEnumerator ShowText(string fullText)
    {
        Tell.text = ""; // まず空にする

        foreach (char letter in fullText.ToCharArray())
        {
            Tell.text += letter; // 一文字追加
            yield return new WaitForSeconds(typingSpeed); // 設定した時間待機
        }
    }
}
