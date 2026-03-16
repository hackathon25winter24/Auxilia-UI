using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowJoinner : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerRateText;
    public Image StateIcon;
    public Image StateBack;

    public Sprite[] StateImage;
    public Sprite[] StateBackImage;

    // データをセットするための関数を用意する
    public void SetRoomData(string player_name, int player_rate, int state)
    {
        playerNameText.text = player_name;
        playerRateText.text = player_rate.ToString();
        StateIcon.sprite = StateImage[state]; 
        StateBack.sprite = StateBackImage[state];
    }
}