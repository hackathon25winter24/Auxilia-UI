using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 戦闘中に変わらない部分のUIの登録用のクラス
public class BaseUIManager : MonoBehaviour
{
    [SerializeField] private ViewManager viewManager;
    public TextMeshProUGUI[] player_name_texts;
    public Image[] characterSmallwindow;// 自分: 0..2, 相手: 3..5
    private string[] player_name; // Scriptableobject内のプレイヤー名を参照するようにする
    void Awake() // 戦闘開始時にすべてのメソッドを発火させていいはず
    {
        viewManager.match_started += ShowPlayerName;
        viewManager.match_started += ShowCharacterSmallwindow;
    }
    public void ShowPlayerName() // プレイヤー名の表示
    {
        for(int i = 0; i < 2; i++)
        {
            player_name_texts[i].text = player_name[i];
        }
    }
    public void ShowCharacterSmallwindow() //左右のキャラクターウィンドウの登録
    {}
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        viewManager.match_started -= ShowPlayerName;
        viewManager.match_started -= ShowCharacterSmallwindow;
    }
}
