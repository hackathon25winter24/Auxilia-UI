using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    public AudioSource audioSource;
    public AudioClip titleBGM;
    public AudioClip HomeBGM;
    public AudioClip SelectBGM;
    public AudioClip WinSE;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded");
        //StopBGM();
        // ここにStopBGMを入れてもいいけれど無音のシーンが誕生します
        // 全てのシーンに対応するBGMができたらここに入れた方が良さそう
        if(scene.name == "TitleScene")
        {
            //StopBGM();
            PlayTitleBGM();
        }
        else if(scene.name == "HomeScene")
        {
            //StopBGM();
            PlayHomeBGM();
        }
        else if(scene.name == "CharacterSelectScene" || scene.name == "MatchingScene")
        {
            //StopBGM();
            PlaySelectBGM();
        }
        else if(scene.name == "ResultScene")
        {
            //StopBGM();
            PlayWinSE();
        }
    }

    public void PlayTitleBGM()
    {
        if (audioSource.clip == titleBGM) return;
        audioSource.clip = titleBGM;
        audioSource.Play();
    }
    public void PlayHomeBGM()
    {
        if (audioSource.clip == HomeBGM) return;
        audioSource.clip = HomeBGM;
        audioSource.Play();
    }
    public void PlaySelectBGM()
    {
        if (audioSource.clip == SelectBGM) return;
        audioSource.clip = SelectBGM;
        audioSource.Play();
    }
    public void PlayWinSE()
    {
        if (audioSource.clip == WinSE) return;
        audioSource.clip = WinSE;
        audioSource.Play();
    }
    public void StopBGM()
    {
        audioSource.Stop();
        audioSource.clip = null; // 現在の曲情報をリセット
    }
}
