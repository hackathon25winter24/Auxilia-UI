using UnityEngine;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public Image matchingImage;
    public Sprite matching_iamge; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        matchingImage.sprite = matching_iamge;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
