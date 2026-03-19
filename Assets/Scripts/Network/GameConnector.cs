using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Game.Network;
using Roommatch;
using Room;

public class GameConnector : MonoBehaviour
{
    private const string ServerUrl = "https://auxilia.trap.show/";
    private UserService.UserServiceClient _userClient;
    private RoomMatchService.RoomMatchServiceClient _roomMatchClient;
    private RoomService.RoomServiceClient _roomClient;
    private BattleService.BattleServiceClient _battleClient;


    // 通信エラーやサーバーからのメッセージを UI に渡すためのイベント
    public event Action<string> OnErrorMessage;

    // internal helper: ログ出力も兼ねてイベントを発火
    private void ShowErrorMessage(string message)
    {
        Debug.LogError($"GameConnector error: {message}");
        OnErrorMessage?.Invoke(message);
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        var handler = new GrpcWebHandler(new System.Net.Http.HttpClientHandler());
        var channel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        _userClient = new UserService.UserServiceClient(channel);
        _roomMatchClient = new RoomMatchService.RoomMatchServiceClient(channel);
        _roomClient = new RoomService.RoomServiceClient(channel);
        _battleClient = new BattleService.BattleServiceClient(channel);
    }

    public async Task<UserResponse> SignUp(string userName, string password)
    {
        try
        {
            var request = new CreateUserRequest { Name = userName, Password = password };
            var response = await _userClient.CreateUserAsync(request);

            PlayerPrefs.SetString("USER_ID", response.Id);
            PlayerPrefs.Save();

            Debug.Log($"SignUp Success: UserID={response.Id}, Name={response.Name}");
            return response;
        }
        catch (RpcException e)
        {
            string errorMessage = "";
            switch (e.StatusCode)
            {
                case StatusCode.AlreadyExists:
                    errorMessage = "そのユーザー名は既に使用されています。";
                    break;
                case StatusCode.InvalidArgument:
                    errorMessage = "ユーザー名を入力してください";
                    break;
                case StatusCode.OutOfRange:
                    errorMessage = "ユーザー名は16字以内で入力してください";
                    break;
                case StatusCode.FailedPrecondition:
                    errorMessage = "パスワードは6文字以上で入力してください";
                    break;
                default:
                    errorMessage = $"登録に失敗しました: {e.Status.Detail}";
                    break;
            }
            ShowErrorMessage(errorMessage);
            return null;
        }
    }
    public async Task<UserResponse> Login(string userName, string password)
    {
        try
        {
            var request = new LoginRequest { Name = userName, Password = password };
            var response = await _userClient.LoginAsync(request);

            // 成功時の処理
            PlayerPrefs.SetString("USER_ID", response.Id);
            PlayerPrefs.Save();
            return response;
        }
        catch (RpcException e)
        {
            // ステータスコードに応じたメッセージの出し分け
            string errorMessage = "";

            switch (e.StatusCode)
            {
                case StatusCode.Unauthenticated:
                    errorMessage = "ユーザー名またはパスワードが正しくありません。";
                    break;
                case StatusCode.NotFound:
                    errorMessage = "ユーザーが見つかりませんでした。";
                    break;
                case StatusCode.Unavailable:
                    errorMessage = "サーバーに接続できません。通信環境を確認してください。";
                    break;
                case StatusCode.DeadlineExceeded:
                    errorMessage = "通信がタイムアウトしました。";
                    break;
                default:
                    errorMessage = "予期せぬエラーが発生しました: " + e.Status.Detail;
                    break;
            }

            // ここでUI（テキストなど）にメッセージをセットする
            ShowErrorMessage(errorMessage);
            
            Debug.LogError($"Login failed: {e.StatusCode} - {e.Message}");
            return null; // 失敗時はnullを返して、呼び出し側で遷移を止める
        }
    }
    public async Task<List<UserResponse>> GetAllUsers()
    {
        try
        {
            var request = new ListUsersRequest();
            var response = await _userClient.ListUsersAsync(request);

            Debug.Log($"<color=green>ユーザー数:</color> {response.Users.Count}");
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
            var request = new GetUserRequest{Id = userId};
            var response = await _userClient.GetUserAsync(request);
            return response;
        }
        catch (Exception e)
        {
            Debug.LogError($"GetUser Error: {e.Message}");
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
            var response = await _userClient.DeleteUserAsync(request);

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

    public async Task<RoomMatch> CreateRoomMatch(string roomName, string ownerId, bool isGaming)
    {
        try
        {
            var request = new CreateRoomMatchRequest 
            { 
                RoomName = roomName, 
                OwnerId = ownerId, 
                IsGaming = isGaming 
            };

            // サーバーへ送信
            var response = await _roomMatchClient.CreateRoomMatchAsync(request);

            // response.Room が実データを持っている構造
            Debug.Log($"<color=cyan>Room Created:</color> ID={response.Room.RoomId}, Name={response.Room.RoomName}");
            return response.Room;
        }
        catch (RpcException e)
        {
            string errorMessage = e.StatusCode switch
            {
                StatusCode.InvalidArgument => "部屋名が正しくありません（10文字以内）。",
                StatusCode.Internal => "サーバーエラーで部屋を作成できませんでした。",
                _ => $"部屋作成エラー: {e.Status.Detail}"
            };
            ShowErrorMessage(errorMessage);
            return null;
        }
    }

    public async Task<List<RoomMatch>> GetAllRoomMatch()
    {
        try
        {
            var request = new ListRoomMatchRequest();
            var response = await _roomMatchClient.ListRoomMatchAsync(request);

            Debug.Log($"<color=green>部屋一覧取得成功:</color> {response.Rooms.Count}件");
            
            // repeated フィールドは Google.Protobuf.Collections.RepeatedField として返るため
            // List に変換して返すと扱いやすいです
            return new List<RoomMatch>(response.Rooms);
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"部屋リストの取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async Task<JoinRoomResponse> JoinRoom(int roomId, string userId)
    {
        try
        {
            var request = new JoinRoomRequest { RoomId = roomId, UserId = userId };
            
            // 修正点: メソッド名を JoinRoomAsync に変更
            var response = await _roomClient.JoinRoomAsync(request);

            Debug.Log($"<color=green>部屋参加成功:</color> RoomID={roomId}, UserID={userId}");
            return response;
        }
        catch (RpcException e)
        {
            string errorMessage = e.StatusCode switch
            {
                StatusCode.NotFound => "指定された部屋が見つかりませんでした。",
                StatusCode.Internal => "サーバーエラーで部屋に参加できませんでした。",
                _ => $"部屋参加エラー: {e.Status.Detail}"
            };
            ShowErrorMessage(errorMessage);
            return null;
        }
    }

    public async Task<GameDataResponse> GetGameData(uint roomId)
    {
        try
        {
            var request = new GetGameDataRequest { RoomId = roomId };
            var response = await _battleClient.GetGameDataAsync(request);
            return response;
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"ゲームデータの取得に失敗しました: {e.Status.Detail}");
            return null;

        }
    }

    public async Task<GameDataResponse> CreateGameData(uint roomID,string player1Id,string player2Id)
    {
        try
        {
            var request = new CreateGameRequest { 
                RoomId = roomID, 
                Player1Id = player1Id, 
                Player2Id = player2Id 
            };
            var response = await _battleClient.CreateGameAsync(request);
            return response;
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"ゲームデータの作成に失敗しました: {e.Status.Detail}");
            return null;

        }
    }

    public async Task<EnterRingResponse> EnterRing(int roomId, string userId){
        try
        {
            var request = new EnterRingRequest { RoomId = roomId, UserId = userId };
            var response = await _roomClient.EnterRingAsync(request);
            return response;
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"リング参加に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async Task<List<Room.Room>> ListRoom(int roomId)
    {
        try
        {
            var request = new ListRoomRequest{RoomId = roomId};
            var response = await _roomClient.ListRoomAsync(request);

            Debug.Log($"<color=green>参加者一覧取得成功:</color> {response.Rooms.Count}件");
            // Debug.Log($"response: {response}, Room.Room: {response.Rooms[0]}");
            
            // repeated フィールドは Google.Protobuf.Collections.RepeatedField として返るため
            // List に変換して返すと扱いやすいです
            return new List<Room.Room>(response.Rooms);
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"参加者リストの取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }
}