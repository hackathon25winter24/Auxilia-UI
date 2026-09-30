using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

// 戦闘中に変わらない部分のUIの登録用のクラス
public class BaseUIManager : MonoBehaviour
{
    public TextMeshProUGUI[] player_name_texts;
    public Image[] characterSmallwindow;// 自分: 0..2, 相手: 3..5
    private string[] player_name; // Scriptableobject内のプレイヤー名を参照するようにする
    public void ChangeView(BattleDataForOnline data, UserData user, CharacterData assets)
    {
        bool flip = user != null && data.player2.player_id == user.user_id;
        var first = flip ? data.player2 : data.player1;
        var second = flip ? data.player1 : data.player2;
        player_name = new[] { first.player_name, second.player_name };
        for (int i = 0; i < player_name_texts.Length && i < 2; i++)
            if (player_name_texts[i] != null) player_name_texts[i].text = player_name[i];
        var all = first.characters.Concat(second.characters).ToArray();
        for (int i = 0; i < characterSmallwindow.Length; i++)
        {
            var image = characterSmallwindow[i];
            if (image == null) continue;
            int id = i < all.Length ? all[i].unique_id : -1;
            image.enabled = assets != null && id >= 0 && id < assets.characters.Length;
            if (image.enabled) image.sprite = assets.characters[id].default_sprite_smallwindow;
        }
    }
    void Start() // 戦闘開始時にすべてのメソッドを発火させていいはず
    {
        
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
        if (NetworkManager.Instance != null)
        {
            
        }
    }
}
