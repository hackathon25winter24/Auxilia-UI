using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public InputData inputData;

    private RectTransform canvasRect;

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
    inputData.left_mouse_button_ispressed = Mouse.current.leftButton.wasPressedThisFrame;
    inputData.right_mouse_button_ispressed = Mouse.current.rightButton.wasPressedThisFrame;

    //マウス座標（キャンバス座標に揃える）
    Vector2 rawMousePos = Mouse.current.position.ReadValue();
    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, rawMousePos, null, out inputData.mouse_position);

    inputData.mouse_wheel = Mouse.current.scroll.ReadValue();

    // キーボード（以下略）
    var kb = Keyboard.current;
    if (kb == null) return;

    inputData.up_key_ispressed    = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
    inputData.down_key_ispressed  = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
    inputData.right_key_ispressed = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
    inputData.left_key_ispressed  = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
    inputData.space_key_ispressed = kb.spaceKey.wasPressedThisFrame;
    inputData.a_key_ispressed = kb.aKey.wasPressedThisFrame;
    inputData.s_key_ispressed = kb.sKey.wasPressedThisFrame;
}
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}