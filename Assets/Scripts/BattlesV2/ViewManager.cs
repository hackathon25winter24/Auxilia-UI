using UnityEngine;
using UnityEngine.Events;
using System;

public class ViewManager : MonoBehaviour
{
    [SerializeField] private BattleDataForOnline battleDataForOnline;
    [SerializeField] private UserData userData;
    [SerializeField] private CharacterData characterData;
    [SerializeField] private GridViewManager gridViewManager;
    [SerializeField] private CharacterViewManager characterViewManager;
    [SerializeField] private UIViewManager uiViewManager;
    [SerializeField] private BaseUIManager baseUIManager;
    [SerializeField] private UnityEvent onViewChanged = new UnityEvent();
    public event Action ViewChanged;
    public static ViewManager Instance { get; private set; }
    public BattleDataForOnline Data => battleDataForOnline;

    private void Awake()
    {
        Instance = this;
        if (gridViewManager == null) gridViewManager = GetComponent<GridViewManager>();
        if (characterViewManager == null) characterViewManager = GetComponent<CharacterViewManager>();
        if (uiViewManager == null) uiViewManager = GetComponent<UIViewManager>();
        if (baseUIManager == null) baseUIManager = GetComponent<BaseUIManager>();
    }

    private void OnEnable() { if (battleDataForOnline != null) battleDataForOnline.Changed += ChangeView; }
    private void OnDisable() { if (battleDataForOnline != null) battleDataForOnline.Changed -= ChangeView; }

    public void Bind(BattleDataForOnline data)
    {
        if (battleDataForOnline != null) battleDataForOnline.Changed -= ChangeView;
        battleDataForOnline = data;
        if (battleDataForOnline != null && isActiveAndEnabled) battleDataForOnline.Changed += ChangeView;
    }

    public void ChangeView()
    {
        if (this == null || !isActiveAndEnabled || battleDataForOnline?.State == null) return;
        // Explicit calls avoid BroadcastMessage recursively invoking ChangeView itself.
        gridViewManager?.ChangeView(battleDataForOnline, userData);
        characterViewManager?.ChangeView(battleDataForOnline, userData, characterData);
        uiViewManager?.ChangeView(battleDataForOnline, userData);
        baseUIManager?.ChangeView(battleDataForOnline, userData, characterData);
        ViewChanged?.Invoke();
        onViewChanged.Invoke();
    }

    private void OnDestroy()
    {
        if (battleDataForOnline != null) battleDataForOnline.Changed -= ChangeView;
        if (Instance == this) Instance = null;
    }
}
