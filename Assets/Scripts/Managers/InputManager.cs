using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    private RectTransform canvasRect;
    public event Action OnMouseRightButtonClicked;
    public event Action OnMouseLeftButtonClicked;
    public event Action OnUpKeyClicked;
    public event Action OnDownKeyClicked;
    public event Action OnRightKeyClicked;
    public event Action OnLeftKeyClicked;
    public event Action OnWKeyClicked;
    public event Action OnAKeyClicked;
    public event Action OnSKeyClicked;
    public event Action OnDKeyClicked;
    public event Action OnSpaceKeyClicked;
    private void Awake()
    {
        // 重複チェック
        InputManager[] instances = FindObjectsByType<InputManager>(FindObjectsSortMode.None);
        if (instances.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        canvasRect = FindFirstObjectByType<Canvas>().GetComponent<RectTransform>();
    }

    void Update()
{
    // マウスボタン
    if(Mouse.current.leftButton.wasPressedThisFrame) OnMouseLeftButtonClicked?.Invoke();
    if(Mouse.current.rightButton.wasPressedThisFrame) OnMouseRightButtonClicked?.Invoke();
    // キーボード（以下略）
    var kb = Keyboard.current;
    if (kb == null) return;

    if(kb.upArrowKey.wasPressedThisFrame) OnUpKeyClicked?.Invoke();
    if(kb.downArrowKey.wasPressedThisFrame) OnDownKeyClicked?.Invoke();
    if(kb.rightArrowKey.wasPressedThisFrame) OnRightKeyClicked?.Invoke();
    if(kb.leftArrowKey.wasPressedThisFrame) OnLeftKeyClicked?.Invoke();
    if(kb.wKey.wasPressedThisFrame) OnWKeyClicked?.Invoke();
    if(kb.sKey.wasPressedThisFrame) OnSKeyClicked?.Invoke();
    if(kb.dKey.wasPressedThisFrame) OnDKeyClicked?.Invoke();
    if(kb.aKey.wasPressedThisFrame) OnAKeyClicked?.Invoke();
    if(kb.spaceKey.wasPressedThisFrame) OnSpaceKeyClicked?.Invoke();
}
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}