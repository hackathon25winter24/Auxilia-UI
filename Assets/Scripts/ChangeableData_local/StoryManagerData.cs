using UnityEngine;

[CreateAssetMenu(fileName = "StoryManagerData", menuName = "Scriptable Objects/StoryManagerData")]
public class StoryManagerData : ScriptableObject
{
    public int serif_number;
    public bool serif_loading;
    public bool is_auto;
    public bool is_wating;
    public bool is_serif;
    public int now_story_number;
    public int Tutorial_progress;
}
