using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

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
    private BattleDataForOnline data;

    public void ChangeView(BattleDataForOnline received, UserData user)
    {
        data = received;
        bool flip = user != null && data.player2.player_id == user.user_id;
        var players = flip ? new[] { data.player2, data.player1 } : new[] { data.player1, data.player2 };
        var all = players.SelectMany(p => p.characters).ToArray();
        for (int i = 0; i < character_hp.Length; i++)
            if (character_hp[i] != null) character_hp[i].text = i < all.Length ? all[i].now_character_hp.ToString() : "";
        for (int i = 0; i < hp_slider.Length && i < all.Length; i++)
            if (hp_slider[i] != null) { hp_slider[i].maxValue = all[i].max_hp; hp_slider[i].value = all[i].now_character_hp; }
        for (int i = 0; i < players.Length; i++)
        {
            if (i < cost_text.Length && cost_text[i] != null) cost_text[i].text = players[i].current_cost_remaining.ToString();
            if (i < base_hp_text.Length && base_hp_text[i] != null) base_hp_text[i].text = players[i].base_hp.ToString();
            if (i < base_hp_slider.Length && base_hp_slider[i] != null)
            { base_hp_slider[i].maxValue = players[i].base_max_hp; base_hp_slider[i].value = players[i].base_hp; }
        }
        if (log_text != null) log_text.text = data.State.LastEvent?.Text ?? "";
        ShowTimer();
    }

    private void Update() => ShowTimer(); // Presentation-only countdown; never advances a turn locally.
    void Start() // メソッドのイベントへの登録
    {
        
    }
    public void ShowCost()
    {
        
    }
    public void ShowTimer()
    {
        if (data?.State == null) return;
        var state = data.State;
        string deadlineText = state.Phase == "turn_end" ? state.PhaseDeadline : state.TurnDeadline;
        float seconds = 0;
        if (!state.Finished && DateTimeOffset.TryParse(deadlineText, out var deadline) &&
            DateTimeOffset.TryParse(state.ServerTime, out var serverTime))
            seconds = Mathf.Max(0, (float)((deadline - serverTime).TotalSeconds -
                (Time.realtimeSinceStartupAsDouble - data.ReceivedAtRealtime)));
        if (time_text != null) time_text.text = Mathf.CeilToInt(seconds).ToString();
        if (timer_slider != null) { timer_slider.maxValue = state.Phase == "turn_end" ? 2 : 120; timer_slider.value = seconds; }
    }
    public void ShowHp() // HPバーとテキストの表示
    {}
    public void ShowLog()
    {}
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        if (NetworkManager.Instance != null)
        {
            
        }
    }
}
