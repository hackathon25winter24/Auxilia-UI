using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowRoom : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI roomHostText;
    public TextMeshProUGUI roomStateText;
    public TextMeshProUGUI roomJoinnerText;
    public Image StateIcon;

    public Sprite[] StateImage;

    // データをセットするための関数を用意する
    public void SetRoomData(string room_name, string owner_name, bool state, int num_joinner)
    {
        roomNameText.text = room_name;
        roomHostText.text = "親：" + owner_name;
        roomJoinnerText.text = num_joinner.ToString() + "/8";
        if (state == false)
        {
            roomStateText.text = "対戦待機中";
            StateIcon.sprite = StateImage[0];
        }else
        {
            roomStateText.text = "対戦中";
            StateIcon.sprite = StateImage[1];
        }
    }
}
