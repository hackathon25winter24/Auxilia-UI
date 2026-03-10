using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Game.Network;

public class GameConnector : MonoBehaviour
{
    private const string ServerUrl = "https://auxilia.trap.show/";
    private UserService.UserServiceClient _client;

    void Awake()
    {
        var handler = new GrpcWebHandler(new System.Net.Http.HttpClientHandler());
        var channel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        _client = new UserService.UserServiceClient(channel);
    }

    public async Task<UserResponse> SignUp(string userName, string password)
    {
        try
        {
            // 1. リクエストの作成
            // proto側でフィールド名を Password に変更している場合はそれを使います
            var request = new CreateUserRequest 
            { 
                Name = userName, 
                Password = password // 生のパスワードを送信（サーバー側でハッシュ化される）
            };

            // 2. サーバーへ送信
            var response = await _client.CreateUserAsync(request);

            // 3. サーバーから返ってきた一意の ID (UUID) をローカルに保存
            // 今後はこの ID を使ってログイン(LoginRequest.Id)することになります
            PlayerPrefs.SetString("USER_ID", response.Id);
            PlayerPrefs.Save();

            Debug.Log($"SignUp Success: UserID={response.Id}, Name={response.Name}");
            return response;
        }
        catch (Exception e)
        {
            Debug.LogError($"SignUp Error: {e.Message}");
            return null;
        }
    }

    public async Task<UserResponse> Login(string userName, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Name = userName,
                Password = password
            };

            var response = await _client.LoginAsync(request);
            
            Debug.Log($"ログイン成功: {response.Name}");
            PlayerPrefs.SetString("USER_ID", response.Id);
            PlayerPrefs.Save();
            return response;
        }
        catch (RpcException e)
        {
            Debug.LogError($"ログイン失敗: {e.Status.Detail}");
            // e.StatusCode が Unauthenticated なら「パスワード間違い」
            throw;
        }
    }

    public async Task<List<UserResponse>> GetAllUsers()
    {
        try
        {
            var request = new ListUsersRequest();
            var response = await _client.ListUsersAsync(request);

            Debug.Log($"<color=green>ユーザー数:</color> {response.Users.Count}");
            return new List<UserResponse>(response.Users);
        }
        catch (Exception e)
        {
            Debug.LogError($"ListUsers Error: {e.Message}");
            return null;
        }
    }

    // --- 4. ユーザー削除 (DeleteUser) ---
    /// <param name="userId">削除したいユーザーのUUID（未指定の場合は保存されている自分のIDを削除）</param>
    public async Task<bool> DeleteUser(string userId = "")
    {
        try
        {
            // 引数が空なら、PlayerPrefsに保存されている自分のIDを取得
            string targetId = string.IsNullOrEmpty(userId) ? PlayerPrefs.GetString("USER_ID", "") : userId;

            if (string.IsNullOrEmpty(targetId))
            {
                Debug.LogWarning("DeleteUser: Target ID is empty.");
                return false;
            }

            // DeleteUserRequest を作成
            var request = new DeleteUserRequest { Id = targetId };
            
            // サーバーに削除リクエストを送信
            var response = await _client.DeleteUserAsync(request);

            if (response.Success)
            {
                Debug.Log($"<color=red>ユーザー削除成功:</color> {targetId}");
                
                // もし自分自身のIDを消したなら、PlayerPrefsもクリアする
                if (targetId == PlayerPrefs.GetString("USER_ID", ""))
                {
                    PlayerPrefs.DeleteKey("USER_ID");
                    PlayerPrefs.Save();
                }
                return true;
            }
            
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"DeleteUser Error: {e.Message}");
            return false;
        }
    }
}