using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TutorialBattleUIManager : MonoBehaviour
{
    public BattleDataForOnline battleDataForTutorial;
    public InputData inputData;
    public GridDataforOnline gridDataforOnline;
    public CharacterData characterData;
    public TextMeshProUGUI[] cost;
    public Sprite[] characterStatesImage;
    public GameObject characterStatus;
    public Image characterStates;
    
    void Awake()
    {
        cost[0].text = "cost:" + battleDataForTutorial.player1.current_cost_remaining;
        cost[1].text = "cost:" + battleDataForTutorial.player2.current_cost_remaining;
        characterStatus.gameObject.SetActive(false);
    }

    void Update()
    {
        cost[0].text = "cost:" + battleDataForTutorial.player1.current_cost_remaining;
        cost[1].text = "cost:" + battleDataForTutorial.player2.current_cost_remaining;
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
