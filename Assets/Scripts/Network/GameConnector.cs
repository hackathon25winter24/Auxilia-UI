using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
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
    private BattleService.BattleServiceClient _battleStreamClient;

    private AsyncServerStreamingCall<GameDataResponse> _call;
    private CancellationTokenSource _cts;
    private bool _isStreamActive;

    public CharacterManager characterManager;
    public PlayerData playerData;

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

        // ストリーム専用の独立チャンネルを作成（並行する単発リクエストで切断されるのを防ぐ）
        var streamHandler = new GrpcWebHandler(new System.Net.Http.HttpClientHandler());
        var streamChannel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
        {
            HttpHandler = streamHandler
        });
        _battleStreamClient = new BattleService.BattleServiceClient(streamChannel);
    }

    public async Task<UserResponse> SignUp(string userName, string password)
    {
        try
        {
            var request = new CreateUserRequest { Name = userName, Password = password };
            var response = await _userClient.CreateUserAsync(request);

            Debug.Log($"SignUp Success: UserID={response.Id}, Name={response.Name}");

            if (playerData != null)
            {
                playerData.player_name = response.Name;
                playerData.battle_number = response.NumBattles;
                playerData.win_number = response.NumWins;
                playerData.story_progress = response.Story;
                playerData.user_id = response.Id;
                playerData.player_rate = response.Rate;
                playerData.password = password;
            }
            else
            {
                Debug.LogWarning("GameConnector: playerData is null. Local persistence of user data will be skipped.");
            }

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
            if (playerData != null)
            {
                playerData.player_name = response.Name;
                playerData.battle_number = response.NumBattles;
                playerData.win_number = response.NumWins;
                playerData.story_progress = response.Story;
                playerData.user_id = response.Id;
                playerData.player_rate = response.Rate;
            }
            else
            {
                Debug.LogWarning("GameConnector: playerData is null in Login. Local persistence skipped.");
            }
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

    public async Task<UserResponse> UpdateUser()
    {
        try
        {
            var request = new UpdateUserRequest
            {
                Id = playerData.user_id,
                Name = playerData.player_name,
                Password = playerData.password,
                Story = playerData.story_progress,
                NumWins = playerData.win_number,
                NumBattles = playerData.battle_number,
                Rate = playerData.player_rate,
                HomeCharacterId = playerData.home_character_ID,
                Deck1 = playerData.character_formation_one,
                Deck2 = playerData.character_formation_two,
                Deck3 = playerData.character_formation_three,
            };
            var response = await _userClient.UpdateUserAsync(request);
            return response;

        }
        catch (Exception e)
        {
            Debug.LogError($"UpdateUser Error: {e.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateStory()
    {
        playerData.story_progress += 1;

        try
        {
            var request = new UpdateUserRequest
            {
                Id = playerData.user_id,
                Name = playerData.player_name,
                Password = playerData.password,
                Story = playerData.story_progress,
                NumWins = playerData.win_number,
                NumBattles = playerData.battle_number,
                Rate = playerData.player_rate,
                HomeCharacterId = playerData.home_character_ID,
                Deck1 = playerData.character_formation_one,
                Deck2 = playerData.character_formation_two,
                Deck3 = playerData.character_formation_three,
            };
            var response = await _userClient.UpdateUserAsync(request);
            if(response != null) return true;
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"UpdateStory Error: {e.Message}");
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

    public async Task<RoomMatch> UpdateRoomMatch(int roomId, string roomName, string ownerId, bool isGaming)
    {
        try
        {
            var request = new UpdateRoomMatchRequest 
            { 
                RoomId = roomId, 
                RoomName = roomName, 
                IsGaming = isGaming,
                OwnerId = ownerId
            };

            // サーバーへ送信
            var response = await _roomMatchClient.UpdateRoomMatchAsync(request);

            // response.Room が実データを持っている構造
            Debug.Log($"<color=cyan>Room Updated:</color> ID={response.Room.RoomId}, Name={response.Room.RoomName}");
            return response.Room;
        }
        catch (RpcException e)
        {
            string errorMessage = e.StatusCode switch
            {
                StatusCode.InvalidArgument => "部屋名が正しくありません（10文字以内）。",
                StatusCode.Internal => "サーバーエラーで部屋を更新できませんでした。",
                _ => $"部屋更新エラー: {e.Status.Detail}"
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

    public async Task<StartMatchResponse> StartMatch(int roomId)
    {
        try
        {
            var request = new StartMatchRequest { RoomId = roomId };
            var response = await _roomClient.StartMatchAsync(request);
            Debug.Log($"<color=green>試合開始成功:</color> RoomID={roomId}");
            return response;
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"試合開始に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public AsyncServerStreamingCall<ListRoomResponse> StreamRoom(RoomStreamRequest request)
    {
        return _roomClient.StreamRoom(request);
    }

    public async Task<UpdateRoomStateResponse> UpdateRoomState(int roomId, string userId, int state, bool isReady)
    {
        try
        {
            var request = new UpdateRoomStateRequest { RoomId = roomId, UserId = userId, State = state, IsReady = isReady };
            var response = await _roomClient.UpdateRoomStateAsync(request);
            Debug.Log($"<color=green>状態更新成功:</color> User={userId}, State={state}");
            return response;
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"状態の更新に失敗しました: {e.Status.Detail}");
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

    public async Task<LeaveRoomResponse> LeaveRoom(int roomId, string userId)
    {
        try
        {
            var request = new LeaveRoomRequest { RoomId = roomId, UserId = userId };
            var response = await _roomClient.LeaveRoomAsync(request);
            return response;
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"部屋の退出に失敗しました: {e.Status.Detail}");
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

    public async Task<List<Room.Room>> GetBattlePlayer(int roomId)
    {
        try
        {
            var request = new ListRoomRequest{RoomId = roomId};
            var response = await _roomClient.ListRoomAsync(request);
            var battlePlayer = new List<Room.Room>(new Room.Room[2]);
            for (int i = 0; i < response.Rooms.Count; i++)
            {
                if (response.Rooms[i].State == 1)
                {
                    battlePlayer[0] = response.Rooms[i];
                }
                if (response.Rooms[i].State == 2)
                {
                    battlePlayer[1] = response.Rooms[i];
                }
            }
            return battlePlayer;
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"1P2Pの取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async Task<List<UniqueCharacter>> RegisterCharacters(int roomId, bool is1p, int[] characterIds)
    {
        try
        {
            var request = new RegisterCharactersRequest{RoomId = (uint)roomId, Is1P = is1p};
            request.CharacterIds.Add(characterIds.Select(x => (uint)x).ToArray());
            Debug.Log($"CharacterIds: {request.CharacterIds}");
            var response = await _battleClient.RegisterCharactersAsync(request);
            return new List<UniqueCharacter>(response.RegisteredCharacters);
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"キャラの登録に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async Task<GameDataResponse> GetGameData(int roomId)
    {
        try
        {
            var request = new GetGameDataRequest{RoomId = (uint)roomId};
            var reponse = await _battleClient.GetGameDataAsync(request);
            return reponse;
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"ゲームデータの取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public void StartStream(uint roomId, string playerId)
    {
        _cts = new CancellationTokenSource();
        _isStreamActive = true;
        _ = ConnectAndReceiveLoop(roomId, playerId);
    }

    private async Task ConnectAndReceiveLoop(uint roomId, string playerId)
    {
        while (_isStreamActive)
        {
            try
            {
                var request = new StreamGameRequest { RoomId = roomId, PlayerId = playerId };
                _call = _battleStreamClient.StreamGame(request, cancellationToken: _cts.Token);
                
                while (await _call.ResponseStream.MoveNext(_cts.Token))
                {
                    var response = _call.ResponseStream.Current;
                    Debug.Log($"<color=orange>[StreamGame] Server Push received.</color>");
                    HandleGameData(response);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常終了
                break;
            }
            catch (RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Cancelled)
            {
                // 正常終了
                break;
            }
            catch (Exception e)
            {
                if (!_isStreamActive) break;
                Debug.LogError($"<color=red>[Stream Error] connection lost: {e.Message}. Reconnecting in 3s...</color>");
                await Task.Delay(3000, _cts.Token);
            }
        }
    }
    private void HandleGameData(GameDataResponse data)
    {
        // Unity側で保持している変数をサーバーから受け取ったデータに更新
        characterManager.GetBattleData(data);
        Debug.Log($"Update received: {data}");
    }

    // おそらくこれを呼び出せばDBのApplyMoveが動くはずです
    public async Task SendMove(int roomId, string playerId, int charaId, int x, int y)
    {
        var move = new MoveAction{CharacterUniqueId = (uint)charaId, ToX = (uint)x, ToY = (uint)y};
        var action = new PlayerAction{RoomId = (uint)roomId, PlayerId = playerId, Move = move};
        
        try 
        {
            await _battleClient.ApplyMoveAsync(action);
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"移動の送信に失敗しました: {e.Status.Detail}");
        }
    }

    public async Task SendAttack(int roomId, string playerId, int attackerCharaId, int attackType, bool isStarted, int baseHP1, int baseHP2, int attackedCharaId, int newHP)
    {
        var attack = new AttackAction{AttackerCharacterUniqueId = (uint)attackerCharaId, AttackType = attackType, IsStarted = isStarted, BaseHp1 = (uint)baseHP1, BaseHp2 = (uint)baseHP2, AttackedCharacterUniqueId = (uint)attackedCharaId, NewHp = (uint)newHP};
        var action = new PlayerAction{RoomId = (uint)roomId, PlayerId = playerId, Attack = attack};
        
        Debug.Log($"<color=orange>[SendAttack] 通信開始: attacker={attackerCharaId}, target={attackedCharaId}, newHp={newHP}, bhp1={baseHP1}, bhp2={baseHP2}</color>");
        try 
        {
            await _battleClient.ApplyAttackAsync(action);
            Debug.Log($"<color=orange>[SendAttack] 通信成功</color>");
        }
        catch (RpcException e)
        {
            Debug.LogError($"[SendAttack] 通信失敗: {e.Status.Detail}");
            ShowErrorMessage($"攻撃の送信に失敗しました: {e.Status.Detail}");
        }
    }

    public async Task SendTurnEnd(int roomId, string playerId)
    {
        bool endTurn = true;
        var action = new PlayerAction{RoomId = (uint)roomId, PlayerId = playerId, EndTurn = endTurn};
        
        try 
        {
            await _battleClient.EndTurnAsync(action);
        }
        catch (RpcException e)
        {
            ShowErrorMessage($"ターン終了の送信に失敗しました: {e.Status.Detail}");
        }
    }

    public async Task SendGridUpdate(int roomId, string playerId, GridDataforOnline gridData, BattleDataforOmline battleData, bool is1p)
    {
        var gridUpdate = new GridUpdateAction();
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                int sendX = is1p ? x : 7 - x;
                int sendY = y;

                bool isSelected = false;
                for (int i = 0; i < battleData.charactersBattleDatas.Length; i++)
                {
                    if (battleData.charactersBattleDatas[i].now_character_position.x == x &&
                        battleData.charactersBattleDatas[i].now_character_position.y == y &&
                        battleData.character_isSelected[i])
                    {
                        isSelected = true;
                        break;
                    }
                }

                gridUpdate.Grids.Add(new GridInfo
                {
                    PositionX = (uint)sendX,
                    PositionY = (uint)sendY,
                    GridType = gridData.sub_grid_state_y[y].sub_grid_state_x[x],
                    IsSelected = isSelected,
                    IsAttackRange = gridData.grid_attack_position_y[y].grid_attack_position_x[x] == 1
                });
            }
        }

        var action = new PlayerAction { RoomId = (uint)roomId, PlayerId = playerId, GridUpdate = gridUpdate };

        try
        {
            await _battleClient.ApplyGridUpdateAsync(action);
            Debug.Log($"<color=lime>[GridUpdate] サーバーへの送信成功 (Room:{roomId})</color>");
        }
        catch (RpcException e)
        {
            Debug.LogError($"[GridUpdate] サーバーへの送信失敗: {e.Status.Detail}");
            ShowErrorMessage($"グリッド情報の送信に失敗しました: {e.Status.Detail}");
        }
    }


    public async Task StopStream()
    {
        _isStreamActive = false;
        try
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            if (_call != null)
            {
                _call.Dispose();
                _call = null;
            }
            await Task.CompletedTask;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"StopStream Error: {e.Message}");
        }
    }
}