using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneChangeManager : MonoBehaviour
{
    public SceneData sceneData;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        sceneData.now_scene_number = 0;
        sceneData.next_scene_number = 0;
    }
    void Update()
    {
        if (sceneData.now_scene_number != sceneData.next_scene_number)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneData.next_scene_number);
            sceneData.now_scene_number = sceneData.next_scene_number;
        }
    }
}
