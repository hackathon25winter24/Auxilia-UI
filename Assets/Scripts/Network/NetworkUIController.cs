using UnityEngine;
using TMPro; // TMP���g�����߂ɕK�v
using System.Collections.Generic;
using Game.Network;

public class NetworkUIController : MonoBehaviour
{
    [Header("References")]
    public GameConnector connector;

    [Header("Login UI")]
    public TMP_InputField loginHashInput; 
    public TMP_InputField loginUserNameInput;
    public TMP_InputField signUpHashInput;
    public TMP_InputField signUpUserNameInput;
    public TMP_InputField deleteUserIDInput;
    public TextMeshProUGUI loginResultText; 
    public TextMeshProUGUI signUpResultText; 

    [Header("User List UI")]
    public TextMeshProUGUI userListDisplay; 
    public TextMeshProUGUI deleteResultText;

    public async void OnClick_Login()
    {
        string hash = loginHashInput.text;
        string userName = loginUserNameInput.text;
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(userName)) return;

        loginResultText.text = "Connecting...";
        var user = await connector.Login(userName, hash);

        if (user != null)
        {
            loginResultText.text = $"Name: {user.Name}\nWins: {user.NumWins},\nBattles: {user.NumBattles}";
            loginResultText.color = Color.green;
        }
        else
        {
            loginResultText.text = "Login failed";
            loginResultText.color = Color.red;
        }
    }
    public async void OnClick_SignUp()
    {
        string hash = signUpHashInput.text;
        string userName = signUpUserNameInput.text;
        if (string.IsNullOrEmpty(hash)) return;

        signUpResultText.text = "Connecting...";

        var user = await connector.SignUp(userName, hash); 

        if (user != null)
        {
            signUpResultText.text = $"succeeded";
            signUpResultText.color = Color.green;
        }
        else
        {
            signUpResultText.text = "Sign Up failed";
            signUpResultText.color = Color.red;
        }
    }

    public async void OnClick_FetchAllUsers()
    {
        userListDisplay.text = "Fetching users...";
        List<UserResponse> users = await connector.GetAllUsers();

        if (users != null)
        {
            //string res = "User List:\n";

            string res = "";
            foreach (var u in users)
            {
                res += $"Name: {u.Name} , ID: {u.Id}, Wins: {u.NumWins}, Battles: {u.NumBattles}\n";
                Debug.Log($"Name: {u.Name} ,\nID: {u.Id},\nWins: {u.NumWins},\nBattles: {u.NumBattles}\n");
            }
            userListDisplay.text = res;
        }
    }

    public async void OnClick_DeleteUser()
    {
        bool success = await connector.DeleteUser(deleteUserIDInput.text);
        if (success)
        {
            deleteUserIDInput.text = "";
            deleteResultText.text = "User deleted successfully.";
        }   
        else
        {
            deleteResultText.text = "Failed to delete user.";
        }

    }
}