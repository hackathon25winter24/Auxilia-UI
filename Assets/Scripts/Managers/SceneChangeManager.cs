using UnityEngine;

public static class SceneChangeManager
{
    public static void MoveScene(int scene_num)
    {
        if (scene_num >= 0 && scene_num < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene_num);
        }
        else
        {
            Debug.LogError($"不明なシーン番号 {scene_num} ");
        }
    }

    // 現在のシーンをリロードする
    public static void ReloadScene()
    {
        int scene_num = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene_num);
    }
}
