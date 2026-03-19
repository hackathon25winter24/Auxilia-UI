using UnityEngine;

[CreateAssetMenu(fileName = "InputData", menuName = "Scriptable Objects/InputData")]
public class InputData : ScriptableObject
{
    public bool left_mouse_button_ispressed;
    public bool right_mouse_button_ispressed;
    public Vector2 mouse_wheel;
    public Vector2 mouse_position;
    public bool up_key_ispressed;
    public bool down_key_ispressed;
    public bool right_key_ispressed;
    public bool left_key_ispressed;
    public bool space_key_ispressed;
    public bool a_key_ispressed;
    public bool s_key_ispressed;
}
