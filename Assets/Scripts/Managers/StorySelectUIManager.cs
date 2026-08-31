using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorySelectUIManager : MonoBehaviour
{
    public StoryData storyData;
    public StoryManagerData storyManagerData;

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
            SceneChangeManager.MoveScene(1);
                break;
            case "Tutorial":
            storyManagerData.now_story_number = 0;
            SceneChangeManager.MoveScene(8);
                break;
            default:
                break;
        }
    }
}
