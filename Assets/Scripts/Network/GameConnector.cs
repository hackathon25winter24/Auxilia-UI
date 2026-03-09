using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Game.Network; // ���Ȃ���Namespace

public class GameConnector : MonoBehaviour
{
    // NeoShowcase�̌��JURL (�����̃X���b�V���Ȃ�)
    private const string ServerUrl = "https://auxilia.trap.show/";
    private UserService.UserServiceClient _client;

    void Awake()
    {
        // HTTPS�o�R��gRPC-Web�ʐM���s�����߂̐ݒ�
        var handler = new GrpcWebHandler(new System.Net.Http.HttpClientHandler());
        var channel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        _client = new UserService.UserServiceClient(channel);
    }

    // --- 1. �V�K�o�^ (SignUp) ---
    public async Task<UserResponse> SignUp(string userHash, int story, int rate)
    {
        try
        {
            var request = new CreateUserRequest { Hash = userHash, Story = story, Rate = rate };
            var response = await _client.CreateUserAsync(request);

            // ����������ID��ۑ�
            PlayerPrefs.SetString("USER_ID", response.Id);
            PlayerPrefs.Save();

            //Debug.Log($"<color=yellow>�V�K�o�^����:</color> {response.Id}");
            return response;
        }
        catch (Exception e)
        {
            Debug.LogError($"SignUp Error: {e.Message}");
            return null;
        }
    }

    // --- 2. ���O�C�� (Login) ---
    public async Task<UserResponse> Login(string userHash)
    {
        try
        {
            // LoginRequest���쐬���đ��M
            var request = new LoginRequest { Hash = userHash };
            var response = await _client.LoginAsync(request);

            // ���O�C�������������[�U�[��ID��ۑ�
            PlayerPrefs.SetString("USER_ID", response.Id);
            PlayerPrefs.Save();

            Debug.Log($"<color=cyan>Hash:</color> {response.Hash} (Rate: {response.Rate})");
            return response;
        }
        catch (Exception e)
        {
            Debug.LogError($"Login Error: {e.Message}");
            return null;
        }
    }

    // --- 3. �S���[�U�[�ꗗ�擾 (GetAllUsers) ---
    public async Task<List<UserResponse>> GetAllUsers()
    {
        try
        {
            // ListUsersRequest�𑗐M (���g�͋�)
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