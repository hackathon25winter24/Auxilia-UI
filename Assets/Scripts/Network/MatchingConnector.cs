using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Roommatch;
using Room;

public class MatchingConnector : MonoBehaviour
{
    private NetworkClientCore _core;
    private RoomMatchService.RoomMatchServiceClient _roomMatchClient;
    private RoomService.RoomServiceClient _roomClient;

    public void Initialize(NetworkClientCore core)
    {
        _core = core;
        _roomMatchClient = new RoomMatchService.RoomMatchServiceClient(_core.Channel);
        _roomClient = new RoomService.RoomServiceClient(_core.Channel);
    }

    public async Task<RoomMatch> CreateRoomMatch(string roomName, string ownerId, bool isGaming)
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

    public async Task<RoomMatch> UpdateRoomMatch(int roomId, string roomName, string ownerId, bool isGaming)
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

    public async Task<List<RoomMatch>> GetAllRoomMatch()
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

    public async Task<RoomMatchResponse> UpdateRoomName(int roomId, string roomName, string ownerId, bool isGaming)
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

    public async Task<StartMatchResponse> StartMatch(int roomId)
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

    public AsyncServerStreamingCall<ListRoomResponse> StreamRoom(RoomStreamRequest request)
    {
        return _roomClient.StreamRoom(request);
    }

    public async Task<UpdateRoomStateResponse> UpdateRoomState(int roomId, string userId, int state, bool isReady)
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

    public async Task<JoinRoomResponse> JoinRoom(int roomId, string userId)
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

    public async Task<LeaveRoomResponse> LeaveRoom(int roomId, string userId)
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

    public async Task<EnterRingResponse> EnterRing(int roomId, string userId)
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

    public async Task<List<Room.Room>> ListRoom(int roomId)
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

    public async Task<List<Room.Room>> GetBattlePlayer(int roomId)
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
}