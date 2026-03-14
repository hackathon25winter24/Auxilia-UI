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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        for (int i = 0; i <= 5; i++)
        {
            CharacterHP[i].text = battleDataforOnline.charactersBattleDatas[i].now_character_hp+  "/" +battleDataforOnline.charactersBattleDatas[i].now_character_maxhp;
            SetMaxHealth(battleDataforOnline.charactersBattleDatas[i].now_character_maxhp);
        }
    }

    // 最大HPを設定し、スライダーの最大値を合わせる
    public void SetMaxHealth(int health)
    {
        for (int i = 0; i <= 5; i++)
        {
            hpSlider[i].maxValue = health;
            hpSlider[i].value = health;
        }
    }

    // 現在のHPをスライダーに反映させる
    public void SetHealth(int health)
    {
        for (int i = 0; i <= 5; i++)
        {
            hpSlider[i].value = health;
        }
    }
}
