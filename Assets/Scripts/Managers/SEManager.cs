using UnityEngine;
using UnityEngine.EventSystems;

public class SEManager : MonoBehaviour
{
    public InputData inputData;
    public AudioSource audioSource;
    public AudioClip inputSE;
    public AudioClip clickSE;
    public AudioClip backSE;
    public AudioClip toNextSE;

    private GameObject obj;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Update()
    {
        if (inputData.left_mouse_button_ispressed || inputData.right_mouse_button_ispressed)
        {
            Debug.Log("Click");
            obj = EventSystem.current.currentSelectedGameObject;
            if (obj != null)
            {
                if (obj.GetComponent<UnityEngine.UI.Button>())
                {
                    Debug.Log("Button");
                    return;// ボタンは個別で音を流す
                }
                else if (obj.GetComponent<TMPro.TMP_InputField>())
                {
                    Debug.Log("TMP_InputField");
                    PlayInputSE();
                }
                else
                {
                    PlayClickSE();
                }
            }
            else
            {
                PlayClickSE();
            }
        }
    }

    public void PlayInputSE()
    {
        // PlayOneShotを使うと、音が重なっても途切れずに最後まで鳴ります
        audioSource.PlayOneShot(inputSE);
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
}
