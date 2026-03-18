using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Threading.Tasks;

public class StoryManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public StoryData storyData;
    public StoryManagerData storyManagerData;

    public Image backImage;
    public Sprite back_image;
    public Image CharacterImage;
    public Sprite sophie_story;
    public Image DownArrow;
    public TextMeshProUGUI TellingCharacterName;
    public TextMeshProUGUI Tell;
    public float speed = 5.0f;
    public float typingSpeed = 0.05f;
    public TextMeshProUGUI autoText;
    public GameObject RightDownUI;

    void Awake()
    {
        storyManagerData.serif_number = 0;
        backImage.sprite = back_image;
        CharacterImage.sprite = sophie_story;
        storyManagerData.is_auto = false;
        storyManagerData.is_wating = false;
        RightDownUI.SetActive(true);
        autoText.gameObject.SetActive(false);
        TellingCharacterName.text = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].name;
        StopAllCoroutines();
        StartCoroutine(ShowText(storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].serif));
    }

    // Update is called once per frame
    async void Update()
    {
        float alpha = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        Color c = DownArrow.color;
        c.a = alpha;
        DownArrow.color = c;

        if(inputData.space_key_ispressed == true ||inputData.left_mouse_button_ispressed == true)
        {
            if (storyData.stories[storyManagerData.now_story_number].serifs.Length -1 <= storyManagerData.serif_number)
            {
                if(storyManagerData.serif_loading)
                {
                StopAllCoroutines();
                Tell.text = "";
                Tell.text = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].serif;
                storyManagerData.serif_loading = false;
                }else
                {
                    if (storyManagerData.is_tutorial)
                    {
                    sceneData.next_scene_number = 1;
                    }else
                    {
                    sceneData.next_scene_number = 11;
                    }
                }
            }else 
            {
            if(storyManagerData.serif_loading == true)
            {
                StopAllCoroutines();
                Tell.text = "";
                Tell.text = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].serif;
                storyManagerData.serif_loading = false;
            }else if(storyManagerData.serif_loading == false)
            {
            storyManagerData.serif_number ++;
            TellingCharacterName.text = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].name;
            StartCoroutine(ShowText(storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].serif));
            }
            }
        }

        if (storyManagerData.serif_loading == false && storyManagerData.is_auto && storyManagerData.is_wating == false)
        {
            if (storyData.stories[storyManagerData.now_story_number].serifs.Length -1 <= storyManagerData.serif_number)
            {
                if (storyManagerData.is_tutorial)
                {
                    sceneData.next_scene_number = 1;
                }else
                {
                    sceneData.next_scene_number = 11;
                }
            }
            storyManagerData.is_wating = true;
            await Task.Delay(2000);
            storyManagerData.serif_number ++;
            TellingCharacterName.text = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].name;
            StartCoroutine(ShowText(storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].serif));
            storyManagerData.is_wating = false;
        }

        if (inputData.a_key_ispressed)
        {
            storyManagerData.is_auto = !storyManagerData.is_auto;
            if(storyManagerData.is_auto == true)
            {
                autoText.gameObject.SetActive(true);
                RightDownUI.SetActive(false);
            }
            if(storyManagerData.is_auto == false)
            {
                storyManagerData.is_wating = false;
                autoText.gameObject.SetActive(false);
                RightDownUI.SetActive(true);
            }
        }
        if (inputData.s_key_ispressed)
        {
            if (storyManagerData.is_tutorial)
            {
                sceneData.next_scene_number = 1;
            }else
            {
                sceneData.next_scene_number = 11;
            }
        }

        // サイン波を使用して 0.0 〜 1.0 の値を作成
        // 公式: alpha = (sin(時間 * 速度) + 1) / 2
        float alpha2 = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;

        // 色を取得してアルファ値を更新し、再代入
        Color co = autoText.color;
        co.a = alpha2;
        autoText.color = co;
    }

    IEnumerator ShowText(string fullText)
    {
        storyManagerData.serif_loading = true;
        Tell.text = ""; // まず空にする

        foreach (char letter in fullText.ToCharArray())
        {
            Tell.text += letter; // 一文字追加
            yield return new WaitForSeconds(typingSpeed); // 設定した時間待機
        }
        storyManagerData.serif_loading = false;
    }

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Auto":
                storyManagerData.is_auto = !storyManagerData.is_auto;
                if(storyManagerData.is_auto == true)
                {
                    autoText.gameObject.SetActive(true);
                    RightDownUI.SetActive(false);
                }
                if(storyManagerData.is_auto == false)
                {
                    storyManagerData.is_wating = false;
                    autoText.gameObject.SetActive(false);
                    RightDownUI.SetActive(true);
                }
                break;
            case "Skip":
            sceneData.next_scene_number = 1;
                break;
            default:
                break;
        }
    }
}
