using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputData inputData;

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
    }

    void Update()
    {
        // マウス
        inputData.left_mouse_button_ispressed = Mouse.current.leftButton.wasPressedThisFrame;
        inputData.right_mouse_button_ispressed = Mouse.current.rightButton.wasPressedThisFrame;
        inputData.mouse_position = Mouse.current.position.ReadValue();
        inputData.mouse_wheel = Mouse.current.scroll.ReadValue();

        // キーボード（|| で結合して上書きを防ぐ）
        var kb = Keyboard.current;
        if (kb == null) return; // キーボードが接続されていない場合の安全策

        inputData.up_key_ispressed    = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
        inputData.down_key_ispressed  = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
        inputData.right_key_ispressed = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
        inputData.left_key_ispressed  = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
        inputData.space_key_ispressed = kb.spaceKey.wasPressedThisFrame;
    }
}