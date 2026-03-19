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
    public StoryCharacterData storyCharacterData;

    public Image backImage;
    public Sprite back_image;
    public Image CharacterImage;
    public RectTransform character;
    public Image DownArrow;
    public TextMeshProUGUI TellingCharacterName;
    public TextMeshProUGUI Tell;
    public float speed = 5.0f;
    public float typingSpeed = 0.05f;
    public TextMeshProUGUI autoText;
    public GameObject RightDownUI;
    public GameObject Shadow;
    public GameObject Texts;
    public GameObject selectionPrefab; 
    public Transform selections;

    void Awake()
    {
        storyManagerData.serif_number = 0;
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
                sceneData.next_scene_number = storyData.stories[0].next_scene;
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

        if(storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].is_selection)
        {
            CreateSelectionButtons();
            selections.gameObject.SetActive(true);
            Texts.SetActive(false);
            CharacterImage.gameObject.SetActive(storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].is_character_exist);
        }else
        {
        selections.gameObject.SetActive(false);
        Texts.SetActive(true);
        backImage.sprite = back_image;
        CharacterImage.gameObject.SetActive(storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].is_character_exist);
        CharacterImage.sprite = storyCharacterData.charactersData[storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].characterID]
        .character_face[storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].character_face];
        float size = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].character_size;
        character.localScale = new Vector3(size, size, size);
        Shadow.SetActive(storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].is_shadowed);

        Tell.text = ""; // まず空にする

        foreach (char letter in fullText.ToCharArray())
        {
            Tell.text += letter; // 一文字追加
            yield return new WaitForSeconds(typingSpeed); // 設定した時間待機
        }
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

    void Start()
    {
        CreateSelectionButtons();
    }

    public void CreateSelectionButtons()
    {
        // 1. 既存の古いボタンをすべて削除（念のため）
        foreach (Transform child in selections)
        {
            Destroy(child.gameObject);
        }

        // 2. 選択肢の数だけループして生成
        for (int i = 0; i < storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].num_selection; i++)
        {
            // ボタンを生成して親(Panel)に入れる
            GameObject newButton = Instantiate(selectionPrefab, selections);

            // 3. テキストを書き換える
            TextMeshProUGUI btnText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].selection_text[i];
            }

            // 4. クリック時の処理を登録（インデックスを渡す）
            int index = i;
            newButton.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(index));
        }
    }

    void OnChoiceSelected(int index)
    {
        storyManagerData.serif_number ++;
        TellingCharacterName.text = storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].name;
        StartCoroutine(ShowText(storyData.stories[storyManagerData.now_story_number].serifs[storyManagerData.serif_number].serif));
    }
}
