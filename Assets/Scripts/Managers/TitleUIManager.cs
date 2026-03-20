using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class TitleUIManager : MonoBehaviour
{
    public PlayerData playerData;
    public SceneData sceneData;
    public GameConnector gameConnector;
    public StoryManagerData storyManagerData;
    public GameObject login_ui;
    public GameObject signup_ui;
    public GameObject error_ui;
    public GameObject targetUI;
    public TMP_InputField user_name_for_login;
    public TMP_InputField password_for_login;
    public TMP_InputField user_name_for_signup;
    public TMP_InputField password_for_signup;
    private string latestErrorMessage = "";

    void Awake()
    {
        error_ui.SetActive(false);
        if (gameConnector != null)
        {
            gameConnector.OnErrorMessage += HandleConnectorError;
        }
    }

    void OnDestroy()
    {
        if (gameConnector != null)
        {
            gameConnector.OnErrorMessage -= HandleConnectorError;
        }
    }

    private void HandleConnectorError(string message)
    {
        latestErrorMessage = message;
        SetErrorMessageText(message);
    }

    private void SetErrorMessageText(string message)
    {
        if (error_ui == null) return;

        var text = error_ui.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            text.text = message;
        }
    }

    public async void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Login":
            playerData.username = user_name_for_login.text;
            playerData.password = password_for_login.text;
            var user = await gameConnector.Login(playerData.username, playerData.password);
            if(user == null)
            {
                error_ui.SetActive(true);
                await Task.Delay(1000);
                error_ui.SetActive(false);
                return;
            }
            playerData.player_name = user.Name;
            playerData.battle_number = user.NumBattles;
            playerData.win_number = user.NumWins;
            playerData.story_progress = user.Story;
            playerData.user_id = user.Id;
            playerData.player_rate = user.Rate;
            if(playerData.story_progress == 1)
            {
                storyManagerData.now_story_number = 0;
                sceneData.next_scene_number = 8;
            }else
            {
                sceneData.next_scene_number = 1;
            }
                break;
            case "Signup":
            playerData.username = user_name_for_signup.text;
            playerData.password = password_for_signup.text;
            var new_user = await gameConnector.SignUp(playerData.username, playerData.password);
            if(new_user == null)
            {
                error_ui.SetActive(true);
                await Task.Delay(1000);
                error_ui.SetActive(false);
                return;
            }
            playerData.player_name = new_user.Name;
            playerData.battle_number = new_user.NumBattles;
            playerData.win_number = new_user.NumWins;
            playerData.story_progress = new_user.Story;
            playerData.user_id = new_user.Id;
            playerData.player_rate = new_user.Rate;
            sceneData.next_scene_number = 1;
                break;
            case "GotoSignup":
            login_ui.SetActive(false);
            signup_ui.SetActive(true);
                break;
            case "GotoLogin":
            login_ui.SetActive(true);
            signup_ui.SetActive(false);
                break;
            case "Back":
            login_ui.SetActive(false);
            signup_ui.SetActive(false);
            targetUI.SetActive(true);
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}