using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;

public class UIViewManager : MonoBehaviour
{
    [SerializeField] private ViewManager viewManager;
    public TextMeshProUGUI[] character_hp_text;
    public Slider[] character_hp_slider;
    public TextMeshProUGUI[] cost_text;
    public TextMeshProUGUI log_text;
    public Slider[] base_hp_slider;
    public TextMeshProUGUI[] base_hp_text;
    public Slider timer_slider; 
    public TextMeshProUGUI time_text;
    void Start() // メソッドのイベントへの登録
    {
        viewManager.cost_changed += ShowCost;
        viewManager.turn_changed += StartTimer;
    }
    public void ShowCost()
    {
        cost_text[0].text = "50";
        cost_text[1].text = "50";
    }
    public void StartTimer()
    {}
    public void ShowBaseHp() // 拠点のHPバーとテキストの表示
    {
        if (base_hp_slider[0] != null) base_hp_slider[0].value = 400;
        if (base_hp_slider[1] != null) base_hp_slider[1].value = 400;
        if (base_hp_text[0] != null) base_hp_text[0].text = "/400";
        if (base_hp_text[1] != null) base_hp_text[1].text = "/400";
    }
    public void ShowCharacterHp() // キャラクターのHPバーとテキストの更新
    {
        for(int i = 0; i <= 5; i++)
        {
            character_hp_slider[i].value = 100;

            // テキストの更新
            character_hp_text[i].text = 100 + "/" + 100;
        }
    }
    public void ShowLog()
    {}
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        viewManager.cost_changed -= ShowCost;
        viewManager.cost_changed -= StartTimer;
    }
}
