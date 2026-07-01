using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class TitleUIManager : MonoBehaviour
{
    public UserData userData;
    public SceneData sceneData;
    public StoryManagerData storyManagerData;
    public GameObject login_ui;
    public GameObject signup_ui;
    public GameObject error_ui;
    public GameObject targetUI;
    public GameObject now_loading_ui;
    public TMP_InputField user_name_for_login;
    public TMP_InputField password_for_login;
    public TMP_InputField user_name_for_signup;
    public TMP_InputField password_for_signup;
    private string latestErrorMessage = "";

    void Awake()
    {
        error_ui.SetActive(false);
        now_loading_ui.SetActive(false);
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnErrorMessage += HandleConnectorError;
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnErrorMessage -= HandleConnectorError;
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
            SEManager.instance?.PlayToNextSE();
            userData.user_name = user_name_for_login.text;
            userData.password = password_for_login.text;
            var user = await NetworkManager.Instance.Auth.Login(userData.user_name, userData.password);
            if(user == null)
            {
                error_ui.SetActive(true);
                await Task.Delay(1000);
                error_ui.SetActive(false);
                return;
            }
            if(userData.story_progress == 1)
            {
                login_ui.SetActive(false);
                now_loading_ui.SetActive(true);
                storyManagerData.now_story_number = 0;
                sceneData.next_scene_number = 8;
            }else
            {
                login_ui.SetActive(false);
                now_loading_ui.SetActive(true);
                sceneData.next_scene_number = 1;
            }
                break;
            case "Signup":
            SEManager.instance?.PlayToNextSE();
            userData.user_name = user_name_for_signup.text;
            userData.password = password_for_signup.text;
            var new_user = await NetworkManager.Instance.Auth.SignUp(userData.user_name, userData.password);
            if(new_user == null)
            {
                error_ui.SetActive(true);
                await Task.Delay(1000);
                error_ui.SetActive(false);
                return;
            }
            if(userData.story_progress == 1)
            {
                signup_ui.SetActive(false);
                now_loading_ui.SetActive(true);
                storyManagerData.now_story_number = 0;
                sceneData.next_scene_number = 8;
            }else
            {
                signup_ui.SetActive(false);
                now_loading_ui.SetActive(true);
                sceneData.next_scene_number = 1;
            }
                break;
            case "GotoSignup":
            SEManager.instance?.PlaySelectSE();
            login_ui.SetActive(false);
            signup_ui.SetActive(true);
                break;
            case "GotoLogin":
            SEManager.instance?.PlaySelectSE();
            login_ui.SetActive(true);
            signup_ui.SetActive(false);
                break;
            case "Back":
            SEManager.instance?.PlayBackSE();
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