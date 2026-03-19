using UnityEngine;

[CreateAssetMenu(fileName = "StoryCharacterData", menuName = "Scriptable Objects/StoryCharacterData")]
public class StoryCharacterData : ScriptableObject
{
    [SerializeField]public CharactersImage[] charactersData;
}

[System.Serializable]
public class CharactersImage
{
    public string character_name;
    public Sprite[] character_face;
}