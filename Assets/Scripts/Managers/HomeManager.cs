using UnityEngine;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public Image backImage;
    public Sprite back_image;
    public Image CharacterImage; 

    public Sprite Sophie;
    public Sprite Shincho;
    public Sprite Aoi;
    public Sprite Berenice;
    public Sprite Chiyo;
    public Sprite Jude;
    public Sprite Nadia;
    public Sprite Sena;
    public Sprite Tsukiha;
    public Sprite Zina;
    public int character_number = 0;

    public void ChangeToPhoto()
    {
        if(character_number == 0)
        {
            CharacterImage.sprite = Sophie;
        }
        if(character_number == 1)
        {
            CharacterImage.sprite = Shincho;
        }
        if(character_number == 2)
        {
            CharacterImage.sprite = Aoi;
        }
        if(character_number == 3)
        {
            CharacterImage.sprite = Berenice;
        }
        if(character_number == 4)
        {
            CharacterImage.sprite = Chiyo;
        }
        if(character_number == 5)
        {
            CharacterImage.sprite = Jude;
        }
        if(character_number == 6)
        {
            CharacterImage.sprite = Nadia;
        }
        if(character_number == 7)
        {
            CharacterImage.sprite = Sena;
        }
        if(character_number == 8)
        {
            CharacterImage.sprite = Tsukiha;
        }
        if(character_number == 9)
        {
            CharacterImage.sprite = Zina;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        backImage.sprite = back_image;
        ChangeToPhoto();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
