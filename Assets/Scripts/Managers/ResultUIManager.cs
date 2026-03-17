using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ResultUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public BattleDataforOmline battleDataforOnline;

    public TextMeshProUGUI playerName;
    public TextMeshProUGUI myNewRate;
    public TextMeshProUGUI MyRateUpDown;
    public TextMeshProUGUI NewRate;
    public TextMeshProUGUI RateUpDown;

    public Image[] characters;
    public Sprite[] charactersFaceImage;
    public Sprite[] charactersResult;
    public Image result;
    public Sprite[] resultImage;
    public Image[] UpDown;
    public Sprite[] UpDownImage;

    void Awake()
    {
        playerName.text = battleDataforOnline.opponent_name;
        characters[0].sprite = charactersResult[battleDataforOnline.selected_character[0]];
        characters[1].sprite = charactersResult[battleDataforOnline.selected_character[1]];
        characters[2].sprite = charactersResult[battleDataforOnline.selected_character[2]];
        characters[3].sprite = charactersFaceImage[battleDataforOnline.selected_character[3]];
        characters[4].sprite = charactersFaceImage[battleDataforOnline.selected_character[4]];
        characters[5].sprite = charactersFaceImage[battleDataforOnline.selected_character[5]];

        if (battleDataforOnline.my_player_id == battleDataforOnline.win_player_id)
        {
            result.sprite = resultImage[0];
        }else
        {
            result.sprite = resultImage[1];
        }
    }

    void Start()
    {
        if (battleDataforOnline.my_rate_updown > 0)
        {
            myNewRate.text = battleDataforOnline.rate.ToString();
            MyRateUpDown.text = "+" + battleDataforOnline.my_rate_updown.ToString();
            MyRateUpDown.color = new Color32(255, 122, 122, 255);
            UpDown[0].sprite = UpDownImage[0];
        }else if (battleDataforOnline.my_rate_updown < 0) 
        {
            myNewRate.text = battleDataforOnline.rate.ToString();
            MyRateUpDown.text = battleDataforOnline.my_rate_updown.ToString();
            MyRateUpDown.color = new Color32(122, 122, 255, 255);
            UpDown[0].sprite = UpDownImage[1];
        }else
        {
            myNewRate.text = battleDataforOnline.rate.ToString();
            MyRateUpDown.text = "+0";
            MyRateUpDown.color = new Color32(255, 255, 255, 255);
            UpDown[0].sprite = UpDownImage[0];
        }

        if (battleDataforOnline.opponent_rate_updown > 0)
        {
            NewRate.text = battleDataforOnline.opponent_rate.ToString();
            RateUpDown.text = "+" + battleDataforOnline.opponent_rate_updown.ToString();
            RateUpDown.color = new Color32(255, 122, 122, 255);
            UpDown[1].sprite = UpDownImage[0];
        }else if (battleDataforOnline.opponent_rate_updown < 0) 
        {
            NewRate.text = battleDataforOnline.opponent_rate.ToString();
            RateUpDown.text = battleDataforOnline.opponent_rate_updown.ToString();
            RateUpDown.color = new Color32(122, 122, 255, 255);
            UpDown[1].sprite = UpDownImage[1];
        }else
        {
            NewRate.text = battleDataforOnline.opponent_rate.ToString();
            RateUpDown.text = "+0";
            RateUpDown.color = new Color32(255, 255, 255, 255);
            UpDown[1].sprite = UpDownImage[0];
        }
    }

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "BackToHome":
                sceneData.next_scene_number = 1;
                break;
            case "BackToMatchingRoom":
                sceneData.next_scene_number = 9;
                break;
            case "BackToMatching":
                sceneData.next_scene_number = 3;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
