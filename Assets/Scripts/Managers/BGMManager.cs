using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    public AudioSource audioSource;
    public AudioClip titleBGM;
    public AudioClip homeBGM;
    public AudioClip selectBGM;
    public AudioClip battleBGM;

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
        audioSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded");
        //StopBGM();
        // ここにStopBGMを入れてもいいけれど無音のシーンが誕生します
        // 全てのシーンに対応するBGMができたらここに入れた方が良さそう
        if(scene.name == "TitleScene")
        {
            StopBGM();// なくてもいけるけど念の為
            PlayTitleBGM();
        }
        else if(scene.name == "HomeScene")
        {
            StopBGM();
            PlayHomeBGM();
        }
        else if(scene.name == "CharacterSelectScene" || scene.name == "MatchingScene")
        {
            StopBGM();
            PlaySelectBGM();
        }
        else if(scene.name == "TutorialBattleScene" || scene.name == "BattleScene")
        {
            StopBGM();
            PlayBattleBGM();
        }
        else if(scene.name == "ResultScene")
        {
            StopBGM();// ResultSceneではWinSEかDefeatSE（未定）をSEとして流す
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
        if (audioSource.clip == homeBGM) return;
        audioSource.clip = homeBGM;
        audioSource.Play();
    }
    public void PlaySelectBGM()
    {
        if (audioSource.clip == selectBGM) return;
        audioSource.clip = selectBGM;
        audioSource.Play();
    }
    public void PlayBattleBGM()
    {
        if (audioSource.clip == battleBGM) return;
        audioSource.clip = battleBGM;
        audioSource.Play();
    }
    public void StopBGM()
    {
        audioSource.Stop();
        audioSource.clip = null; // 現在の曲情報をリセット
    }
}
