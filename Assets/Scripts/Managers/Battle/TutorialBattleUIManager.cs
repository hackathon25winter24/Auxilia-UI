using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TutorialBattleUIManager : MonoBehaviour
{
    public BattleDataforLocal battleDataforLocal;
    public InputData inputData;
    public GridDataforOnline gridDataforOnline;
    public CharacterData characterData;
    public TextMeshProUGUI[] CharacterHP;
    public Slider[] hpSlider;
    public TextMeshProUGUI[] cost;
    public Sprite[] characterStatesImage;
    public GameObject characterStatus;
    public Image characterStates;
    
    void Awake()
    {
        // 初期化処理
        for (int i = 0; i <= 5; i++)
        {
            var data = battleDataforLocal.charactersBattleDatasLocal[i];
            
            // スライダーの最大値を設定
            hpSlider[i].maxValue = data.now_character_maxhp;
            // 現在の値をスライダーとテキストに反映
            UpdateCharacterUI(i);
        }

        cost[0].text = "cost:" + battleDataforLocal.now_my_cost;
        cost[1].text = "cost:" + battleDataforLocal.now_enemy_cost;
        characterStatus.gameObject.SetActive(false);
    }

    void Update()
    {
        // 常に最新のデータをUIに反映
        for (int i = 0; i <= 5; i++)
        {
            UpdateCharacterUI(i);
        }
        cost[0].text = "cost:" + battleDataforLocal.now_my_cost;
        cost[1].text = "cost:" + battleDataforLocal.now_enemy_cost;
    }

    // 特定のインデックス(i)のキャラクターUIを更新する専用メソッド
    public void UpdateCharacterUI(int i)
    {
        var data = battleDataforLocal.charactersBattleDatasLocal[i];
        
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
            characterStates.sprite = characterStatesImage[battleDataforLocal.character_id[0]];
            characterStatus.gameObject.SetActive(true);
                break;
            case "smallwindow2":
            characterStates.sprite = characterStatesImage[battleDataforLocal.character_id[1]];
            characterStatus.gameObject.SetActive(true);
                break;
            case "smallwindow3":
            characterStates.sprite = characterStatesImage[battleDataforLocal.character_id[2]];
            characterStatus.gameObject.SetActive(true);
                break;
            case "smallwindow4":
            characterStates.sprite = characterStatesImage[battleDataforLocal.character_id[3]];
            characterStatus.gameObject.SetActive(true);
                break;
            case "smallwindow5":
            characterStates.sprite = characterStatesImage[battleDataforLocal.character_id[4]];
            characterStatus.gameObject.SetActive(true);
                break;
            case "smallwindow6":
            characterStates.sprite = characterStatesImage[battleDataforLocal.character_id[5]];
            characterStatus.gameObject.SetActive(true);
                break;
            case "backfromcharacterstates":
            characterStatus.gameObject.SetActive(false);
                break;
        }
    }
}
