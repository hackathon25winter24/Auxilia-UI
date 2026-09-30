using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Cysharp.Threading.Tasks;
using Roommatch;
using Room;
using Game.Network.V2;

public class MatchingConnector : MonoBehaviour
{
    private NetworkClientCore _core;
    private RoomMatchServiceV2.RoomMatchServiceV2Client _roomMatchClient;
    private RoomServiceV2.RoomServiceV2Client _roomClient;

    private CancellationTokenSource _roomStreamCts;
    public event Action<int> MatchStarted;

    public void Initialize(NetworkClientCore core)
    {
        _core = core;
        _roomMatchClient = new RoomMatchServiceV2.RoomMatchServiceV2Client(_core.Channel);
        _roomClient = new RoomServiceV2.RoomServiceV2Client(_core.Channel);
    }

    public async UniTask<RoomMatch> CreateRoomMatch(string roomName, string ownerId, bool isGaming)
    {
        try
        {
            var request = new CreateRoomMatchRequest { RoomName = roomName, OwnerId = ownerId, IsGaming = isGaming };
            var response = await _roomMatchClient.CreateRoomMatchAsync(request, _core.SessionHeaders);
            return response.Room;
        }
        catch (RpcException e)
        {
            string errorMessage = e.StatusCode switch {
                StatusCode.InvalidArgument => "部屋名が正しくありません（10文字以内）。",
                StatusCode.Internal => "サーバーエラーで部屋を作成できませんでした。",
                _ => $"部屋作成エラー: {e.Status.Detail}"
            };
            _core.ShowErrorMessage(errorMessage);
            return null;
        }
    }

    public async UniTask<RoomMatch> UpdateRoomMatch(int roomId, string roomName, string ownerId, bool isGaming)
    {
        try
        {
            var request = new UpdateRoomMatchRequest { RoomId = roomId, RoomName = roomName, IsGaming = isGaming, OwnerId = ownerId };
            var response = await _roomMatchClient.UpdateRoomMatchAsync(request, _core.SessionHeaders);
            return response.Room;
        }
        catch (RpcException e)
        {
            string errorMessage = e.StatusCode switch {
                StatusCode.InvalidArgument => "部屋名が正しくありません（10文字以内）。",
                StatusCode.Internal => "サーバーエラーで部屋を更新できませんでした。",
                _ => $"部屋更新エラー: {e.Status.Detail}"
            };
            _core.ShowErrorMessage(errorMessage);
            return null;
        }
    }

