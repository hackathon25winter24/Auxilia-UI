using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    public BattleDataforLocal battleDataforLocal;
    public InputData inputData;
    public GridDataforOnline gridDataforOnline;
    public CharacterData characterData;
    public BattleDataforOmline battleDataforOnline;
    public GridDataforLocal gridDataforLocal;
    public TextMeshProUGUI[] CharacterHP;
    public Slider[] hpSlider;
    
    void Awake()
    {
        // 初期化処理
        for (int i = 0; i <= 5; i++)
        {
            var data = battleDataforOnline.charactersBattleDatas[i];
            
            // スライダーの最大値を設定
            hpSlider[i].maxValue = data.now_character_maxhp;
            // 現在の値をスライダーとテキストに反映
            UpdateCharacterUI(i);
        }
    }

    void Update()
    {
        // 常に最新のデータをUIに反映
        for (int i = 0; i <= 5; i++)
        {
            UpdateCharacterUI(i);
        }
    }

    // 特定のインデックス(i)のキャラクターUIを更新する専用メソッド
    public void UpdateCharacterUI(int i)
    {
        var data = battleDataforOnline.charactersBattleDatas[i];
        
        // スライダーの現在値を更新（maxhpではなくhpを入れる）
        hpSlider[i].value = data.now_character_hp;

        // テキストの更新
        CharacterHP[i].text = data.now_character_hp + "/" + data.now_character_maxhp;
    }
}
