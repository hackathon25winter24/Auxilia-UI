using UnityEngine;
using TMPro; // TMP���g�����߂ɕK�v
using System.Collections.Generic;
using Game.Network;

public class NetworkUIController : MonoBehaviour
{
    [Header("References")]
    public GameConnector connector;

    [Header("Login UI")]
    public TMP_InputField loginHashInput; // TMP�łɕύX
    public TMP_InputField signUpHashInput; // TMP�łɕύX
    public TextMeshProUGUI loginResultText; // TMP�łɕύX
    public TextMeshProUGUI signUpResultText; // TMP�łɕύX

    [Header("User List UI")]
    public TextMeshProUGUI userListDisplay; // TMP�łɕύX

    public async void OnClick_Login()
    {
        string hash = loginHashInput.text;
        if (string.IsNullOrEmpty(hash)) return;

        loginResultText.text = "A";
        var user = await connector.Login(hash);

        if (user != null)
        {
            loginResultText.text = $"Hash: {user.Hash}\nRate: {user.Rate}";
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
        if (string.IsNullOrEmpty(hash)) return;

        signUpResultText.text = "C";
        var user = await connector.SignUp(hash, 1, 100); // Story=1, Rate=100

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
            foreach (var u in users) res += $"Name: {u.Hash} , Rate:{u.Rate}\n";
            userListDisplay.text = res;
        }
    }

    public async void OnClick_DeleteAllUsers()
    {
        userListDisplay.text = "Deleting...";
        bool success = await connector.DeleteAllUsers();

        if (success)
        {
            userListDisplay.text = "All users deleted.";
            userListDisplay.color = Color.green;
        }
        else
        {
            userListDisplay.text = "Delete failed";
            userListDisplay.color = Color.red;
        }
    }
}