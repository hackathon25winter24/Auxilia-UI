using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Network.V2;
using Grpc.Core;

/// <summary>Transport only. No SO, UI, scene, retry, or membership decisions.</summary>
public sealed class LobbyConnector
{
    private readonly LobbyServiceV2.LobbyServiceV2Client unary, streaming;
    private readonly Func<Metadata> headers;
    public LobbyConnector(NetworkClientCore core) : this(new LobbyServiceV2.LobbyServiceV2Client(core.Channel),
        new LobbyServiceV2.LobbyServiceV2Client(core.StreamChannel), () => core.SessionHeaders) { }
    public LobbyConnector(LobbyServiceV2.LobbyServiceV2Client unary, LobbyServiceV2.LobbyServiceV2Client streaming, Func<Metadata> headers)
    { this.unary = unary; this.streaming = streaming; this.headers = headers; }
    public async Task<RoomSnapshot> Create(string name, CancellationToken ct)
    { using var call = unary.CreateLobbyAsync(new CreateLobbyRequest { RoomName = name }, headers(), cancellationToken: ct); return await call.ResponseAsync; }
    public async Task<RoomSnapshot> Join(uint roomId, CancellationToken ct)
    { using var call = unary.JoinLobbyAsync(new LobbyRequest { RoomId = roomId }, headers(), cancellationToken: ct); return await call.ResponseAsync; }
    public async Task<RoomSnapshot> Get(uint roomId, CancellationToken ct)
    { using var call = unary.GetLobbyAsync(new LobbyRequest { RoomId = roomId }, headers(), cancellationToken: ct); return await call.ResponseAsync; }
    public async Task<RoomSnapshot> Apply(RoomCommand request, CancellationToken ct)
    { using var call = unary.ApplyRoomCommandAsync(request, headers(), cancellationToken: ct); return await call.ResponseAsync; }
    public AsyncServerStreamingCall<RoomSnapshot> Watch(uint roomId, CancellationToken ct) =>
        streaming.WatchRoom(new LobbyRequest { RoomId = roomId }, headers(), cancellationToken: ct);
}
