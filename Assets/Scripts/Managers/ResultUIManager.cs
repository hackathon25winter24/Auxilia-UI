using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ResultUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    public BattleDataforOmline battleDataforOnline;

    public TextMeshProUGUI playerName;
    public TextMeshProUGUI myNewRate;
    public TextMeshProUGUI MyRateUpDown;
    public TextMeshProUGUI NewRate;
    public TextMeshProUGUI RateUpDown;

    public Image[] characters;
    public Sprite[] charactersFaceImage;
    public Sprite[] charactersResult;
    public Image result;
    public Sprite[] resultImage;
    public Image[] UpDown;
    public Sprite[] UpDownImage;

    void Awake()
    {
        InitializeFields();

        // 相手の名前を表示
        if (playerName != null) playerName.text = battleDataforOnline.opponent_name;
        
        // 自分のキャラクター表示（リザルト大画像を使用）
        if (battleDataforOnline.selected_character != null && battleDataforOnline.selected_character.Length >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < characters.Length)
                {
                    int charId = battleDataforOnline.selected_character[i];
                    if (charId >= 0 && charId < charactersResult.Length)
                    {
                        characters[i].sprite = charactersResult[charId];
                    }
                }
            }
        }

        // 敵（相手）のキャラクターアイコン表示（アイコン画像を使用）
        if (battleDataforOnline.selected_character != null && battleDataforOnline.selected_character.Length >= 6)
        {
            for (int i = 0; i < 3; i++)
            {
                int imgIdx = i + 3; // characters配列の3番目からが敵用と想定
                if (imgIdx < characters.Length)
                {
                    int charId = battleDataforOnline.selected_character[imgIdx];
                    if (charId >= 0 && charId < charactersFaceImage.Length)
                    {
                        characters[imgIdx].sprite = charactersFaceImage[charId];
                    }
                }
            }
        }

        // 勝敗ログ
        Debug.Log($"<color=yellow>[ResultUI] my_id={battleDataforOnline.my_player_id}, win_id={battleDataforOnline.win_player_id}</color>");

        // 勝敗に応じた素材切り替え
        if (playerData.user_id == battleDataforOnline.win_player_id)
        {
            // 勝利
            SEManager.instance?.PlayWinSE();
            if (resultImage.Length > 0) result.sprite = resultImage[0];
        }else
        {
            // 敗北
            SEManager.instance?.PlayDefeatSE();
            if (resultImage.Length > 1) result.sprite = resultImage[1];
        }
    }

    public RoomData roomData;
    public GameConnector gameConnector;

    void Start()
    {
        InitializeFields();

        if (battleDataforOnline == null) return;

        if (battleDataforOnline.my_rate_updown > 0)
        {
            myNewRate.text = battleDataforOnline.rate.ToString();
            MyRateUpDown.text = "+" + battleDataforOnline.my_rate_updown.ToString();
            MyRateUpDown.color = new Color32(255, 122, 122, 255);
            UpDown[0].sprite = UpDownImage[0];
        }else if (battleDataforOnline.my_rate_updown < 0) 
        {
            myNewRate.text = battleDataforOnline.rate.ToString();
            MyRateUpDown.text = battleDataforOnline.my_rate_updown.ToString();
            MyRateUpDown.color = new Color32(122, 122, 255, 255);
            UpDown[0].sprite = UpDownImage[1];
        }else
        {
            myNewRate.text = battleDataforOnline.rate.ToString();
            MyRateUpDown.text = "+0";
            MyRateUpDown.color = new Color32(255, 255, 255, 255);
            UpDown[0].sprite = UpDownImage[0];
        }

        if (battleDataforOnline.opponent_rate_updown > 0)
        {
            NewRate.text = battleDataforOnline.opponent_rate.ToString();
            RateUpDown.text = "+" + battleDataforOnline.opponent_rate_updown.ToString();
            RateUpDown.color = new Color32(255, 122, 122, 255);
            UpDown[1].sprite = UpDownImage[0];
        }else if (battleDataforOnline.opponent_rate_updown < 0) 
        {
            NewRate.text = battleDataforOnline.opponent_rate.ToString();
            RateUpDown.text = battleDataforOnline.opponent_rate_updown.ToString();
            RateUpDown.color = new Color32(122, 122, 255, 255);
            UpDown[1].sprite = UpDownImage[1];
        }else
        {
            NewRate.text = battleDataforOnline.opponent_rate.ToString();
            RateUpDown.text = "+0";
            RateUpDown.color = new Color32(255, 255, 255, 255);
            UpDown[1].sprite = UpDownImage[0];
        }
    }

    public void OnButtonClick(string buttonName)
    {
        // Unityイベントから非同期メソッドを呼ぶ
        _ = HandleButtonClick(buttonName);
    }

    private async System.Threading.Tasks.Task HandleButtonClick(string buttonName)
    {
        // 念のためバトルストリームを停止
        if (gameConnector != null) await gameConnector.StopStream();

        switch (buttonName)
        {
            case "BackToHome":
                SEManager.instance?.PlayToNextSE();
                if (gameConnector != null && roomData != null)
                {
                    await gameConnector.LeaveRoom(roomData.room_id, playerData.user_id);
                }
                sceneData.next_scene_number = 1;
                break;

            case "BackToMatchingRoom": // 退出して部屋一覧へ
                SEManager.instance?.PlayToNextSE();
                if (gameConnector != null && roomData != null)
                {
                    await gameConnector.LeaveRoom(roomData.room_id, playerData.user_id);
                }
                sceneData.next_scene_number = 9;
                break;

            case "Rematch": // 新設ボタン名: 再戦
                SEManager.instance?.PlayToNextSE();
                if (gameConnector != null && roomData != null)
                {
                    // 部屋に留まったまま Ready を false に更新
                    await gameConnector.UpdateRoomState(roomData.room_id, playerData.user_id, battleDataforOnline.my_player_id, false);
                }
                sceneData.next_scene_number = 10; // 待機室 (MatchingRoom) へ戻る
                break;

            case "BackToMatching":
                SEManager.instance?.PlayToNextSE();
                sceneData.next_scene_number = 3;
                break;

            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }

    private void InitializeFields()
    {
        sceneData = GetSO(sceneData);
        playerData = GetSO(playerData);
        battleDataforOnline = GetSO(battleDataforOnline);
        roomData = GetSO(roomData);

        if (gameConnector == null)
        {
            gameConnector = FindFirstObjectByType<GameConnector>();
        }
    }

    private T GetSO<T>(T existing) where T : ScriptableObject
    {
        if (existing != null) return existing;
        T[] assets = Resources.FindObjectsOfTypeAll<T>();
        if (assets.Length > 0) return assets[0];
        return null;
    }
}
