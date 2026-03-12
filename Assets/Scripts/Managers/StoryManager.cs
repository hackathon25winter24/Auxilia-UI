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

    public int serif_number;
    public bool serif_loading;

    void Awake()
    {
        serif_number = 0;
        backImage.sprite = back_image;
        CharacterImage.sprite = sophie_story;
        TellingCharacterName.text = storyData.stories[storyData.now_story_number].serifs[serif_number].name;
        StopAllCoroutines();
        StartCoroutine(ShowText(storyData.stories[storyData.now_story_number].serifs[serif_number].serif));
    }

    // Update is called once per frame
    void Update()
    {
        float alpha = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        Color c = DownArrow.color;
        c.a = alpha;
        DownArrow.color = c;

        if(inputData.space_key_ispressed == true ||inputData.left_mouse_button_ispressed == true)
        {
            if (storyData.stories[storyData.now_story_number].serifs.Length -2 < serif_number)
            {
                if(serif_loading == true)
                {
                StopAllCoroutines();
                Tell.text = "";
                Tell.text = storyData.stories[storyData.now_story_number].serifs[serif_number].serif;
                serif_loading = false;
                }else if(serif_loading == false)
                {
                sceneData.next_scene_number = 1;
                }
            }else 
            {
            if(serif_loading == true)
            {
                StopAllCoroutines();
                Tell.text = "";
                Tell.text = storyData.stories[storyData.now_story_number].serifs[serif_number].serif;
                serif_loading = false;
            }else if(serif_loading == false)
            {
            serif_number ++ ;
            TellingCharacterName.text = storyData.stories[storyData.now_story_number].serifs[serif_number].name;
            StartCoroutine(ShowText(storyData.stories[storyData.now_story_number].serifs[serif_number].serif));
            }
            }
        }
    }

    IEnumerator ShowText(string fullText)
    {
        serif_loading = true;
        Tell.text = ""; // まず空にする

        foreach (char letter in fullText.ToCharArray())
        {
            Tell.text += letter; // 一文字追加
            yield return new WaitForSeconds(typingSpeed); // 設定した時間待機
        }
        serif_loading = false;
    }
}
