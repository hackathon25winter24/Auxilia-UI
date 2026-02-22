using UnityEngine;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
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

    public void ChangeToPhoto()
    {
        if(playerData.home_character_ID == 0)
        {
            CharacterImage.sprite = Sophie;
        }
        if(playerData.home_character_ID == 1)
        {
            CharacterImage.sprite = Jude;
        }
        if(playerData.home_character_ID == 2)
        {
            CharacterImage.sprite = Nadia;
        }
        if(playerData.home_character_ID == 3)
        {
            CharacterImage.sprite = Tsukiha;
        }
        if(playerData.home_character_ID == 4)
        {
            CharacterImage.sprite = Aoi;
        }
        if(playerData.home_character_ID == 5)
        {
            CharacterImage.sprite = Sena;
        }
        if(playerData.home_character_ID == 6)
        {
            CharacterImage.sprite = Berenice;
        }
        if(playerData.home_character_ID == 7)
        {
            CharacterImage.sprite = Chiyo;
        }
        if(playerData.home_character_ID == 8)
        {
            CharacterImage.sprite = Shincho;
        }
        if(playerData.home_character_ID == 9)
        {
            CharacterImage.sprite = Zina;
        }
    }

    void Awake()
    {
        backImage.sprite = back_image;
        ChangeToPhoto();
    }

    void Update()
    {
        
    }
}
