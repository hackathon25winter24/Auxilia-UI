using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Game.Network;

public class BattleConnector : MonoBehaviour
{
    private NetworkClientCore _core;
    private BattleService.BattleServiceClient _battleClient;
    private BattleService.BattleServiceClient _battleStreamClient;

    private AsyncServerStreamingCall<GameDataResponse> _call;
    private CancellationTokenSource _cts;
    private bool _isStreamActive;

    private BattleOnlineManager _battleOnlineManager;

    public void Initialize(NetworkClientCore core, BattleOnlineManager battleOnlineManager)
    {
        _core = core;
        _battleOnlineManager = battleOnlineManager;
        
        _battleClient = new BattleService.BattleServiceClient(_core.Channel);
        _battleStreamClient = new BattleService.BattleServiceClient(_core.StreamChannel);
    }

    /// <summary>
    /// rpc CreateGame(CreateGameRequest) returns (GameDataResponse);
    /// </summary>
    public async UniTask<GameDataResponse> CreateGameData(uint roomID, string player1Id, string player2Id)
    {
        try
        {
            var request = new CreateGameRequest { RoomId = roomID, Player1Id = player1Id, Player2Id = player2Id };
            return await _battleClient.CreateGameAsync(request);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"ゲームデータの作成に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    /// <summary>
    /// rpc RegisterCharacters(RegisterCharactersRequest) returns (RegisterCharactersResponse);
    /// </summary>
    public async UniTask<List<UniqueCharacter>> RegisterCharacters(int roomId, bool is1p, int[] characterIds)
    {
        try
        {
            var request = new RegisterCharactersRequest { RoomId = (uint)roomId, Is1P = is1p };
            request.CharacterIds.Add(characterIds.Select(x => (uint)x).ToArray());
            var response = await _battleClient.RegisterCharactersAsync(request);
            return new List<UniqueCharacter>(response.RegisteredCharacters);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"キャラの登録に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    /// <summary>
    /// rpc GetGameData(GetGameDataRequest) returns (GameDataResponse);
    /// </summary>
    public async UniTask<GameDataResponse> GetGameData(int roomId)
    {
        try
        {
            var request = new GetGameDataRequest { RoomId = (uint)roomId };
            return await _battleClient.GetGameDataAsync(request);
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"ゲームデータの取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }

    /// <summary>
    /// rpc StreamGame(StreamGameRequest) returns (stream GameDataResponse);
    /// </summary>
    public void StartStream(uint roomId, string playerId)
    {
        _cts = new CancellationTokenSource();
        _isStreamActive = true;
        _ = ConnectAndReceiveLoop(roomId, playerId);
    }

    private async UniTask ConnectAndReceiveLoop(uint roomId, string playerId)
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
                    await HandleGameData(response);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled) { break; }
            catch (Exception e)
            {
                if (!_isStreamActive) break;
                Debug.LogError($"<color=red>[Stream Error] connection lost: {e.Message}. Reconnecting in 3s...</color>");
                await Task.Delay(3000, _cts.Token);
            }
        }
    }

    private async UniTask HandleGameData(GameDataResponse data)
    {
        if (_battleOnlineManager != null)
        {
            _battleOnlineManager.ReceiveBattleData(data);
        }
        await Task.CompletedTask;
    }

    public async UniTask StopStream()
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

    /// <summary>
    /// rpc ApplyMove(MoveAction) returns (AcceptResponse);
    /// </summary>
    public async UniTask<bool> SendMove(int roomId, string playerId, int charaId, int x, int y, CancellationToken ct = default)
    {
        var request = new MoveAction { RoomId = (uint)roomId, PlayerId = playerId, CharacterId = (uint)charaId, ToX = (uint)x, ToY = (uint)y };
        try 
        {
            var response = await _battleClient.ApplyMoveAsync(request, cancellationToken: ct);
            return response.Success;
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"移動の送信に失敗しました: {e.Status.Detail}");
            return false;
        }
    }

    /// <summary>
    /// rpc ApplyAttack(AttackAction) returns (AcceptResponse);
    /// </summary>
    public async UniTask<bool> SendAttack(int roomId, string playerId, int attackerCharaId, int attackType, List<AttackInfo> attackInfos, CancellationToken ct = default)
    {
        var request = new AttackAction { RoomId = (uint)roomId, PlayerId = playerId, AttackerCharacterId = (uint)attackerCharaId, AttackType = attackType };
        if (attackInfos != null) request.AttackInfos.AddRange(attackInfos);
        
        try 
        {
            var response = await _battleClient.ApplyAttackAsync(request, cancellationToken: ct);
            return response.Success;
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"攻撃の送信に失敗しました: {e.Status.Detail}");
            return false;
        }
    }

/// <summary>
/// 変更のあったグリッドの差分（1マス分）だけをサーバーに送信する
/// rpc ApplyGridUpdate(GridUpdateAction) returns (AcceptResponse);
/// </summary>
    public async UniTask<bool> SendGridUpdate(int roomId, string playerId, int x, int y, int gridType, int debuffType, bool is1p, CancellationToken ct = default)
    {
        if (_battleClient == null) return false;

        // 💡 自分が2Pの場合、サーバーの絶対座標（1P視点基準）に反転させる
        int sendX = is1p ? x : 7 - x;
        int sendY = y;

        var request = new GridUpdateAction
        {
            RoomId = (uint)roomId,
            // PlayerId = playerId // ※もし .proto に player_id を追加した場合はここを有効化してください
            Grid = new GridInfo
            {
                PositionX = (uint)sendX,
                PositionY = (uint)sendY,
                GridType = gridType,
                DebuffType = debuffType,
                IsCharacterStay = false // 💡 座標競合を防ぐため、フロントからは一律false（判定はサーバーに一任）を推奨
            }
        };
        
        try
        {
            var response = await _battleClient.ApplyGridUpdateAsync(request, cancellationToken: ct);
            Debug.Log($"<color=lime>[GridUpdate] 差分送信成功: ({sendX}, {sendY}) -> Type:{gridType}</color>");
            return response.Success;
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"グリッド情報の差分送信に失敗しました: {e.Status.Detail}");
            return false;
        }
    }

