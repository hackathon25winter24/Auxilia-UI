using UnityEngine;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public Image backImage;
    public Sprite back_image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        backImage.sprite = back_image;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
