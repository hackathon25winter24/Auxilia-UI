using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BattleViewManager : MonoBehaviour
{
    // ここでオンラインバトル中の画面表示全体を担いたい。1p2pを自分と相手に解釈し直して表示を決めたり、座標反転はここでやる
    public UserData userData;
    public BattleDataForOnline battleDataForOnline;
    public CharacterData characterData;

    public GameObject[] characters;// 自分: 0..2, 相手: 3..5
    public Image[] characterSmallwindow;// 自分: 0..2, 相手: 3..5
    public PlayerState self;
    public PlayerState opponent;
    private bool is_1p;

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
    private Coroutine _clearLogCoroutine;
    
    [Header("Base UI")]
    public Slider myBaseSlider;
    public Slider enemyBaseSlider;
    public TextMeshProUGUI myBaseHPText;
    public TextMeshProUGUI enemyBaseHPText;
    
    void Awake()
    {
        Debug.Log("[BattleViewManager] Awake Started");
    }

        void Start()
    {
        userData = GetSo(userData);
        // battleOnlineManagerのAwakeでgameData.Player1/2Idが入ってる
        is_1p = (userData.user_id == battleDataForOnline.player1.player_id);
        self     = (is_1p) ? battleDataForOnline.player1 : battleDataForOnline.player2;
        opponent = (is_1p) ? battleDataForOnline.player2 : battleDataForOnline.player1;
    }
    void Update()
    {
        // 常に最新のデータを画面に反映
        UpdateCharacterUI();
        UpdateBaseUI();
        UpdateCharacterPosition();

        cost[0].text = "cost:" + self.current_cost_remaining;
        cost[1].text = "cost:" + opponent.current_cost_remaining;
        // 元はこっちだったけどどちらにする？
        // cost[0].text = battleDataForOnline.now_my_cost.ToString();
        // cost[1].text = battleDataForOnline.now_enemy_cost.ToString();
    }

    public void SubscribeToEvents()
    {
        if (characterManager != null)
        {
            // 重複購読を防ぐため一旦解除
            characterManager.OnAttackExecuted -= HandleAttackExecuted;
            characterManager.OnAttackExecuted += HandleAttackExecuted;
            Debug.Log("<color=green>[BattleViewManager] OnAttackExecuted サブスクライブ完了</color>");
        }
    }


    public void InitUI()
    {
        Debug.Log("[BattleViewManager] InitUI started.");

        // 初期化処理
        for(int i = 0; i <= 5; i++)
        {
            CharactersBattleData chara = DecideCharacter(i);
            // スライダーの最大値を設定
            hpSlider[i].maxValue = characterData.characters[chara.unique_id].default_hp;            
        }
        // 現在の値をスライダーとテキストに反映
        UpdateCharacterUI();

        playerNames[0].text = self.player_name;
        playerNames[1].text = opponent.player_name;
        cost[0].text = "cost:" + self.current_cost_remaining;
        cost[1].text = "cost:" + opponent.current_cost_remaining;

        // 拠点の初期化 (MaxHP=200)
        if (myBaseSlider != null) myBaseSlider.maxValue = 200;
        if (enemyBaseSlider != null) enemyBaseSlider.maxValue = 200;
        UpdateBaseUI();
        
        if (shadow != null) shadow.gameObject.SetActive(false);
        if (characterStates != null) characterStates.gameObject.SetActive(false);
        if (backfromStates != null) backfromStates.gameObject.SetActive(false);
        
        Debug.Log("[BattleViewManager] InitUI finished successfully.");
    }

    void OnDestroy()
    {
        if (characterManager != null)
        {
            characterManager.OnAttackExecuted -= HandleAttackExecuted;
        }
    }


    private T GetSo<T>(T existing) where T : ScriptableObject
    {
        if (existing != null) return existing;
        var targets = Resources.FindObjectsOfTypeAll<T>();
        if (targets.Length > 0) return targets[0];
        return null;
    }
    // 初期設定時に自キャラと相手キャラのスプライトを反映する関数
    public void SetupCharacters(Google.Protobuf.Collections.RepeatedField<Game.Network.UniqueCharacter> uniqueCharacters)
    {
        int my_index = 0;
        int opponent_index = 3;
        for (int i = 0; i <= 5; i++)
        {
            if (uniqueCharacters[i].Is1P == is_1p)// uniqueCharacter[i]が自分のキャラかどうか。観戦者なら || (Is1P && !is_player)などとすれば良さそう
            {
                characters[my_index].GetComponent<Image>().sprite = characterData.characters[uniqueCharacters[i].CharacterId].default_sprite_mini;
                characterSmallwindow[my_index].sprite = characterData.characters[uniqueCharacters[i].CharacterId].default_sprite_smallwindow;
                my_index ++;
            }
            else// uniqueCharacter[i]が相手のキャラ
            {
                characters[opponent_index].GetComponent<Image>().sprite = characterData.characters[uniqueCharacters[i].CharacterId].default_sprite_mini;
                characterSmallwindow[opponent_index].sprite = characterData.characters[uniqueCharacters[i].CharacterId].default_sprite_smallwindow;
                opponent_index ++;
            }
        }
    }

    public void UpdateCharacterPosition()
    {
        for (int i = 0; i <= 5; i++)
        {
            CharactersBattleData chara = DecideCharacter(i);
            int worldPosX = chara.now_character_position.x * 50 - 175;
            int worldPosY = chara.now_character_position.y * -50 + 60;
            characters[i].transform.position = new Vector3Int(worldPosX, worldPosY);
        }
    }

    public void UpdateBaseUI()
    {
        if (myBaseSlider != null) myBaseSlider.value = self.base_hp;
        if (enemyBaseSlider != null) enemyBaseSlider.value = opponent.base_hp;
        
        if (myBaseHPText != null) myBaseHPText.text = self.base_hp + "/200";
        if (enemyBaseHPText != null) enemyBaseHPText.text = opponent.base_hp + "/200";
    }

    // キャラクターUIを更新するメソッド
    public void UpdateCharacterUI()
    {
        for(int i = 0; i <= 5; i++)
        {
            CharactersBattleData chara = DecideCharacter(i);
            // スライダーの現在値を更新（maxhpではなくhpを入れる）
            hpSlider[i].value = chara.now_character_hp;

            // テキストの更新
            CharacterHP[i].text = chara.now_character_hp + "/" + characterData.characters[chara.unique_id].default_hp;
        }
    }

    // 1..5の通し番号になっているオブジェクトについて、1..2: 自キャラ、3..5: 相手キャラを取得するための補助関数。
    private CharactersBattleData DecideCharacter(int i)
    {
        if(i <= 2)// 自キャラ
        {
            return self.characters[i];
        }
        else// 相手キャラ
        {
            return opponent.characters[i-3];
        }
    }

    public void OnButtonClick(string buttonName)
    {
        switch(buttonName)
        {
            case "smallwindow1":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[self.characters[0].unique_id];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow2":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[self.characters[1].unique_id];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow3":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[self.characters[2].unique_id];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow4":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[opponent.characters[0].unique_id];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow5":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[opponent.characters[1].unique_id];
            shadow.gameObject.SetActive(true);
            characterStates.gameObject.SetActive(true);
            backfromStates.gameObject.SetActive(true);
                break;
            case "smallwindow6":
            SEManager.instance?.PlaySelectSE();
            characterStates.sprite = characterStatesImage[opponent.characters[2].unique_id];
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
        Debug.Log($"<color=cyan>[BattleViewManager] HandleAttackExecuted 受信</color>: Attacker={data.attackerUniqueId}, Target={data.targetUniqueId}, Damage={data.finalDamage}");
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
        for (int i = 0; i <= 5; i++)
        {
            CharactersBattleData chara = DecideCharacter(i);
            if (chara.unique_id == uniqueId)
            {
                if (uniqueId >= 0 && uniqueId < characterData.characters.Length)
                {
                    return characterData.characters[uniqueId].default_name_japanese;
                }
            }
        }
        return "不明";
    }

    private void AddLog(string message)
    {
        Debug.Log($"<color=cyan>[BattleViewManager] AddLog</color>: {message}");
        if (logText != null)
        {
            logText.text = message;

            // 3秒後に消去するコルーチンを開始
            if (_clearLogCoroutine != null)
            {
                StopCoroutine(_clearLogCoroutine);
            }
            _clearLogCoroutine = StartCoroutine(ClearLogAfterDelay(3f));
        }
        else
        {
            Debug.LogWarning("<color=red>[BattleViewManager] logText (TextMeshProUGUI) がアサインされていません！</color>");
        }
    }

    private IEnumerator ClearLogAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (logText != null)
        {
            logText.text = "";
        }
        _clearLogCoroutine = null;
    }
}