    /// <summary>
    /// rpc EndTurn(EndTurnRequest) returns (AcceptResponse);
    /// </summary>
    public async UniTask<bool> SendTurnEnd(int roomId, string playerId, CancellationToken ct = default)
    {
        var request = new EndTurnRequest { RoomId = (uint)roomId, PlayerId = playerId };
        try 
        {
            var response = await _battleClient.EndTurnAsync(request, cancellationToken: ct);
            return response.Success;
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"ターン終了の送信に失敗しました: {e.Status.Detail}");
            return false;
        }
    }

    /// <summary>
    /// rpc NewTurn(NewTurnRequest) returns (AcceptResponse);
    /// </summary>
    public async UniTask<bool> SendNewTurn(int roomId, string playerId, CancellationToken ct = default)
    {
        var request = new NewTurnRequest { RoomId = (uint)roomId, PlayerId = playerId };
        try
        {
            // 💡 proto定義に合わせ、自動生成関数名を NewTurnAsync に修正
            var response = await _battleClient.NewTurnAsync(request, cancellationToken: ct);
            return response.Success;
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"新規ターン開始の送信に失敗しました: {e.Status.Detail}");
            return false;
        }
    }

    /// <summary>
    /// rpc FetchActionLog(FetchActionLogRequest) returns (GameActionLog);
    /// </summary>
    public async UniTask<GameActionLog> FetchActionLog(int roomId, uint sequence, CancellationToken ct = default)
    {
        var request = new FetchActionLogRequest { RoomId = (uint)roomId, Sequence = sequence };
        try
        {

            var response = await _battleClient.FetchActionLogAsync(request, cancellationToken: ct);
            return response;
        }
        catch (RpcException e)
        {
            _core.ShowErrorMessage($"アクションログ(Seq:{sequence})の取得に失敗しました: {e.Status.Detail}");
            return null;
        }
    }
}

//使用例
// private CancellationTokenSource _cts;

//     private void Start()
//     {
//         // 1. スイッチ（Source）を作成
//         _cts = new CancellationTokenSource();

//         // 2. 処理を始めるときに、旗（cancellationToken）を渡す
//         //実行中の非同期処理やバックグラウンド処理を、安全かつ確実に途中で終了（キャンセル）させるための仕組み
//         StartNetworkConnection(_cts.Token);
//     }

//     private void OnDestroy()
//     {
//         // 3. 自身が破壊されたら（シーン遷移など）、実行中の処理にストップをかける
//         if (_cts != null)
//         {
//             _cts.Cancel(); // これで旗に「ストップ」が書き込まれる
//             _cts.Dispose();
//         }
//     }


// public async UniTask StartNetworkConnection(CancellationToken ct)
// {
//     try
//     {
//         Debug.Log("通信を開始します...");

//         // gRPCの通信APIに、旗（ct）をそのまま渡す
//         // もし通信中に ct.Cancel() が呼ばれると、この通信はその瞬間に強制終了します
//         var response = await _battleClient.ApplyMoveAsync(request, cancellationToken: ct);
        
//         Debug.Log("通信が正常に完了しました！");
//     }
//     catch (System.OperationCanceledException)
//     {
//         // 💡 キャンセルされた場合は、ここを通過して安全に終了する
//         Debug.LogWarning("通信処理が安全にキャンセルされました。");
//     }
// }