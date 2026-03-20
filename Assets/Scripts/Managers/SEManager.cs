using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SEManager : MonoBehaviour
{
    public static SEManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SEManager>();
            }
            return _instance;
        }
    }
    private static SEManager _instance;
    public InputData inputData;
    public AudioSource audioSource;
    public AudioClip selectSE;
    public AudioClip clickSE;
    public AudioClip backSE;
    public AudioClip toNextSE;
    public AudioClip startTurnSE;
    public AudioClip damageSE;
    public AudioClip healSE;
    public AudioClip WinSE;
    public AudioClip DefeatSE;

    private GameObject obj;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        if (inputData.left_mouse_button_ispressed || inputData.right_mouse_button_ispressed)
        {
            obj = EventSystem.current.currentSelectedGameObject;
            if (obj != null)
            {
                if (obj.GetComponent<UnityEngine.UI.Button>())
                {
                    return;// ボタンは個別で音を流す
                }
                else if (obj.GetComponent<TMPro.TMP_InputField>())
                {
                    PlaySelectSE();
                }
                else
                {
                    //PlayClickSE(); // 会話シーンとかホームキャラ選択で鳴るのうざかったので一旦消す
                }
            }
            else
            {
                //PlayClickSE();
            }
        }
    }

    public void PlaySelectSE()
    {
        // PlayOneShotを使うと、音が重なっても途切れずに最後まで鳴ります
        audioSource.PlayOneShot(selectSE);
    }
    public void PlayClickSE()// 通常クリックした時のSE
    {
        audioSource.PlayOneShot(clickSE);
    }
    public void PlayBackSE()
    {
        audioSource.PlayOneShot(backSE);
    }
    public void PlayToNextSE()
    {
        audioSource.PlayOneShot(toNextSE);
    }
    public void PlayStartTurnSE()
    {
        audioSource.PlayOneShot(startTurnSE);
    }
    public void PlayDamageSE()
    {
        audioSource.PlayOneShot(damageSE);
    }
    public void PlayHealSE()
    {
        audioSource.PlayOneShot(healSE);
    }
    public void PlayWinSE()
    {
        audioSource.PlayOneShot(WinSE);
    }
    public void PlayDefeatSE()
    {
        audioSource.PlayOneShot(DefeatSE);
    }
}
