using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks.Triggers;

public class TutorialBattleStoryManager : MonoBehaviour
{
    [SerializeField] private UserData userData;
    [SerializeField] private InputData inputData;
    [SerializeField] private StoryManagerData storyData;
    [SerializeField] private TutorialBattleStoryView storyView;
    private BattleModel battleModel;
    [SerializeField] private string[] selifs;
    private Coroutine activeRoutine;
    private bool isInit = false; // 初期化されたかのフラグ
    public bool isStoryHidden = false;
    private float lastShowedSerifTime;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float autoWaitTime = 1.0f;

    [Header("Network")]
    private NetworkManager Net => NetworkManager.Instance;
    private AuthenticationConnector authenticationConnector => Net?.Auth;

    public void Init(BattleModel _battleModel, out StoryManagerData _storyData)
    {
        battleModel = _battleModel;
        _storyData = storyData;
        storyView.Init(selifs, storyData, this);

        storyData.serif_number = 0;
        storyData.serif_loading = false;
        storyData.is_auto = false;
        storyData.is_serif = false;

        isStoryHidden = false;
        lastShowedSerifTime = Time.time + 100;

        storyView.ResetUIsAlpha(true, true);
        storyView.ShowStory(true);
        storyView.ShowAutoText(false);
        
        isInit = true;

        // 最初のセリフを開始
        StartSerif();
        storyData.Tutorial_progress = 0;
        storyData.is_serif = true;
    }

    private void Update()
    {
        if(!isInit)
        {
            return;
        }

        storyView.FlashUIs();

        if (inputData.space_key_ispressed)
        {
            OnPlayerClick();
            return;
        }

        if (!isStoryHidden && storyData.is_auto && !storyData.serif_loading)
        {
            if (Time.time - lastShowedSerifTime >= autoWaitTime)
            {
                OnPlayerClick();
                return;
            }
        }

        if(isNextByBattle())
        {
            storyData.serif_number++;
            if (storyData.serif_number < selifs.Length)
            {
                storyView.ShowStory(true);
                StartSerif();
            }
            else
            {
                EndStory();
            }
        }
    }

    private void StartSerif()
    {
        if(storyData.serif_number >= selifs.Length)
        {
            EndStory();
            return;
        }

        if(activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }
        activeRoutine = StartCoroutine(StoryFlowRoutine());
    }

    public IEnumerator StoryFlowRoutine()
    {
        // 配列から現在の文字列を取得
        string currentText = selifs[storyData.serif_number];

        // タイピング演出
        storyData.serif_loading = true;
        string storyText = "";
        storyView.SetStoryText(storyText);
        foreach (char letter in currentText.ToCharArray())
        {
            storyText += letter;
            storyView.SetStoryText(storyText);
            yield return new WaitForSeconds(typingSpeed);
        }

        lastShowedSerifTime = Time.time;
        storyData.serif_loading = false;
    }

    public void OnPlayerClick()
    {
        if (storyData.serif_loading)
        {
            // セリフ中の場合、即座に全文を表示する
            StopCoroutine(activeRoutine);
            lastShowedSerifTime = Time.time;
            storyData.serif_loading = false;
            storyView.SetStoryText(selifs[storyData.serif_number]);
            return;
        }

        if(!canNextStoryByClick())
        {
            storyView.ShowStory(false);
            return;
        }

        storyData.serif_number++;
        if (storyData.serif_number < selifs.Length)
        {
            storyView.ShowStory(true);
            StartSerif();
        }
        else
        {
            EndStory();
        }
    }

    private bool isNextByBattle()
    {
        if(!isStoryHidden) return false;

        int n = storyData.serif_number;

        // キャラ選択
        if(n == 2)
        {
            return battleModel.selected != -1;
        }

        // 移動
        if(n == 6)
        {
            return battleModel.player1.remainingCost <= 0;
        }

        // ターン終了
        if(n == 9)
        {
            return !battleModel.is1pTurn;
        }

        // ゲーム終了
        if(n == 14)
        {
            return battleModel.player2.baseHp < 200;
        }

        return false;
    }

    private bool canNextStoryByClick()
    {
        int n = storyData.serif_number;
        if(n == 2 || n == 6 || n == 9 || n == 14)
        {
            return false;
        }

        return true;
    }

    async private void EndStory()
    {
        if(userData.story_progress == 1)
        {
            userData.story_progress = 2;
            if (authenticationConnector != null) await authenticationConnector.UpdateUser();
            SceneChangeManager.MoveScene(1);
        }else
        {
            SceneChangeManager.MoveScene(11);
        }
    }

    
    public void ToggleAutoMode()
    {
        SEManager.instance.PlaySelectSE();
        storyData.is_auto = !storyData.is_auto;
        storyView.ShowAutoText(storyData.is_auto);
        storyView.ResetUIsAlpha(false, true);
    }

    public void SkipStory()
    {
        SEManager.instance.PlayToNextSE();
        EndStory();
    }
}