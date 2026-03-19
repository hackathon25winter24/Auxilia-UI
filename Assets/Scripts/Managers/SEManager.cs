using UnityEngine;

public class SEandBGMManager : MonoBehaviour
{
    public InputData inputData;
    public AudioSource audioSource;
    public AudioClip clickSE;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void UpDate()
    {
        if (inputData.left_mouse_button_ispressed || inputData.right_mouse_button_ispressed)
        {
            PlaySE();
        }
    }

    public void PlaySE()
    {
        // PlayOneShotを使うと、音が重なっても途切れずに最後まで鳴ります
        audioSource.PlayOneShot(clickSE);
    }
}
