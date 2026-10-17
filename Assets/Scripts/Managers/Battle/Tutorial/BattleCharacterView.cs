using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

public class BattleCharacterView : MonoBehaviour
{
    private CharactersData characterData;
    private BattleManager battleManager;
    private RectTransform myTransform;
    private Image myImage;
    private Slider hpSlider;
    private TextMeshProUGUI hpText;
    [SerializeField] private GameObject attackWindow;
    private RectTransform attackWindowTransform;
    [SerializeField] private Image attackWindowBackground;
    [SerializeField] private Image[] attackButtons;

    [SerializeField] Vector2 attackWindowOffset = new Vector2(390, -150);

    // 初期化
    public void Init(CharactersData _characterData, BattleManager battleManager, GameObject smallWindow, bool isPlayer1, int index)
    {
        characterData = _characterData;
        this.battleManager = battleManager;

        hpSlider = smallWindow.GetComponentInChildren<Slider>();
        hpText = smallWindow.GetComponentInChildren<TextMeshProUGUI>();

        smallWindow.SetActive(true);
        smallWindow.GetComponent<Image>().sprite = characterData.default_sprite_smallwindow;

        var statusButton = smallWindow.GetComponentInChildren<Button>();
        statusButton.onClick.AddListener(() => battleManager.ShowStatus(isPlayer1, index));

        if (myTransform == null)
        {
            myTransform = GetComponent<RectTransform>();
        }

        if (myImage == null)
        {
            myImage = GetComponent<Image>();
        }

        if (attackWindowTransform == null && attackWindow != null)
        {
            attackWindowTransform = attackWindow.GetComponent<RectTransform>();
        }

        myImage.sprite = characterData.default_sprite_mini;

        hpSlider.maxValue = characterData.default_hp;
        SetHp(characterData.default_hp);
    }

    // キャラクター位置の反映
    public void SetPosition(Vector2Int pos, bool isOpenAttackWindow)
    {
        if (myTransform == null)
        {
            myTransform = GetComponent<RectTransform>();
        }

        int worldPosX = pos.x * 50 - 175;
        int worldPosY = pos.y * -50 + 60;
        myTransform.anchoredPosition = new Vector2(worldPosX, worldPosY);

        if (isOpenAttackWindow)
        {
            ShowAttackWindow();
        }
    }

    // そのキャラの攻撃ボタンの表示
    public void ShowAttackWindow()
    {
        if (attackWindow != null)
        {
            attackWindow.SetActive(true);

            // 攻撃ボタンの見た目の変更
            attackWindowTransform.anchoredPosition = myTransform.anchoredPosition + attackWindowOffset;
            attackWindowBackground.sprite = characterData.attack_button_backimage;

            for(int i = 0; i < 3; i++)
            {
                attackButtons[i].sprite = characterData.attacks[i].attack_button;
            }
        }
    }

    // 攻撃ボタンの消去
    public void HideAttackWindow()
    {
        if (attackWindow != null)
        {
            attackWindow.SetActive(false);
        }
    }

    public void SetHp(int hp)
    {
        hpSlider.value = hp;
        hpText.text = hp.ToString() + "/" + hpSlider.maxValue.ToString();
    }

    public void Selected(int characterIndex)
    {
        battleManager.SelectedCharacter(true, characterIndex);
    }
}