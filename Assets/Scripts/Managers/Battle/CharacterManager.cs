using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    public RectTransform[] characters; 
    public Image[] character_image;
    public BattleDataforLocal battleDataforLocal;
    public InputData inputData;
    public GridDataforOnline gridDataforOnline;
    public CharacterData characterData;
    public BattleDataforOmline battleDataforOnline;
    public Sprite[] Character_mini_image;
    public bool[] character_isSelected;
    public int[] on_grid_number;
    public int selected_character_id;
    public RectTransform AttackButton;
    public Image AttackButtonBackImage;
    public Image[] AttackButtonOne;
    public Image[] CharacterSmallwindow;
    public Sprite[] CharacterSmallwindowImage;
    public RectTransform BackButton;
    public TextMeshProUGUI[] CharacterHP;
    public Slider[] hpSlider;

    void Awake()
    {
        BackButton.gameObject.SetActive(false);
        // 配列が空、または要素が足りない場合の安全策
        if (characters == null || characters.Length < 6)
        {
            Debug.LogError("CharacterManager: characters配列に6つの要素をアサインしてください。");
            return;
        }

        for (int i = 0; i <= 5; i++)
        {
        character_isSelected[i] = false;
        }

        AttackButton.gameObject.SetActive(false);

        // anchoredPositionを使用して、Canvas内の相対座標で配置する
        characters[0].anchoredPosition = new Vector2(-175, 30); 
        character_image[0].sprite = Character_mini_image[battleDataforLocal.character_id[0]];
        on_grid_number[0] = 0;
        CharacterSmallwindow[0].sprite = CharacterSmallwindowImage[battleDataforLocal.character_id[0]];
        characters[1].anchoredPosition = new Vector2(-125, -70); 
        character_image[1].sprite = Character_mini_image[battleDataforLocal.character_id[1]];
        on_grid_number[1] = 17;
        CharacterSmallwindow[1].sprite = CharacterSmallwindowImage[battleDataforLocal.character_id[1]];
        characters[2].anchoredPosition = new Vector2(-175, -170); 
        character_image[2].sprite = Character_mini_image[battleDataforLocal.character_id[2]];
        on_grid_number[2] = 32;
        CharacterSmallwindow[2].sprite = CharacterSmallwindowImage[battleDataforLocal.character_id[2]];
        characters[3].anchoredPosition = new Vector2(175, 30); 
        character_image[3].sprite = Character_mini_image[battleDataforLocal.character_id[3]];
        on_grid_number[3] = 7;
        CharacterSmallwindow[3].sprite = CharacterSmallwindowImage[battleDataforLocal.character_id[3]];
        characters[4].anchoredPosition = new Vector2(125, -70); 
        character_image[4].sprite = Character_mini_image[battleDataforLocal.character_id[4]];
        on_grid_number[4] = 22;
        CharacterSmallwindow[4].sprite = CharacterSmallwindowImage[battleDataforLocal.character_id[4]];
        characters[5].anchoredPosition = new Vector2(175, -170); 
        character_image[5].sprite = Character_mini_image[battleDataforLocal.character_id[5]];
        on_grid_number[5] = 39;
        CharacterSmallwindow[5].sprite = CharacterSmallwindowImage[battleDataforLocal.character_id[5]];

        for (int i = 0; i <= 5; i++)
        {
        battleDataforOnline.charactersBattleDatas[i].now_character_hp = characterData.characters[battleDataforLocal.character_id[i]].default_hp;
        battleDataforOnline.charactersBattleDatas[i].now_character_maxhp = characterData.characters[battleDataforLocal.character_id[i]].default_hp;
        battleDataforOnline.charactersBattleDatas[i].now_character_move_cost = characterData.characters[battleDataforLocal.character_id[i]].default_move_cost;
        }
        battleDataforOnline.now_my_cost = 50;

        for (int i = 0; i <= 5; i++)
        {
            CharacterHP[i].text = battleDataforOnline.charactersBattleDatas[i].now_character_hp+  "/" +battleDataforOnline.charactersBattleDatas[i].now_character_maxhp;
            SetMaxHealth(battleDataforOnline.charactersBattleDatas[i].now_character_maxhp);
        }

        for (int i = 0; i <= 2; i++)
        {
        characters[i].localScale = new Vector3(-1, 1, 1);
        }
    }

    void Start()
    {
        gridDataforOnline.grid_state[0] = -1;
        gridDataforOnline.grid_state[17] = -1;
        gridDataforOnline.grid_state[32] = -1;
        gridDataforOnline.grid_state[7] = -1;
        gridDataforOnline.grid_state[22] = -1;
        gridDataforOnline.grid_state[39] = -1;

    }

    public void OnButtonClick(string buttonName)
    {
        if (character_isSelected[0])
        {
            if(buttonName == "BackButton")
            {
            for (int i = 0; i <= 5; i++)
            {
            character_isSelected[i] = false;
            AttackButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
            }
            }
        }
        if (character_isSelected[1])
        {
            if(buttonName == "BackButton")
            {
            for (int i = 0; i <= 5; i++)
            {
            character_isSelected[i] = false;
            AttackButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
            }
            }
        }
        if (character_isSelected[2])
        {
            if(buttonName == "BackButton")
            {
            for (int i = 0; i <= 5; i++)
            {
            character_isSelected[i] = false;
            AttackButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
            }
            }
        }
        switch (buttonName)
        {
            case "1":
            selected_character_id = 0;
            BackButton.gameObject.SetActive(true);
            character_isSelected[selected_character_id] = true;
            AttackButton.gameObject.SetActive(true);
            AttackButton.anchoredPosition = characters[0].anchoredPosition + new Vector2(100, 0); 
            AttackButtonOne[0].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[0].attack_button;
            AttackButtonOne[1].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[1].attack_button;
            AttackButtonOne[2].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[2].attack_button;
            AttackButtonBackImage.sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attack_button_backimage;
                break;
            case "2":
            selected_character_id = 1;
            BackButton.gameObject.SetActive(true);
            character_isSelected[selected_character_id] = true;
            AttackButton.gameObject.SetActive(true);
            AttackButton.anchoredPosition = characters[1].anchoredPosition + new Vector2(100, 0); 
            AttackButtonOne[0].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[0].attack_button;
            AttackButtonOne[1].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[1].attack_button;
            AttackButtonOne[2].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[2].attack_button;
            AttackButtonBackImage.sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attack_button_backimage;
                break;
            case "3":
            selected_character_id = 2;
            BackButton.gameObject.SetActive(true);
            character_isSelected[selected_character_id] = true;
            AttackButton.gameObject.SetActive(true);
            AttackButton.anchoredPosition = characters[2].anchoredPosition + new Vector2(100, 0); 
            AttackButtonOne[0].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[0].attack_button;
            AttackButtonOne[1].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[1].attack_button;
            AttackButtonOne[2].sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attacks[2].attack_button;
            AttackButtonBackImage.sprite = characterData.characters[battleDataforLocal.character_id[selected_character_id]].attack_button_backimage;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    void Update()
    {
        if (inputData.up_key_ispressed)
        {
            if(character_isSelected[0] || character_isSelected[1] || character_isSelected[2] || character_isSelected[3] || character_isSelected[4] || character_isSelected[5])
            {
            switch(on_grid_number[selected_character_id])
            {
            case 0: case 1: case 2: case 3: case 4: case 5: case 6: case 7:
            break;
            default:
            if (gridDataforOnline.grid_state[on_grid_number[selected_character_id] + -8] >= 0)
            {
            characters[selected_character_id].anchoredPosition += new Vector2(0, 50); 
            on_grid_number[selected_character_id] += -8;
            gridDataforOnline.grid_state[on_grid_number[selected_character_id] +8] = 0;
            gridDataforOnline.grid_state[on_grid_number[selected_character_id]] = -1;
            AttackButton.anchoredPosition += new Vector2(0, 50); 
            }
            break;
            }
            }
        }
        if (inputData.down_key_ispressed)
        {
            if(character_isSelected[0] || character_isSelected[1] || character_isSelected[2] || character_isSelected[3] || character_isSelected[4] || character_isSelected[5])
            {
            switch(on_grid_number[selected_character_id])
            {
            case 32: case 33: case 34: case 35: case 36: case 37: case 38: case 39:
            break;
            default:
            if (gridDataforOnline.grid_state[on_grid_number[selected_character_id] + 8] >= 0)
            {
            characters[selected_character_id].anchoredPosition += new Vector2(0, -50); 
            on_grid_number[selected_character_id] += 8;
            gridDataforOnline.grid_state[on_grid_number[selected_character_id] -8] = 0;
            gridDataforOnline.grid_state[on_grid_number[selected_character_id]] = -1;
            AttackButton.anchoredPosition += new Vector2(0, -50); 
            }
            break;
            }
            }
        }
        if (inputData.right_key_ispressed)
        {
            if(character_isSelected[0] || character_isSelected[1] || character_isSelected[2] || character_isSelected[3] || character_isSelected[4] || character_isSelected[5])
            {
            switch(on_grid_number[selected_character_id])
            {
            case 7: case 15: case 23: case 31: case 39:
            break;
            default:
            if (gridDataforOnline.grid_state[on_grid_number[selected_character_id] + 1] >= 0)
            {
            characters[selected_character_id].anchoredPosition += new Vector2(50, 0); 
            on_grid_number[selected_character_id] += 1;
            gridDataforOnline.grid_state[on_grid_number[selected_character_id] -1] = 0;
            gridDataforOnline.grid_state[on_grid_number[selected_character_id]] = -1;
            AttackButton.anchoredPosition += new Vector2(50, 0); 
            }
            break;
            }
            }
        }
        if (inputData.left_key_ispressed)
        {
            if(character_isSelected[0] || character_isSelected[1] || character_isSelected[2] || character_isSelected[3] || character_isSelected[4] || character_isSelected[5])
            {
                switch(on_grid_number[selected_character_id])
            {
            case 0: case 8: case 16: case 24: case 32:
            break;
            default:
            if (gridDataforOnline.grid_state[on_grid_number[selected_character_id] - 1] >= 0)
            {
            characters[selected_character_id].anchoredPosition += new Vector2(-50, 0); 
            on_grid_number[selected_character_id] += -1;
            gridDataforOnline.grid_state[on_grid_number[selected_character_id] +1] = 0;
            gridDataforOnline.grid_state[on_grid_number[selected_character_id]] = -1;
            AttackButton.anchoredPosition += new Vector2(-50, 0); 
            }
            break;
            }
            }
        }
    }

    // 最大HPを設定し、スライダーの最大値を合わせる
    public void SetMaxHealth(int health)
    {
        for (int i = 0; i <= 5; i++)
        {
            hpSlider[i].maxValue = health;
            hpSlider[i].value = health;
        }
    }

    // 現在のHPをスライダーに反映させる
    public void SetHealth(int health)
    {
        for (int i = 0; i <= 5; i++)
        {
            hpSlider[i].value = health;
        }
    }
}
