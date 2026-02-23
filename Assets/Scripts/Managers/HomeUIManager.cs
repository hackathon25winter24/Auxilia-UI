using UnityEngine;

public class HomeUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Battle":
                sceneData.next_scene_number = 3;
                Debug.Log("battle button was plessed");
                break;
            case "Story":
                Debug.Log("story button was plessed");
                break;
            case "Character":
                sceneData.next_scene_number = 7;
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
