using UnityEngine;

[CreateAssetMenu(fileName = "StoryData", menuName = "Scriptable Objects/StoryData")]
public class StoryData : ScriptableObject
{
    [SerializeField] public StoriesData[] stories;
}

[System.Serializable]
public class StoriesData
{
    public int story_number;
    public int next_scene;
    public SerifData[] serifs;
}

[System.Serializable]
public class SerifData
{
    public int characterID;
    public int character_face;
    public int character_move;
    public float character_size;
    public bool is_shadowed;
    public string name;
    [TextArea(3, 10)]
    public string serif;
}