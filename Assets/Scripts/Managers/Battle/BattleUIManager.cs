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
    public TextMeshProUGUI[] CharacterHP;
    public Slider[] hpSlider;
    public TextMeshProUGUI[] cost;
    public Image shadow;
    public Image characterStates;
    public RectTransform backfromStates;
    public Sprite[] characterStatesImage;
    
    public void InitUI()
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

        cost[0].text = "cost:" + battleDataforOnline.now_my_cost;
        cost[1].text = "cost:" + battleDataforOnline.now_enemy_cost;
        shadow.gameObject.SetActive(false);
        characterStates.gameObject.SetActive(false);
        backfromStates.gameObject.SetActive(false);
    }

    void Update()
    {
        // 常に最新のデータをUIに反映
        for (int i = 0; i <= 5; i++)
        {
            UpdateCharacterUI(i);
        }
        cost[0].text = battleDataforOnline.now_my_cost.ToString();
        cost[1].text = battleDataforOnline.now_enemy_cost.ToString();
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

        public void OnButtonClick(string buttonName)
    {
        switch(buttonName)
        {
            case "smallwindow1":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[battleDataforOnline.selected_character[0]];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow2":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[battleDataforOnline.selected_character[1]];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow3":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[battleDataforOnline.selected_character[2]];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow4":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[battleDataforOnline.selected_character[3]];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow5":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[battleDataforOnline.selected_character[4]];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow6":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[battleDataforOnline.selected_character[5]];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "backfromcharacterstates":
            SEManager.instance?.PlayBackSE();
            shadow.gameObject.SetActive(false);
            characterStates.gameObject.SetActive(false);
            backfromStates.gameObject.SetActive(false);
                break;
        }
    }
}
