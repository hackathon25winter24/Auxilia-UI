using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeManager : MonoBehaviour
{
    public TextMeshProUGUI HomeCharacterName;
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
        if(playerData.home_character_ID == 0)
        {
            CharacterImage.sprite = Sophie;
            HomeCharacter_icon.sprite = First_squad;
            Player_icon.sprite = First_squad_user;
            HomeCharacterName.text = "ソフィー・マヤ・フローレス";
        }
        if(playerData.home_character_ID == 1)
        {
            CharacterImage.sprite = Jude;
            HomeCharacter_icon.sprite = First_squad;
            Player_icon.sprite = First_squad_user;
            HomeCharacterName.text = "ジュード・アーノルド・ダガー";
        }
        if(playerData.home_character_ID == 2)
        {
            CharacterImage.sprite = Nadia;
            HomeCharacter_icon.sprite = Second_squad;
            Player_icon.sprite = Second_squad_user;
            HomeCharacterName.text = "ナディア・ミレーヌ・エイギーユ・ドゥ・メデゥシーヌ";
        }
        if(playerData.home_character_ID == 3)
        {
            CharacterImage.sprite = Tsukiha;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "三雲 月葉(みくも つきは)";
        }
        if(playerData.home_character_ID == 4)
        {
            CharacterImage.sprite = Aoi;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "藤原 扇衣(ふじわら あおい)";
        }
        if(playerData.home_character_ID == 5)
        {
            CharacterImage.sprite = Sena;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "一条 星凪(いちじょう せな)";
        }
        if(playerData.home_character_ID == 6)
        {
            CharacterImage.sprite = Berenice;
            HomeCharacter_icon.sprite = Second_squad;
            Player_icon.sprite = Second_squad_user;
            HomeCharacterName.text = "ベレニス・イネス・マルヴェランス";
        }
        if(playerData.home_character_ID == 7)
        {
            CharacterImage.sprite = Chiyo;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "望月 千代(もちつき ちよ)";
        }
        if(playerData.home_character_ID == 8)
        {
            CharacterImage.sprite = Shincho;
            HomeCharacter_icon.sprite = Third_squad;
            Player_icon.sprite = Third_squad_user;
            HomeCharacterName.text = "新著 久無子(しんちょ くなし)";
        }
        if(playerData.home_character_ID == 9)
        {
            CharacterImage.sprite = Zina;
            HomeCharacter_icon.sprite = Fourth_squad;
            Player_icon.sprite = Fourth_squad_user;
            HomeCharacterName.text = "ジナイダ・ヤカーヴナ・ストレリツォーヴァ";
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
