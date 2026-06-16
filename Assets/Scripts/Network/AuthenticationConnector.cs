using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Game.Network;

public class AuthenticationConnector : MonoBehaviour
{
    private NetworkClientCore _core;
    private UserService.UserServiceClient _userClient;
    private UserData _userData;

    public void Initialize(NetworkClientCore core, UserData userData)
    {
        _core = core;
        _userData = userData;
        _userClient = new UserService.UserServiceClient(_core.Channel);
    }

    public async Task<UserResponse> SignUp(string userName, string password)
    {
        try
        {
            var request = new CreateUserRequest { Name = userName, Password = password };
            var response = await _userClient.CreateUserAsync(request);

            Debug.Log($"SignUp Success: UserID={response.Id}, Name={response.Name}");

            if (_userData != null)
            {
                _userData.user_id = response.Id;
                _userData.user_name = response.Name;
                _userData.password = password;
                _userData.story_progress = response.Story;
                _userData.num_wins = response.NumWins;
                _userData.num_battles = response.NumBattles;
                _userData.rate = response.Rate;
            }
            return response;
        }
        catch (RpcException e)
        {
            string errorMessage = e.StatusCode switch
            {
                StatusCode.AlreadyExists => "そのユーザー名は既に使用されています。",
                StatusCode.InvalidArgument => "ユーザー名を入力してください",
                StatusCode.OutOfRange => "ユーザー名は16字以内で入力してください",
                StatusCode.FailedPrecondition => "パスワードは6文字以上で入力してください",
                _ => $"登録に失敗しました: {e.Status.Detail}"
            };
            _core.ShowErrorMessage(errorMessage);
            return null;
        }
    }

    public async Task<UserResponse> Login(string userName, string password)
    {
        try
        {
            var request = new LoginRequest { Name = userName, Password = password };
            var response = await _userClient.LoginAsync(request);

            if (_userData != null)
            {
                _userData.user_id = response.Id;
                _userData.user_name = response.Name;
                _userData.password = password;
                _userData.story_progress = response.Story;
                _userData.num_wins = response.NumWins;
                _userData.num_battles = response.NumBattles;
                _userData.rate = response.Rate;
                _userData.home_character_id = response.HomeCharacterId;
                _userData.deck1 = response.Deck1;
                _userData.deck2 = response.Deck2;
                _userData.deck3 = response.Deck3;
            }
            return response;
        }
        catch (RpcException e)
        {
            string errorMessage = e.StatusCode switch
            {
                StatusCode.Unauthenticated => "ユーザー名またはパスワードが正しくありません。",
                StatusCode.NotFound => "ユーザーが見つかりませんでした。",
                StatusCode.Unavailable => "サーバーに接続できません。通信環境を確認してください。",
                StatusCode.DeadlineExceeded => "通信がタイムアウトしました。",
                _ => $"予期せぬエラーが発生しました: {e.Status.Detail}"
            };
            _core.ShowErrorMessage(errorMessage);
            return null;
        }
    }

    public async Task<List<UserResponse>> GetAllUsers()
    {
        try
        {
            var request = new ListUsersRequest();
            var response = await _userClient.ListUsersAsync(request);
            return new List<UserResponse>(response.Users);
        }
        catch (Exception e)
        {
            Debug.LogError($"ListUsers Error: {e.Message}");
            return null;
        }
    }

    public async Task<UserResponse> GetUser(string userId)
    {
        try
        {
            var request = new GetUserRequest { Id = userId };
            return await _userClient.GetUserAsync(request);
        }
        catch (Exception e)
        {
            Debug.LogError($"GetUser Error: {e.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteUser(string userId = "")
    {
        try
        {
            string targetId = string.IsNullOrEmpty(userId) ? PlayerPrefs.GetString("USER_ID", "") : userId;
            if (string.IsNullOrEmpty(targetId)) return false;

            var request = new DeleteUserRequest { Id = targetId };
            var response = await _userClient.DeleteUserAsync(request);

            if (response.Success)
            {
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

    public async Task<UserResponse> UpdateUser()
    {
        try
        {
            var request = new UpdateUserRequest
            {
                Id = _userData.user_id,
                Name = _userData.user_name,
                Password = _userData.password,
                Story = _userData.story_progress,
                NumWins = _userData.num_wins,
                NumBattles = _userData.num_battles,
                Rate = _userData.rate,
                HomeCharacterId = _userData.home_character_id,
                Deck1 = _userData.deck1,
                Deck2 = _userData.deck2,
                Deck3 = _userData.deck3,
            };
            return await _userClient.UpdateUserAsync(request);
        }
        catch (Exception e)
        {
            Debug.LogError($"UpdateUser Error: {e.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateStory()
    {
        _userData.story_progress += 1;
        try
        {
            var request = new UpdateUserRequest
            {
                Id = _userData.user_id,
                Name = _userData.user_name,
                Password = _userData.password,
                Story = _userData.story_progress,
                NumWins = _userData.num_wins,
                NumBattles = _userData.num_battles,
                Rate = _userData.rate,
                HomeCharacterId = _userData.home_character_id,
                Deck1 = _userData.deck1,
                Deck2 = _userData.deck2,
                Deck3 = _userData.deck3,
            };
            var response = await _userClient.UpdateUserAsync(request);
            return response != null;
        }
        catch (Exception e) {
            Debug.LogError($"UpdateStory Error: {e.Message}");
            return false;
        }
    }
}