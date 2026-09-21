using Unity.VisualScripting;
using UnityEngine;

public class TutorialBattleStoryManager : MonoBehaviour
{
    [SerializeField] private InputData inputData;
    [SerializeField] private StoryManagerData storyData;
    [SerializeField] private TutorialBattleStoryView storyView;
    private BattleModel battleModel;
    [SerializeField] private string[] selifs;
    private Coroutine activeRoutine;
    private bool isInit = false; // 初期化されたかのフラグ

    [Header("Network")]
    private NetworkManager Net => NetworkManager.Instance;
    private AuthenticationConnector authenticationConnector => Net?.Auth;

    private void Start()
    {
        storyData.serif_number = 0;
        storyData.serif_loading = false;
        storyData.is_auto = false;
        storyData.is_serif = false;

        storyView.ShowStory();
        storyView.HideAutoText();

        // 最初のセリフを開始
        //StartNewSerif();
        //storyManagerData.Tutorial_progress = 0;
        //storyManagerData.is_serif = true;
    }

    private void Update()
    {
        if(!isInit)
        {
            return;
        }

        storyView.FlashUIs();
    }

    public void Init(BattleModel _battleModel)
    {
        battleModel = _battleModel;
        
        isInit = true;
    }

    public void OnPlayerClick()
    {
        
    }

    private bool isNextByBattle()
    {
        return false;
    }

    private void StartSerif()
    {
        
    }

    private void EndStory()
    {
        
    }

    
    public void ToggleAutoMode()
    {
        storyData.is_auto = !storyData.is_auto;
    }

    public void SkipStory()
    {
        EndStory();
    }
}