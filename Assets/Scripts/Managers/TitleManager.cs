using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public Image targetImage;  // 表示先のUI Image
    public Sprite newSprite;   // 切り替えたいPNG画像（Sprite）
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetImage.sprite = newSprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputData.left_mouse_button_ispressed == true || inputData.right_mouse_button_ispressed == true)
        {
            sceneData.next_scene_number = 1;
        }
    }
}
