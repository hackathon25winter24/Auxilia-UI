using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Cysharp.Threading.Tasks;
using Roommatch;
using Room;

public class MatchingConnector : MonoBehaviour
{
    private NetworkClientCore _core;
    private RoomMatchService.RoomMatchServiceClient _roomMatchClient;
    private RoomService.RoomServiceClient _roomClient;

    private AsyncDuplexStreamingCall<RoomStreamRequest, ListRoomResponse> _roomStreamCall;
    private CancellationTokenSource _roomStreamCts;
    private bool _isRoomStreamActive;

    public void Initialize(NetworkClientCore core)
    {
        _core = core;
        _roomMatchClient = new RoomMatchService.RoomMatchServiceClient(_core.Channel);
        _roomClient = new RoomService.RoomServiceClient(_core.Channel);
    }

    public async UniTask<RoomMatch> CreateRoomMatch(string roomName, string ownerId, bool isGaming)
    {
        try
        {
            var request = new CreateRoomMatchRequest { RoomName = roomName, OwnerId = ownerId, IsGaming = isGaming };
            var response = await _roomMatchClient.CreateRoomMatchAsync(request);
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
            var response = await _roomMatchClient.UpdateRoomMatchAsync(request);
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
            var response = await _roomMatchClient.ListRoomMatchAsync(request);
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
            return await _roomMatchClient.UpdateRoomMatchAsync(request);
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
            return await _roomClient.StartMatchAsync(request);
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
            return await _roomClient.UpdateRoomStateAsync(request);
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
            return await _roomClient.JoinRoomAsync(request);
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
            return await _roomClient.LeaveRoomAsync(request);
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
            return await _roomClient.EnterRingAsync(request);
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
            var response = await _roomClient.ListRoomAsync(request);
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
            var response = await _roomClient.ListRoomAsync(request);
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

    /// <summary>
    /// ルーム同期の双方向ストリームを開始し、監視ループを実行します
    /// </summary>
    public void StartRoomStream(int roomId, string userId, Action<ListRoomResponse> onRoomUpdated)
    {
        if (_isRoomStreamActive)
        {
            Debug.LogWarning("[MatchingConnector] 既にルームストリームが稼働しています。一度切断します。");
            _ = StopRoomStream();
        }

        _roomStreamCts = new CancellationTokenSource();
        _isRoomStreamActive = true;

        RoomStreamLoop(roomId, userId, onRoomUpdated, _roomStreamCts.Token).Forget();
    }

    /// <summary>
    /// ルームストリームを安全に終了・破棄します
    /// </summary>
    public async UniTask StopRoomStream()
    {
        _isRoomStreamActive = false;

        if (_roomStreamCall != null)
        {
            try
            {
                // クライアント側からの送信完了をサーバーに通知
                await _roomStreamCall.RequestStream.CompleteAsync();
            }
            catch (Exception) { /* 切断時の例外は無視 */ }
            
            _roomStreamCall.Dispose();
            _roomStreamCall = null;
        }

        if (_roomStreamCts != null)
        {
            _roomStreamCts.Cancel();
            _roomStreamCts.Dispose();
            _roomStreamCts = null;
        }

        Debug.Log("[MatchingConnector] Room stream stopped safely.");
    }

    /// <summary>
    /// 双方向ストリームの接続、リクエスト初期シグナルの送信、およびサーバー応答の受信を行う内部ループ
    /// </summary>
    private async UniTaskVoid RoomStreamLoop(int roomId, string userId, Action<ListRoomResponse> onRoomUpdated, CancellationToken ct)
    {
        Debug.Log($"[MatchingConnector] RoomStreamLoop Started. Room:{roomId}, User:{userId}");

        try
        {
            // 1. 双方向ストリーミングのコール（コネクション）を生成
            _roomStreamCall = _roomClient.StreamRoom(cancellationToken: ct);

            // 2. 双方向ストリームなので、まず「この部屋に居ます」という最初の要求(RequestStream)を書き込む
            var initialRequest = new RoomStreamRequest { RoomId = roomId, UserId = userId };
            await _roomStreamCall.RequestStream.WriteAsync(initialRequest);
            Debug.Log("[MatchingConnector] Initial RoomStreamRequest written to RequestStream.");

            // 3. サーバーから随時プッシュされてくる部屋の状態（ListRoomResponse）を監視・待機
            while (await _roomStreamCall.ResponseStream.MoveNext(ct))
            {
                ListRoomResponse response = _roomStreamCall.ResponseStream.Current;
                Debug.Log($"[MatchingConnector] ルーム更新データを受信しました。参加人数: {response.Rooms.Count}");

                if (onRoomUpdated != null)
                {
                    // Unityのメインスレッドに戻して安全にコールバック（UI更新など）を実行
                    await UniTask.SwitchToMainThread(ct);
                    onRoomUpdated.Invoke(response);
                }
            }
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            Debug.Log("[MatchingConnector] ルームストリームが正常に切断（キャンセル）されました。");
        }
        catch (Exception e)
        {
            Debug.LogError($"[MatchingConnector] ルームストリームループ内で例外が発生しました: {e.Message}");
            _core?.ShowErrorMessage("ルームのリアルタイム同期が切断されました。");
        }
        finally
        {
            _isRoomStreamActive = false;
            Debug.Log("[MatchingConnector] RoomStreamLoop Finished.");
        }
    }
}