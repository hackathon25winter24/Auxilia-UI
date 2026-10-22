using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Game.Network.V2;

public class RoomUIManager : MonoBehaviour
{
    public InputManager inputManager;
    public UserData userData;
    public RoomData roomData;
    public Image[] joinnersUI;
    public Sprite[] joinnersUIImage;
    public TextMeshProUGUI[] userName;
    public TextMeshProUGUI[] userRate;
    public GameObject renameRoomUI;
    public TMP_InputField renameRoomText;
    [SerializeField] TextMeshProUGUI roomNameText;
    private TextMeshProUGUI startBattleButtonText;
    private Button startButton;
    private GameObject editButton;
    private RoomUseCase useCase;
    private bool navigating;

    private async void Start()
    {
        try
        {
            var button = GameObject.Find("StartBattleButton");
            startButton = button?.GetComponent<Button>();
            startBattleButtonText = button?.GetComponentInChildren<TextMeshProUGUI>();
            editButton = GameObject.Find("EditButton");
            int id = roomData.room_id;
            useCase = NetworkManager.Instance.ConfigureRoom(roomData);
            roomData.Changed += OnRoomChanged;
            if (roomData.Snapshot == null) await useCase.Resume(checked((uint)id), this.GetCancellationTokenOnDestroy());
            else await useCase.Refresh(this.GetCancellationTokenOnDestroy());
            if (this != null) OnRoomChanged(RoomChangeType.RoomSync);
        }
        catch (OperationCanceledException) { }
        catch (Exception e) { Debug.LogError($"部屋情報の取得に失敗しました: {e.Message}"); }
    }

    private void OnDestroy()
    {
        if (roomData != null) roomData.Changed -= OnRoomChanged;
        // NetworkManager owns the watch across selection, battle and result scenes.
    }

    private void OnRoomChanged(RoomChangeType type)
    {
        UpDateRoomVisuals(null);
        var snapshot = roomData.Snapshot;
        if (navigating || snapshot == null || string.IsNullOrEmpty(snapshot.MatchId)) return;
        if (snapshot.Phase == LobbyPhase.Selecting || snapshot.Phase == LobbyPhase.Battling)
        {
            navigating = true;
            // Selection also initializes the battle snapshot/stream when resuming a battle.
            SceneChangeManager.MoveScene(10);
        }
    }

    public void UpDateRoomVisuals(List<Room.Room> unused)
    {
        var snapshot = roomData.Snapshot;
        if (snapshot == null) return;
        if (roomNameText != null) roomNameText.text = snapshot.RoomName;
        // Keep players ahead of spectators, without changing server-owned SO data.
        var displayedMembers = snapshot.Members.OrderBy(m => m.Seat == 0 ? 1 : 0).ToArray();
        for (int i = 0; i < joinnersUI.Length; i++)
        {
            var member = i < displayedMembers.Length ? displayedMembers[i] : null;
            joinnersUI[i].sprite = joinnersUIImage[member == null ? 0 : 1];
            if (i < userName.Length) userName[i].text = member == null ? "-----" :
                (member.Seat == 1 ? "[1P] " : member.Seat == 2 ? "[2P] " : "[観戦] ") + member.Name;
            if (i < userRate.Length) userRate[i].text = member == null ? "Rate: ----" : "Rate: " + member.Rate;
            var outline = joinnersUI[i].GetComponent<Outline>();
            if (outline == null)
            {
                outline = joinnersUI[i].gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(3, -3);
            }
            outline.enabled = member?.Ready == true;
            outline.effectColor = Color.green;
        }
        var me = snapshot.Members.FirstOrDefault(m => m.UserId == userData.user_id);
        bool owner = snapshot.OwnerId == userData.user_id;
        if (editButton != null) editButton.SetActive(snapshot.CanRename);
        if (startBattleButtonText != null)
            startBattleButtonText.text = owner && snapshot.CanStart ? "対戦開始" :
                snapshot.CanReady ? (me?.Ready == true ? "準備完了解除" : "準備完了") : "開始待ち";
        if (startButton != null) startButton.interactable = !roomData.IsBusy &&
            roomData.Connection != RoomConnectionState.Unavailable && (snapshot.CanStart || snapshot.CanReady);
    }

    public async void OnClickReadyOrStart()
    {
        if (useCase == null || roomData.IsBusy || navigating) return;
        try
        {
            var snapshot = roomData.Snapshot;
            if (snapshot == null) return;
            if (snapshot.CanStart) await useCase.StartMatch();
            else if (snapshot.CanReady)
            {
                var me = snapshot.Members.FirstOrDefault(m => m.UserId == userData.user_id);
                if (me != null) await useCase.SetReady(!me.Ready);
            }
        }
        catch (Exception e) { Debug.LogError($"準備・開始に失敗しました: {e.Message}"); }
    }

    public async void OnClickLeaveRoom()
    {
        if (useCase == null || roomData.IsBusy || navigating) return;
        try
        {
            await useCase.Leave();
            if (this != null) { navigating = true; SceneChangeManager.MoveScene(3); }
        }
        catch (Exception e) { Debug.LogError($"退室に失敗しました: {e.Message}"); }
    }

    // Existing RoomScene UnityEvents use these string arguments.
    public async void OnButtonClick(string action)
    {
        if (useCase == null || roomData.IsBusy || navigating) return;
        try
        {
            switch (action)
            {
                case "RenameRoom":
                    if (roomData.Snapshot?.CanRename == true) renameRoomUI.SetActive(true);
                    break;
                case "RenameRoomBack": renameRoomUI.SetActive(false); break;
                case "AplyRenameRoom":
                    await useCase.Rename(renameRoomText.text);
                    if (this != null) renameRoomUI.SetActive(false);
                    break;
                case "changeStatus":
                    if (roomData.Snapshot?.CanLeaveRing == true) await useCase.LeaveRing();
                    else if (roomData.Snapshot?.CanEnterRing == true) await useCase.EnterRing();
                    break;
            }
        }
        catch (Exception e) { Debug.LogError($"部屋の操作に失敗しました: {e.Message}"); }
    }

    public void DebugBack() => OnClickLeaveRoom();
}
