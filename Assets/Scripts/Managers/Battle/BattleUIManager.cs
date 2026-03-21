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
    public TextMeshProUGUI[] playerNames;
    public Image shadow;
    public Image characterStates;
    public RectTransform backfromStates;
    public Sprite[] characterStatesImage;
    public CharacterManager characterManager;
    public TextMeshProUGUI logText;
    
    public void SubscribeToEvents()
    {
        if (characterManager != null)
        {
            // 重複購読を防ぐため一旦解除
            characterManager.OnAttackExecuted -= HandleAttackExecuted;
            characterManager.OnAttackExecuted += HandleAttackExecuted;
            Debug.Log("<color=green>[BattleUIManager] OnAttackExecuted サブスクライブ完了</color>");
        }
    }

    public void InitUI()
    {
        Debug.Log("[BattleUIManager] InitUI started.");

        // 初期化処理
        for (int i = 0; i <= 5; i++)
        {
            var data = battleDataforOnline.charactersBattleDatas[i];
            
            // スライダーの最大値を設定
            hpSlider[i].maxValue = data.now_character_maxhp;
            // 現在の値をスライダーとテキストに反映
            UpdateCharacterUI(i);
        }

        playerNames[0].text = battleDataforOnline.player1_name;
        playerNames[1].text = battleDataforOnline.player2_name;
        cost[0].text = "cost:" + battleDataforOnline.now_my_cost;
        cost[1].text = "cost:" + battleDataforOnline.now_enemy_cost;
        
        if (shadow != null) shadow.gameObject.SetActive(false);
        if (characterStates != null) characterStates.gameObject.SetActive(false);
        if (backfromStates != null) backfromStates.gameObject.SetActive(false);
        
        Debug.Log("[BattleUIManager] InitUI finished successfully.");
    }

    void OnDestroy()
    {
        if (characterManager != null)
        {
            characterManager.OnAttackExecuted -= HandleAttackExecuted;
        }
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

    private void HandleAttackExecuted(CharacterManager.AttackEventData data)
    {
        Debug.Log($"<color=cyan>[BattleUIManager] HandleAttackExecuted 受信</color>: Attacker={data.attackerUniqueId}, Target={data.targetUniqueId}, Damage={data.finalDamage}");
        string attackerName = GetCharacterName(data.attackerUniqueId);
        string targetName = data.targetUniqueId == 0 ? "拠点" : GetCharacterName(data.targetUniqueId);

        string logMessage = "";
        if (data.isPlayerAttack)
        {
            logMessage = $"<color=#5fb3ff>[味方]</color> {attackerName}の攻撃！ {targetName}に {data.finalDamage} ダメージ！";
        }
        else
        {
            // 相手の攻撃
            logMessage = $"<color=#ff5f5f>[敵]</color> {attackerName}の攻撃！";
            if (data.targetUniqueId != 0 || data.finalDamage != 0)
            {
                logMessage += $" {targetName}に {data.finalDamage} ダメージ！";
            }
        }

        AddLog(logMessage);
        Debug.Log($"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }

    private string GetCharacterName(uint uniqueId)
    {
        for (int i = 0; i < 6; i++)
        {
            if (battleDataforOnline.charactersBattleDatas[i].unique_id == uniqueId)
            {
                int charId = battleDataforLocal.character_id[i];
                if (charId >= 0 && charId < characterData.characters.Length)
                {
                    return characterData.characters[charId].default_name_japanese;
                }
            }
        }
        return "不明";
    }

    private void AddLog(string message)
    {
        Debug.Log($"<color=cyan>[BattleUIManager] AddLog</color>: {message}");
        if (logText != null)
        {
            // 上書きではなく追記にする（最大10行程度に制限するとより良いが、一旦は追記のみ）
            logText.text = message;
        }
        else
        {
            Debug.LogWarning("<color=red>[BattleUIManager] logText (TextMeshProUGUI) がアサインされていません！</color>");
        }
    }

    void Awake()
    {
        Debug.Log("[BattleUIManager] Awake Started");
    }
}
