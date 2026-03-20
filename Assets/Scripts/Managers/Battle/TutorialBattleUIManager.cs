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
    public TextMeshProUGUI[] cost;
    public Sprite[] characterStatesImage;
    public GameObject characterStatus;
    public Image characterStates;
    
    void Awake()
    {
        cost[0].text = "cost:" + battleDataforLocal.now_my_cost;
        cost[1].text = "cost:" + battleDataforLocal.now_enemy_cost;
        characterStatus.gameObject.SetActive(false);
    }

    void Update()
    {
        cost[0].text = "cost:" + battleDataforLocal.now_my_cost;
        cost[1].text = "cost:" + battleDataforLocal.now_enemy_cost;
    }

        public void OnButtonClick(string buttonName)
    {
        switch(buttonName)
        {
            case "smallwindow":
            characterStates.sprite = characterStatesImage[0];
            characterStatus.gameObject.SetActive(true);
                break;
            case "backfromcharacterstates":
            characterStatus.gameObject.SetActive(false);
                break;
        }
    }
}
