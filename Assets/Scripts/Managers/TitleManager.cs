using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public Image titleImage;
    public Image rogoImage;
    public Image tap_to_startImage;
    public Sprite title_image;
    public Sprite rogo_image;
    public Sprite tap_to_start_image;
    public Image targetImage; 
    public float speed = 5.0f;
    public GameObject targetUI;
    void Start()
    {
        titleImage.sprite = title_image;
        rogoImage.sprite = rogo_image;
        tap_to_startImage.sprite = tap_to_start_image;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputData.left_mouse_button_ispressed == true || inputData.right_mouse_button_ispressed == true)
        {
            sceneData.next_scene_number = 1;
            targetUI.SetActive(false);
        }
        if (targetImage == null) return;

        // サイン波を使用して 0.0 〜 1.0 の値を作成
        // 公式: alpha = (sin(時間 * 速度) + 1) / 2
        float alpha = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;

        // 色を取得してアルファ値を更新し、再代入
        Color c = targetImage.color;
        c.a = alpha;
        targetImage.color = c;
    }
}
