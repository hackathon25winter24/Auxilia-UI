using System;
using System.Collections.Generic;
using System.Linq;
using Game.Network.V2;
using Google.Protobuf;
using UnityEngine;

public enum RoomChangeType { RoomSync, MembersChanged, ReadyChanged, OwnerChanged, NameChanged, PhaseChanged, PermissionsChanged, RoomClosed, ConnectionChanged, OperationChanged }
public enum RoomConnectionState { Disconnected, Connecting, Connected, Reconnecting, Unavailable }

public partial class RoomData
{
    [SerializeField, TextArea(3, 12)] private string onlineSnapshotJson;
    [NonSerialized] private RoomSnapshot onlineSnapshot;
    // Return a copy: consumers cannot silently modify server-owned state.
    public RoomSnapshot Snapshot => onlineSnapshot?.Clone();
    public ulong Revision => onlineSnapshot?.Revision ?? 0;
    public string MatchId => onlineSnapshot?.MatchId ?? "";
    public RoomConnectionState Connection { get; private set; }
    public bool IsBusy { get; private set; }
    public event Action<RoomChangeType> Changed;
    private int notificationGeneration;

    public void ResetOnline()
    {
        notificationGeneration++;
        onlineSnapshot = null;
        onlineSnapshotJson = "";
        room_id = 0; room_name = ""; room_my_index = -1;
        usersData = Array.Empty<UsersData>();
        Connection = RoomConnectionState.Disconnected;
        Changed?.Invoke(RoomChangeType.RoomSync);
    }

    public bool StoreOnline(RoomSnapshot incoming, string playerId)
    {
        if (incoming == null || incoming.RoomId == 0 || incoming.Revision == 0) return false;
        var old = onlineSnapshot;
        if (old != null && (old.RoomId != incoming.RoomId || incoming.Revision <= old.Revision)) return false;
        onlineSnapshot = incoming.Clone();
        onlineSnapshotJson = JsonFormatter.Default.Format(onlineSnapshot);
        room_id = checked((int)onlineSnapshot.RoomId); room_name = onlineSnapshot.RoomName;
        usersData = onlineSnapshot.Members.Select(m => new UsersData {
            user_id = m.UserId, user_rate = m.Rate, user_state = m.Seat,
            is_ready = m.Ready, is_host = m.UserId == onlineSnapshot.OwnerId
        }).ToArray();
        room_my_index = Array.FindIndex(usersData, u => u.user_id == playerId);
        var changes = new List<RoomChangeType>();
        if (old == null) changes.Add(RoomChangeType.RoomSync);
        else
        {
            if (!old.Members.Select(m => (m.UserId, m.Name, m.Rate, m.Seat, m.JoinedAt)).SequenceEqual(
                onlineSnapshot.Members.Select(m => (m.UserId, m.Name, m.Rate, m.Seat, m.JoinedAt)))) changes.Add(RoomChangeType.MembersChanged);
            if (!old.Members.Select(m => (m.UserId, m.Ready)).SequenceEqual(onlineSnapshot.Members.Select(m => (m.UserId, m.Ready)))) changes.Add(RoomChangeType.ReadyChanged);
            if (old.OwnerId != onlineSnapshot.OwnerId) changes.Add(RoomChangeType.OwnerChanged);
            if (old.RoomName != onlineSnapshot.RoomName) changes.Add(RoomChangeType.NameChanged);
            if (old.Phase != onlineSnapshot.Phase || old.MatchId != onlineSnapshot.MatchId) changes.Add(RoomChangeType.PhaseChanged);
            if (old.CanStart != incoming.CanStart || old.StartBlockReason != incoming.StartBlockReason || old.CanReady != incoming.CanReady ||
                old.CanRename != incoming.CanRename || old.CanEnterRing != incoming.CanEnterRing || old.CanLeaveRing != incoming.CanLeaveRing || old.CanLeaveRoom != incoming.CanLeaveRoom)
                changes.Add(RoomChangeType.PermissionsChanged);
            if (changes.Count == 0) changes.Add(RoomChangeType.RoomSync);
        }
        if (incoming.Phase == LobbyPhase.Closed) changes.Add(RoomChangeType.RoomClosed);
        int generation = ++notificationGeneration;
        foreach (var type in changes) { if (generation != notificationGeneration) break; Changed?.Invoke(type); }
        return true;
    }

    public void SetConnection(RoomConnectionState state)
    {
        if (Connection == state) return;
        Connection = state; Changed?.Invoke(RoomChangeType.ConnectionChanged);
    }
    public void SetBusy(bool busy)
    {
        if (IsBusy == busy) return;
        IsBusy = busy; Changed?.Invoke(RoomChangeType.OperationChanged);
    }
}
