using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 戦闘中に変わらない部分のUIの登録用のクラス
public class BaseUIManager : MonoBehaviour
{
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private CharacterData characterData;
    [SerializeField] private BattleDataForOnline battleDataForOnline;
    public TextMeshProUGUI[] player_name_texts;
    public Image[] character_smallwindow;// 左: 0..2, 右: 3..5
    void Awake() // 戦闘開始時にすべてのメソッドを発火させていいはず
    {
        viewManager.match_started += ShowPlayerName;
        viewManager.match_started += ShowCharacterSmallwindow;
    }
    public void ShowPlayerName() // プレイヤー名の表示
    {
        player_name_texts[0].text = battleDataForOnline.player1.player_name;
        player_name_texts[1].text = battleDataForOnline.player2.player_name;
    }
    public void ShowCharacterSmallwindow() //左右のキャラクターウィンドウの登録
    {
        for(int i = 0; i < 3; i++)
        {
            int chara_id = int.Parse(battleDataForOnline.player1.characters[i].character_id);
            if(character_smallwindow[i] != null && characterData.characters[chara_id].default_image_mini != null)
            {
                character_smallwindow[i] = characterData.characters[chara_id].default_image_mini;
            }
        }
        for(int i = 0; i < 3; i++)
        {
            int chara_id = int.Parse(battleDataForOnline.player1.characters[i].character_id);
            if(character_smallwindow[i + 3] != null && characterData.characters[chara_id].default_image_mini != null)
            {
                character_smallwindow[i + 3] = characterData.characters[chara_id].default_image_mini;
            }
        }
    }
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        viewManager.match_started -= ShowPlayerName;
        viewManager.match_started -= ShowCharacterSmallwindow;
    }
}
