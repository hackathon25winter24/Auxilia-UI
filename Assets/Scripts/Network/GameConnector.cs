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

    // --- 4. ���񂾂s���g�� (DeleteAllUsers) ---
    /// <summary>
    /// Calls a RPC to delete every user in the database.  
    /// The server must implement a unary method "DeleteAllUsers" taking and returning
    /// google.protobuf.Empty; otherwise this will throw an RpcException.
    /// </summary>
    public async Task<bool> DeleteAllUsers()
    {
        try
        {
            var request = new Google.Protobuf.WellKnownTypes.Empty();

            // manually construct a Method descriptor in case the generated client
            // doesn't include the RPC (proto may not yet have been updated).
            var method = new Grpc.Core.Method<Google.Protobuf.WellKnownTypes.Empty, Google.Protobuf.WellKnownTypes.Empty>(
                Grpc.Core.MethodType.Unary,
                "user.UserService",
                "DeleteAllUsers",
                Grpc.Core.Marshallers.Create(
                    arg => arg.ToByteArray(),
                    Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom),
                Grpc.Core.Marshallers.Create(
                    arg => arg.ToByteArray(),
                    Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom)
            );

            var response = await _client.CallInvoker.AsyncUnaryCall(method, null, new Grpc.Core.CallOptions(), request);
            Debug.Log("<color=red>DeleteAllUsers succeeded</color>");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"DeleteAllUsers Error: {e.Message}");
            return false;
        }
    }
}