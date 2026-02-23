using UnityEngine;

public class MatchingUIManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;

    void Start()
    {
        
    }

    public void OnButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                sceneData.next_scene_number = 1;
                Debug.Log("battle button was plessed");
                break;
            case "Story":
                Debug.Log("story button was plessed");
                break;
            case "Character":
                break;
            default:
                Debug.Log("不明なボタン: " + buttonName);
                break;
        }
    }
}
