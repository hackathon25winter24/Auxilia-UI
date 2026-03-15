using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class TitleUIManager : MonoBehaviour
{
    public PlayerData playerData;
    public SceneData sceneData;
    public GameConnector gameConnector;
    public GameObject login_ui;
    public GameObject signup_ui;
    public GameObject error_ui;
    public GameObject targetUI;
    public TMP_InputField user_name_for_login;
    public TMP_InputField password_for_login;
    public TMP_InputField user_name_for_signup;
    public TMP_InputField password_for_signup;

    void Awake()
    {
        error_ui.SetActive(false);
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
            sceneData.next_scene_number = 1;
                break;
            case "Signup":
            playerData.username = user_name_for_signup.text;
            playerData.password = password_for_signup.text;
            var new_user = await gameConnector.SignUp(playerData.username, playerData.password);
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