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
    // マウスボタン
    inputData.left_mouse_button_ispressed = Mouse.current.leftButton.wasPressedThisFrame;
    inputData.right_mouse_button_ispressed = Mouse.current.rightButton.wasPressedThisFrame;

    //マウス座標（カメラ中心を原点にする修正）
    Vector2 rawMousePos = Mouse.current.position.ReadValue();
    Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    inputData.mouse_position = rawMousePos - screenCenter; 

    inputData.mouse_wheel = Mouse.current.scroll.ReadValue();

    // キーボード（以下略）
    var kb = Keyboard.current;
    if (kb == null) return;

    inputData.up_key_ispressed    = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
    inputData.down_key_ispressed  = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
    inputData.right_key_ispressed = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
    inputData.left_key_ispressed  = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
    inputData.space_key_ispressed = kb.spaceKey.wasPressedThisFrame;
}
}