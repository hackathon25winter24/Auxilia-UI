using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputData inputData;
    void Start()
    {
        //初期化処理
        inputData.left_mouse_button_ispressed = false;
        inputData.right_mouse_button_ispressed = false;
        inputData.up_key_ispressed = false;
        inputData.down_key_ispressed = false;
        inputData.right_key_ispressed = false;
        inputData.left_key_ispressed = false;
        inputData.space_key_ispressed = false;
    }

    void Update()
    {
        inputData.left_mouse_button_ispressed = Mouse.current.leftButton.wasPressedThisFrame;
        inputData.right_mouse_button_ispressed = Mouse.current.rightButton.wasPressedThisFrame;
        inputData.mouse_position = Mouse.current.position.ReadValue();
        inputData.mouse_wheel = Mouse.current.scroll.ReadValue();

        inputData.up_key_ispressed = Keyboard.current.upArrowKey.wasPressedThisFrame;
        inputData.up_key_ispressed = Keyboard.current.wKey.wasPressedThisFrame;
        inputData.down_key_ispressed = Keyboard.current.downArrowKey.wasPressedThisFrame;
        inputData.down_key_ispressed = Keyboard.current.sKey.wasPressedThisFrame;
        inputData.right_key_ispressed = Keyboard.current.rightArrowKey.wasPressedThisFrame;
        inputData.right_key_ispressed = Keyboard.current.dKey.wasPressedThisFrame;
        inputData.left_key_ispressed = Keyboard.current.leftArrowKey.wasPressedThisFrame;
        inputData.left_key_ispressed = Keyboard.current.aKey.wasPressedThisFrame;
        inputData.space_key_ispressed = Keyboard.current.spaceKey.wasPressedThisFrame;
    }
}
