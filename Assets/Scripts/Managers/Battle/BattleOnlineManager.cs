using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>Game scene coordinator. Network data is written by BattleConnector only.</summary>
public class BattleOnlineManager : MonoBehaviour
{
    public CharacterData characterData;
    public InputManager inputManager;
    public UserData userData;
    public BattleDataForOnline battleDataforOnline;
    public BattleDataforLocal battleDataforLocal;
    public RoomData roomData;
    public BattleViewManager battleViewManager;
    public TMPro.TextMeshProUGUI gametext;
    public UnityEngine.UI.Slider timerSlider;
    private NetworkManager Net => NetworkManager.Instance;
    private CancellationTokenSource destroyCts;
    private ViewManager view;
    private bool leaving;

    private async void Start()
    {
        destroyCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        if (roomData == null || Net?.Battle == null || battleDataforOnline == null)
        {
            Debug.LogError("オンライン戦闘のNetworkManager / RoomData / BattleDataForOnlineを設定してください。", this);
            return;
        }
        Net.Battle.BindData(battleDataforOnline);
        view = GetComponent<ViewManager>();
        if (view == null) view = gameObject.AddComponent<ViewManager>();
        view.Bind(battleDataforOnline);
        view.ViewChanged += CheckFinished;
        if (inputManager != null) inputManager.OnSpaceKeyClicked += EndMyTurn;
        try
        {
            var snapshot = await Net.Battle.GetRoomGameData(roomData.room_id, destroyCts.Token);
            if (snapshot == null) return;
            await Net.Battle.GetDefinitions(destroyCts.Token);
            Net.Battle.StartStream(destroyCts.Token);
            // A scene entered with an unchanged cached snapshot still needs its first render.
            view.ChangeView();
        }
        catch (OperationCanceledException) { }
    }

    public void EndMyTurn()
    {
        var state = battleDataforOnline?.State;
        if (Net?.Battle == null || state == null || !state.Started || state.Finished ||
            state.TurnPlayerId != Net.Core.PlayerId || state.Phase != "action") return;
        EndTurnAsync().Forget();
    }

    private async UniTask EndTurnAsync()
    {
        try { await Net.Battle.SendTurnEnd(destroyCts.Token); }
        catch (OperationCanceledException) { }
    }

    public void StartMyTurn() { }

    private void CheckFinished()
    {
        if (!leaving && battleDataforOnline.is_finished)
        {
            leaving = true;
            SceneChangeManager.MoveScene(6);
        }
    }

    private void OnDestroy()
    {
        if (view != null) view.ViewChanged -= CheckFinished;
        if (inputManager != null) inputManager.OnSpaceKeyClicked -= EndMyTurn;
        Net?.Battle?.StopStream().Forget();
        destroyCts?.Cancel();
        destroyCts?.Dispose();
    }
}
