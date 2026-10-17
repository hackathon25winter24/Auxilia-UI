using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleView : MonoBehaviour
{
    [Header("Components")]
    private BattleManager battleManager;
    private BattleModel battleModel;
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player2NameText;
    [SerializeField] private Slider timeBar;
    [SerializeField] private TextMeshProUGUI player1CostText;
    [SerializeField] private TextMeshProUGUI player2CostText;
    [SerializeField] private Base player1Base;
    [SerializeField] private Base player2Base;
    [SerializeField] private GameObject characterStatus;
    [SerializeField] private Image characterStatusImage;
    [SerializeField] private TextMeshProUGUI gameText;

    [Header("Settings")]
    [SerializeField] private float movingTextDuration = 2.0f;
    [SerializeField] private Vector2 startPosition = new Vector2(1000, 0);
    [SerializeField] private Vector2 destination = new Vector2(-1000, 0);

    public bool IsMoveRoutineRunning { get; private set; }

    public void Init(string player1Name, string player2Name, int initCost, BattleManager battleManager, BattleModel battleModel)
    {
        this.battleManager = battleManager;
        this.battleModel = battleModel;

        player1NameText.text = player1Name;
        player2NameText.text = player2Name;
        player1CostText.text = initCost.ToString();
        player2CostText.text = initCost.ToString();

        player1Base.slider.maxValue = battleModel.player1.maxBaseHp;
        player2Base.slider.maxValue = battleModel.player2.maxBaseHp;
        SetBaseHp(true, battleModel.player1.baseHp);
        SetBaseHp(false, battleModel.player2.baseHp);

        timeBar.maxValue = battleModel.maxTime;
        timeBar.value = battleModel.maxTime;
    }

    public void SetGameText(string text)
    {
        if (gameText != null)
        {
            gameText.text = text;
        }
    }

    public IEnumerator MoveRoutine()
    {
        IsMoveRoutineRunning = true;
        float elapsed = 0f;

        while (elapsed < movingTextDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / movingTextDuration;
            float slowingT = t + Mathf.Sin(t * Mathf.PI * 2f) * 0.15f; 

            gameText.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, destination, slowingT);

            yield return null;
        }
        gameText.rectTransform.anchoredPosition = destination;
        IsMoveRoutineRunning = false;
    }

    public void SetRemainingCost(bool isPlayer1, int cost)
    {
        if (isPlayer1 && player1CostText != null)
        {
            player1CostText.text = cost.ToString();
        }
        else if (!isPlayer1 && player2CostText != null)
        {
            player2CostText.text = cost.ToString();
        }
    }

    public void SetRemainingTime(float time)
    {
        if (timeBar != null)
        {
            timeBar.value = Mathf.Clamp(time, timeBar.minValue, timeBar.maxValue);
        }
    }

    public void SetBaseHp(bool isPlayer1, int hp)
    {
        Base _base = isPlayer1 ? player1Base : player2Base;
        var _slider = _base.slider;

        _slider.value = Mathf.Clamp(hp, _slider.minValue, _slider.maxValue);
        _base.text.text = Mathf.Clamp(hp, _slider.minValue, _slider.maxValue).ToString() + "/" + _slider.maxValue;
    }

    public void ShowStatus(Sprite statusSprite)
    {
        if (characterStatus != null)
        {
            characterStatus.SetActive(true);
        }

        if (characterStatusImage != null && statusSprite != null)
        {
            characterStatusImage.sprite = statusSprite;
        }
    }

    public void HideStatus()
    {
        if (characterStatus != null)
        {
            characterStatus.SetActive(false);
        }
    }

    public void AttackButtonClicked(int attackIndex)
    {
        battleManager.AttackButtonClicked(attackIndex);
    }

    public void ShowStatusButtonClicked_player1(int characterIndex)
    {
        battleManager.ShowStatus(true, characterIndex);
    }

    public void ShowStatusButtonClicked_player2(int characterIndex)
    {
        battleManager.ShowStatus(false, characterIndex);
    }

    [System.Serializable]
    private class Base
    {
        public Slider slider;
        public TextMeshProUGUI text;
    }
}
