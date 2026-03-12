using UnityEngine;

public class BattleOnlineManager : MonoBehaviour
{
    public InputData inputData;
    public SceneData sceneData;
    public PlayerData playerData;
    void Update()
    {
        if (inputData.space_key_ispressed)
        {
            sceneData.next_scene_number = 6;
        }
    }
}
