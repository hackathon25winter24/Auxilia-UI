using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeManager : MonoBehaviour
{
    public TextMeshProUGUI HomeCharacterName;
    public InputData inputData;
    public SceneData sceneData;
    public UserData userData;
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
    public Sprite Dana;
    public Image Player_icon;
    public Image HomeCharacter_icon;
    public Sprite First_squad_user;
    public Sprite Second_squad_user;
    public Sprite Third_squad_user;
    public Sprite Fourth_squad_user;
    public Sprite First_squad;
    public Sprite Second_squad;
    public Sprite Third_squad;
    public Sprite Fourth_squad;

    public void ChangeToPhoto()
    {
        if(userData.home_character_id == 0)
        {
            CharacterImage.sprite = Sophie;
            HomeCharacter_icon.sprite = First_squad;
            Player_icon.sprite = First_squad_user;
            HomeCharacterName.text = "ソフィー・マヤ・フローレス";
        }
        if(userData.home_character_id == 1)
        {
            CharacterImage.sprite = Jude;
            HomeCharacter_icon.sprite = First_squad;
            Player_icon.sprite = First_squad_user;
            HomeCharacterName.text = "ジュード・アーノルド・ダガー";
        }
        if(userData.home_character_id == 2)
        {
            CharacterImage.sprite = Nadia;
            HomeCharacter_icon.sprite = Second_squad;
            Player_icon.sprite = Second_squad_user;
            HomeCharacterName.text = "ナディア・ミレーヌ・エイギーユ・ドゥ・メデゥシーヌ";
        }
        if(userData.home_character_id == 3)
        {
            CharacterImage.sprite = Tsukiha;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "三雲 月葉";
        }
        if(userData.home_character_id == 4)
        {
            CharacterImage.sprite = Aoi;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "藤原 扇衣";
        }
        if(userData.home_character_id == 5)
        {
            CharacterImage.sprite = Sena;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "一条 星凪";
        }
        if(userData.home_character_id == 6)
        {
            CharacterImage.sprite = Berenice;
            HomeCharacter_icon.sprite = Second_squad;
            Player_icon.sprite = Second_squad_user;
            HomeCharacterName.text = "ベレニス・イネス・マルヴェランス";
        }
        if(userData.home_character_id == 7)
        {
            CharacterImage.sprite = Chiyo;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "望月 千代";
        }
        if(userData.home_character_id == 8)
        {
            CharacterImage.sprite = Shincho;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "新著 久無子";
        }
        if(userData.home_character_id == 9)
        {
            CharacterImage.sprite = Zina;
            HomeCharacter_icon.sprite = Fourth_squad;
            Player_icon.sprite = Fourth_squad_user;
            HomeCharacterName.text = "ジナイダ・ヤカーヴナ・ストレリツォーヴァ";
        }

         if(userData.home_character_id == 10)
        {
            CharacterImage.sprite = Dana;
            HomeCharacter_icon.sprite = Fourth_squad;
            Player_icon.sprite = Fourth_squad_user;
            HomeCharacterName.text = "ダニール・アトラーヴィチ・カラマーゾフ";
        }
    }

    void Awake()
    {
        backImage.sprite = back_image;
        ChangeToPhoto();
    }

    void Update()
    {
        ChangeToPhoto();
    }
}
