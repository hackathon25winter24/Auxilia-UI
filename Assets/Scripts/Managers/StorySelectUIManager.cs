using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorySelectUIManager : MonoBehaviour
{
    public SceneData sceneData;

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
            sceneData.next_scene_number = 1;
                break;
            default:
                break;
        }
    }
}
