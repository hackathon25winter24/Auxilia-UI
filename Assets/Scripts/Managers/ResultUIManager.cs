using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ResultUIManager : MonoBehaviour
{
    public UserData userData;
    public RoomData roomData;
    public BattleDataForOnline battleDataForOnline;

    public PlayerState self;
    public PlayerState opponent;

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

    private NetworkManager Net => NetworkManager.Instance;
    private BattleConnector battleConnector => Net?.Battle;

    void Awake()
    {
        InitializeFields();
        bool is_1p = (userData.user_id == battleDataForOnline.player1.player_id);
        self     = (is_1p) ? battleDataForOnline.player1 : battleDataForOnline.player2;
        opponent = (is_1p) ? battleDataForOnline.player2 : battleDataForOnline.player1;

        // 相手の名前を表示
        if (playerName != null) playerName.text = opponent.player_name;
        
        
        if ((self.characters != null && self.characters.Length >= 3) && (opponent.characters != null && opponent.characters.Length >= 3))
        {
            // 自分のキャラクター表示（リザルト大画像を使用）
            for (int i = 0; i <= 2; i++)
            {
                if (i < characters.Length)
                {
                    int charId = self.characters[i].unique_id;
                    if (charId >= 0 && charId < charactersResult.Length)
                    {
                        characters[i].sprite = charactersResult[charId];
                    }
                }
            }

            // 敵（相手）のキャラクターアイコン表示（アイコン画像を使用）
            for (int i = 0; i <= 2; i++)
            {
                if (i+3 < characters.Length)
                {
                    int charId = opponent.characters[i].unique_id;
                    if (charId >= 0 && charId < charactersFaceImage.Length)
                    {
                        characters[i+3].sprite = charactersFaceImage[charId];
                    }
                }
            }
        }


        // 勝敗ログ
        Debug.Log($"<color=yellow>[ResultUI] my_id={self.player_id}, win_id={battleDataForOnline.winner_player_id}</color>");

        // 勝敗に応じた素材切り替え
        if (userData.user_id == battleDataForOnline.winner_player_id)
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

    void Start()
    {
        InitializeFields();

        if (battleDataForOnline == null) return;

        if (self.rate_updown > 0)
        {
            myNewRate.text = self.rate.ToString();
            MyRateUpDown.text = "+" + self.rate_updown.ToString();
            MyRateUpDown.color = new Color32(255, 122, 122, 255);
            UpDown[0].sprite = UpDownImage[0];
        }else if (self.rate_updown < 0) 
        {
            myNewRate.text = self.rate.ToString();
            MyRateUpDown.text = self.rate_updown.ToString();
            MyRateUpDown.color = new Color32(122, 122, 255, 255);
            UpDown[0].sprite = UpDownImage[1];
        }else
        {
            myNewRate.text = self.rate.ToString();
            MyRateUpDown.text = "+0";
            MyRateUpDown.color = new Color32(255, 255, 255, 255);
            UpDown[0].sprite = UpDownImage[0];
        }

        if (opponent.rate_updown > 0)
        {
            NewRate.text = opponent.rate.ToString();
            RateUpDown.text = "+" + opponent.rate_updown.ToString();
            RateUpDown.color = new Color32(255, 122, 122, 255);
            UpDown[1].sprite = UpDownImage[0];
        }else if (opponent.rate_updown < 0) 
        {
            NewRate.text = opponent.rate.ToString();
            RateUpDown.text = opponent.rate_updown.ToString();
            RateUpDown.color = new Color32(122, 122, 255, 255);
            UpDown[1].sprite = UpDownImage[1];
        }else
        {
            NewRate.text = opponent.rate.ToString();
            RateUpDown.text = "+0";
            RateUpDown.color = new Color32(255, 255, 255, 255);
            UpDown[1].sprite = UpDownImage[0];
        }
    }

    private bool navigating;
    public async void OnButtonClick(string buttonName)
    {
        if (navigating) return;
        if (buttonName != "BackToHome" && buttonName != "BackToMatching" &&
            buttonName != "BackToMatchingRoom" && buttonName != "Rematch") return;
        SEManager.instance?.PlayToNextSE();
        navigating = true;
        try
        {
            if (battleConnector != null) await battleConnector.StopStream();
            int id = roomData.room_id;
            var useCase = NetworkManager.Instance.ConfigureRoom(roomData);
            if (roomData.Snapshot == null) await useCase.Resume(checked((uint)id));
            else await useCase.Refresh();
            switch (buttonName)
            {
                case "BackToHome":
                case "BackToMatching":
                    await useCase.Leave();
                    if (this != null) SceneChangeManager.MoveScene(buttonName == "BackToHome" ? 1 : 3);
                    break;
                case "BackToMatchingRoom":
                case "Rematch":
                    // Preserve membership and seats. The next match requires a new room start.
                    if (this != null) SceneChangeManager.MoveScene(9);
                    break;
                default: navigating = false; break;
            }
        }
        catch (System.Exception e)
        {
            navigating = false;
            Debug.LogError($"リザルトからの移動に失敗しました: {e.Message}");
        }
    }
    private void InitializeFields()
    {
        userData = GetSO(userData);
        battleDataForOnline = Net?.Battle?.Data ?? GetSO(battleDataForOnline);
        roomData = GetSO(roomData);
    }

    private T GetSO<T>(T existing) where T : ScriptableObject
    {
        if (existing != null) return existing;
        T[] assets = Resources.FindObjectsOfTypeAll<T>();
        if (assets.Length > 0) return assets[0];
        return null;
    }
}
