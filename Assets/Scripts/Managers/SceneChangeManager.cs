using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    public SceneData sceneData;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (sceneData.now_scene_number != sceneData.next_scene_number)
        {
            SceneManager.LoadScene(sceneData.next_scene_number);
            sceneData.now_scene_number = sceneData.next_scene_number;
        }
    }
}