    public async UniTask<List<RoomMatch>> GetAllRoomMatch()
    {
        try
        {
            var request = new ListRoomMatchRequest();
            var response = await _roomMatchClient.ListRoomMatchAsync(request, _core.SessionHeaders);
            return new List<RoomMatch>(response.Rooms);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"部屋リストの取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async UniTask<RoomMatchResponse> UpdateRoomName(int roomId, string roomName, string ownerId, bool isGaming)
    {
        try
        {
            var request = new UpdateRoomMatchRequest { RoomId = roomId, RoomName = roomName, OwnerId = ownerId, IsGaming = isGaming };
            return await _roomMatchClient.UpdateRoomMatchAsync(request, _core.SessionHeaders);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"部屋名の更新に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async UniTask<StartMatchResponse> StartMatch(int roomId)
    {
        try
        {
            var request = new StartMatchRequest { RoomId = roomId };
            return await _roomClient.StartMatchAsync(request, _core.SessionHeaders);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"試合開始に失敗しました: {e.Status.Detail}");
            return null;
        }
    }


    public async UniTask<UpdateRoomStateResponse> UpdateRoomState(int roomId, string userId, int state, bool isReady)
    {
        try
        {
            var request = new UpdateRoomStateRequest { RoomId = roomId, UserId = userId, State = state, IsReady = isReady };
            return await _roomClient.UpdateRoomStateAsync(request, _core.SessionHeaders);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"状態の更新に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async UniTask<JoinRoomResponse> JoinRoom(int roomId, string userId)
    {
        try
        {
            var request = new JoinRoomRequest { RoomId = roomId, UserId = userId };
            return await _roomClient.JoinRoomAsync(request, _core.SessionHeaders);
        }
        catch (RpcException e)
        {
            string errorMessage = e.StatusCode switch {
                StatusCode.NotFound => "指定された部屋が見つかりませんでした。",
                StatusCode.Internal => "サーバーエラーで部屋に参加できませんでした。",
                _ => $"部屋参加エラー: {e.Status.Detail}"
            };
            _core.ShowErrorMessage(errorMessage);
            return null;
        }
    }

    public async UniTask<LeaveRoomResponse> LeaveRoom(int roomId, string userId)
    {
        try
        {
            var request = new LeaveRoomRequest { RoomId = roomId, UserId = userId };
            return await _roomClient.LeaveRoomAsync(request, _core.SessionHeaders);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"部屋の退出に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async UniTask<EnterRingResponse> EnterRing(int roomId, string userId)
    {
        try
        {
            var request = new EnterRingRequest { RoomId = roomId, UserId = userId };
            return await _roomClient.EnterRingAsync(request, _core.SessionHeaders);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"リング参加に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async UniTask<List<Room.Room>> ListRoom(int roomId)
    {
        try
        {
            var request = new ListRoomRequest { RoomId = roomId };
            var response = await _roomClient.ListRoomAsync(request, _core.SessionHeaders);
            return new List<Room.Room>(response.Rooms);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"参加者リストの取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async UniTask<List<Room.Room>> GetBattlePlayer(int roomId)
    {
        try
        {
            var request = new ListRoomRequest { RoomId = roomId };
            var response = await _roomClient.ListRoomAsync(request, _core.SessionHeaders);
            var battlePlayer = new List<Room.Room>(new Room.Room[2]);
            for (int i = 0; i < response.Rooms.Count; i++)
            {
                if (response.Rooms[i].State == 1) battlePlayer[0] = response.Rooms[i];
                if (response.Rooms[i].State == 2) battlePlayer[1] = response.Rooms[i];
            }
            return battlePlayer;
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"1P2Pの取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    public async UniTask<SetReadyResponse> SetReady(int roomId, string userId, bool isReady)
    {

        Debug.Log($"[MatchingConnector] Sending SetReady... Room: {roomId}, User: {userId}, Ready: {isReady}");

        try
        {
            var request = new SetReadyRequest{RoomId = roomId,UserId = userId,Ready = isReady};
            // 通常の_channelを使ってクライアントを生成し、非同期でリクエストを送信
            
            // このコンポーネントが破棄されたらキャンセルされるようにトークンを渡す
            var response = await _roomClient.SetReadyAsync(request, _core.SessionHeaders, cancellationToken: this.GetCancellationTokenOnDestroy());
            
            Debug.Log($"[MatchingConnector] SetReady Response Received. Total Rooms Count: {response.Rooms.Count}");
            return response;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            // Go側で「spectator cannot ready (観戦者は準備完了できない)」が返ってきた場合のハンドリング
            Debug.LogWarning($"[MatchingConnector] SetReady Rejected: 観戦者は準備完了できません。");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MatchingConnector] SetReady Error: {ex.Message}");
            throw;
        }
    }

    // gRPC-Web cannot carry a duplex RPC. Poll the authenticated V2 ListRoom endpoint.
    public void StartRoomStream(int roomId, string userId, Action<ListRoomResponse> onRoomUpdated)
    {
        StopRoomStream().Forget();
        _roomStreamCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        RoomStreamLoop(roomId, onRoomUpdated, _roomStreamCts.Token).Forget();
    }
    public UniTask StopRoomStream()
    {
        var source = _roomStreamCts;
        _roomStreamCts = null;
        source?.Cancel();
        source?.Dispose();
        return UniTask.CompletedTask;
    }
    private async UniTask RoomStreamLoop(int roomId, Action<ListRoomResponse> callback, CancellationToken ct)
    {
        ListRoomResponse previous = null;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var call = _roomClient.ListRoomAsync(new ListRoomRequest { RoomId = roomId }, _core.SessionHeaders, cancellationToken: ct);
                    var response = await call.ResponseAsync;
                    await UniTask.SwitchToMainThread(ct);
                    if (!response.Equals(previous)) callback?.Invoke(response);
                    previous = response;
                    using var matchesCall = _roomMatchClient.ListRoomMatchAsync(new ListRoomMatchRequest(), _core.SessionHeaders, cancellationToken: ct);
                    var matches = await matchesCall.ResponseAsync;
                    await UniTask.SwitchToMainThread(ct);
                    ct.ThrowIfCancellationRequested();
                    foreach (var room in matches.Rooms)
                        if (room.RoomId == roomId && room.IsGaming)
                        {
                            MatchStarted?.Invoke(roomId);
                            return;
                        }
                }
                catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled && ct.IsCancellationRequested) { return; }
                catch (RpcException e)
                {
                    await UniTask.SwitchToMainThread(ct);
                    _core.ShowErrorMessage($"ルーム同期に失敗しました: {e.Status.Detail}");
                    if (e.StatusCode == StatusCode.Unauthenticated || e.StatusCode == StatusCode.PermissionDenied) return;
                }
                await UniTask.Delay(1000, cancellationToken: ct);
            }
        }
        catch (OperationCanceledException) { }
    }
    private void OnDestroy() => StopRoomStream().Forget();
}
