using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIViewManager : MonoBehaviour
{
    public TextMeshProUGUI[] character_hp;
    public Slider[] hp_slider;
    public TextMeshProUGUI[] cost_text;
    public TextMeshProUGUI log_text;
    public Slider[] base_hp_slider;
    public TextMeshProUGUI[] base_hp_text;
    public Slider timer_slider; 
    public TextMeshProUGUI time_text;
    void Start() // メソッドのイベントへの登録
    {
        
    }
    public void ShowCost()
    {
        
    }
    public void ShowTimer()
    {}
    public void ShowHp() // HPバーとテキストの表示
    {}
    public void ShowLog()
    {}
}
