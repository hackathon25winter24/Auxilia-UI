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
    public TextMeshProUGUI resultText; // 追加: 勝敗テキスト表示用
    public Image[] UpDown;
    public Sprite[] UpDownImage;

    void Awake()
    {
        // 相手の名前を表示
        if (playerName != null) playerName.text = battleDataforOnline.opponent_name;
        
        // 自分のキャラクター表示（リザルト大画像を使用）
        if (battleDataforOnline.selected_character != null && battleDataforOnline.selected_character.Length >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < characters.Length)
                {
                    int charId = battleDataforOnline.selected_character[i];
                    if (charId >= 0 && charId < charactersResult.Length)
                    {
                        characters[i].sprite = charactersResult[charId];
                    }
                }
            }
        }

        // 敵（相手）のキャラクターアイコン表示（アイコン画像を使用）
        if (battleDataforOnline.selected_character != null && battleDataforOnline.selected_character.Length >= 6)
        {
            for (int i = 0; i < 3; i++)
            {
                int imgIdx = i + 3; // characters配列の3番目からが敵用と想定
                if (imgIdx < characters.Length)
                {
                    int charId = battleDataforOnline.selected_character[imgIdx];
                    if (charId >= 0 && charId < charactersFaceImage.Length)
                    {
                        characters[imgIdx].sprite = charactersFaceImage[charId];
                    }
                }
            }
        }

        // 勝敗に応じた素材切り替え
        if (battleDataforOnline.my_player_id == battleDataforOnline.win_player_id)
        {
            // 勝利
            if (resultImage.Length > 0) result.sprite = resultImage[0];
            if (resultText != null) resultText.text = "VICTORY";
        }else
        {
            // 敗北
            if (resultImage.Length > 1) result.sprite = resultImage[1];
            if (resultText != null) resultText.text = "DEFEAT";
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
