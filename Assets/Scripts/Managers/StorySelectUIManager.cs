using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorySelectUIManager : MonoBehaviour
{
    public SceneData sceneData;
    public StoryData storyData;
    public StoryManagerData storyManagerData;

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
            sceneData.next_scene_number = 1;
                break;
            case "Tutorial":
            storyManagerData.now_story_number = 0;
            sceneData.next_scene_number = 8;
                break;
            default:
                break;
        }
    }
}
