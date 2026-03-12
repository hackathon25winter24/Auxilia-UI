using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    // Imageそのものよりも、位置をいじるならRectTransformで持つ方が効率的です
    public RectTransform[] characters; 
    public Image[] character_image;
    public BattleDataforLocal battleDataforLocal;
    public InputData inputData;
    public GridDataforOnline gridDataforOnline;
    public Sprite[] Character_mini_image;
    public bool[] character_isSelected;
    public int[] on_grid_number;
    public int selected_character_id;

    void Awake()
    {
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

        // anchoredPositionを使用して、Canvas内の相対座標で配置する
        characters[0].anchoredPosition = new Vector2(-175, 30); 
        character_image[0].sprite = Character_mini_image[battleDataforLocal.character_id1];
        on_grid_number[0] = 0;
        characters[1].anchoredPosition = new Vector2(-125, -70); 
        character_image[1].sprite = Character_mini_image[battleDataforLocal.character_id2];
        on_grid_number[1] = 17;
        characters[2].anchoredPosition = new Vector2(-175, -170); 
        character_image[2].sprite = Character_mini_image[battleDataforLocal.character_id3];
        on_grid_number[2] = 32;
        characters[3].anchoredPosition = new Vector2(175, 30); 
        character_image[3].sprite = Character_mini_image[battleDataforLocal.character_id4];
        on_grid_number[3] = 7;
        characters[4].anchoredPosition = new Vector2(125, -70); 
        character_image[4].sprite = Character_mini_image[battleDataforLocal.character_id5];
        on_grid_number[4] = 22;
        characters[5].anchoredPosition = new Vector2(175, -170); 
        character_image[5].sprite = Character_mini_image[battleDataforLocal.character_id6];
        on_grid_number[5] = 39;

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
        if (character_isSelected[0]) return;
        if (character_isSelected[1]) return;
        if (character_isSelected[2]) return;
        if (character_isSelected[3]) return;
        if (character_isSelected[4]) return;
        if (character_isSelected[5]) return;
        switch (buttonName)
        {
            case "1":
            selected_character_id = 0;
            character_isSelected[selected_character_id] = true;
                break;
            case "2":
            selected_character_id = 1;
            character_isSelected[selected_character_id] = true;
                break;
            case "3":
            selected_character_id = 2;
            character_isSelected[selected_character_id] = true;
                break;
            case "4":
            selected_character_id = 3;
            character_isSelected[selected_character_id] = true;
                break;
            case "5":
            selected_character_id = 4;
            character_isSelected[selected_character_id] = true;
                break;
            case "6":
            selected_character_id = 5;
            character_isSelected[selected_character_id] = true;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    void Update()
    {
        if (inputData.left_mouse_button_ispressed || inputData.right_mouse_button_ispressed)
        {
            if(character_isSelected[0] || character_isSelected[1] || character_isSelected[2] || character_isSelected[3] || character_isSelected[4] || character_isSelected[5])
            {
            for (int i = 0; i <= 5; i++)
            {
            character_isSelected[i] = false;
            }
            }
        }
        if (inputData.up_key_ispressed)
        {
            if(character_isSelected[0] || character_isSelected[1] || character_isSelected[2] || character_isSelected[3] || character_isSelected[4] || character_isSelected[5])
            {
            switch(on_grid_number[selected_character_id])
            {
            case 0: case 1: case 2: case 3: case 4: case 5: case 6: case 7:
            break;
            default:
            if (gridDataforOnline.grid_state[on_grid_number[selected_character_id] += -8] >= 0)
            {
            characters[selected_character_id].anchoredPosition += new Vector2(0, 50); 
            on_grid_number[selected_character_id] += -8;
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
            if (gridDataforOnline.grid_state[on_grid_number[selected_character_id] += 8] >= 0)
            {
            characters[selected_character_id].anchoredPosition += new Vector2(0, -50); 
            on_grid_number[selected_character_id] += 8;
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
            if (gridDataforOnline.grid_state[on_grid_number[selected_character_id] += 1] >= 0)
            {
            characters[selected_character_id].anchoredPosition += new Vector2(50, 0); 
            on_grid_number[selected_character_id] += 1;
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
            if (gridDataforOnline.grid_state[on_grid_number[selected_character_id] += -1] >= 0)
            {
            characters[selected_character_id].anchoredPosition += new Vector2(-50, 0); 
            on_grid_number[selected_character_id] += -1;
            }
            break;
            }
            }
        }
    }
}
