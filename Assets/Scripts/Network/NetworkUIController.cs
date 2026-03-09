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
    public TMP_InputField signUpHashInput;
    public TMP_InputField deleteUserIDInput;
    public TextMeshProUGUI loginResultText; 
    public TextMeshProUGUI signUpResultText; 

    [Header("User List UI")]
    public TextMeshProUGUI userListDisplay; 
    public TextMeshProUGUI deleteResultText;

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
            foreach (var u in users)
            {
                res += $"Name: {u.Hash} , ID: {u.Id}, Rate: {u.Rate}\n";
                Debug.Log($"Name: {u.Hash} ,\nID: {u.Id},\nRate: {u.Rate}\n");
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