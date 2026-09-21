using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class BattleManager : MonoBehaviour
{
    [Header("クラス")]
    [SerializeField] InputData inputData;
    private BattleModel battleModel;
    [SerializeField] CharacterData characterData;
    [SerializeField] BattleView battleView;
    [SerializeField] GridView gridView;
    private AttackManager attackManager;
    private EnemyManager enemyManager;
    [SerializeField] StoryManagerData storyManagerData;
    [SerializeField] TutorialBattleStoryManager tutorialBattleStoryManager;

    [Space]

    [Header("初期状態")]
    [SerializeField] DefaultCharactersData[] player1CharactersData;
    [SerializeField] DefaultCharactersData[] player2CharactersData;
    [SerializeField] Transform gridsParentTs;
    [SerializeField] GameObject gridPrefab;
    
    private void Awake()
    {
        InitBattleModel();
        attackManager = new AttackManager(inputData, battleModel);
        enemyManager = new EnemyManager(this, battleModel, battleView);
        tutorialBattleStoryManager.Init(battleModel);

        InitGrids();
        InitCharacters();
        battleView.Init(battleModel.player1.playerName, battleModel.player2.playerName, battleModel.player1.maxCost, this, battleModel);
        TimerStop();
    }

    private void InitBattleModel()
    {
        battleModel = new BattleModel(player1CharactersData, player2CharactersData, characterData);

        battleModel.player1.BaseHpChanged += BaseHpChanged1;
        battleModel.player2.BaseHpChanged += BaseHpChanged2;

        battleModel.player1.RemainingCostChanged += RemainingCostChanged1;
        battleModel.player2.RemainingCostChanged += RemainingCostChanged2;

        battleModel.RemainingTimeChanged += RemainingTimeChanged;

        foreach (var character in battleModel.player1.characters)
        {
            if(character == null) continue;
            character.hpChanged += CharacterHpChanged;
        }
        foreach (var character in battleModel.player2.characters)
        {
            if(character == null) continue;
            character.hpChanged += CharacterHpChanged;
        }
    }

    private void Update()
    {
        if(battleModel.is1pTurn)
        {
            if(!battleModel.isAttack) Move();
            else
            {
                attackManager.Attack();
                
                // attackGridの表示を更新
                for(int x = 0; x < 8; x++)
                {
                    for(int y = 0; y < 5; y++)
                    {
                        if(battleModel.isAttackGrid[x,y])
                        {
                            gridView.UpdateGridImage(x, y, GridStateEnum.Attack);
                        }
                        else
                        {
                            gridView.UpdateGridImage(x, y, battleModel.nowGridState[x,y]);
                        }
                    }
                }
            }

            if (inputData.space_key_ispressed)
            {
                EndTurn();
            }
        }
        else
        {
            
        }

        if(battleModel.isTimerRunning)
        {
            UpdateTimer();
        }
    }

    private void InitGrids()
    {
        for(int x = 0; x < 8; x++)
        {
            for(int y = 0; y < 5; y++)
            {
                Vector2 pos = new Vector2(x * 50 - 175, y * -50 + 60);
                GameObject obj = Instantiate(gridPrefab, gridsParentTs);
                obj.transform.localPosition = pos;

                gridView.Init(x, y, obj.GetComponent<Image>());
                UpdateGridState(new Vector2Int(x, y), battleModel.nowGridState[x,y]);
            }
        }
    }

    private void InitCharacters()
    {
        for (int i = 0; i < player1CharactersData.Length; i++)
        {
            var character = battleModel.player1.characters[i];
            if(character == null) continue;
            InitCharacter(character, player1CharactersData[i].smallWindow, true, i);
        }

        for (int i = 0; i < player2CharactersData.Length; i++)
        {
            var character = battleModel.player2.characters[i];
            if(character == null) continue;
            InitCharacter(character, player2CharactersData[i].smallWindow, false, i);
        }
    }

    private void InitCharacter(BattleModel.CharacterBattleData character, GameObject smallWindow, bool isPlayer1, int index)
    {
        // CharactersDataをidから検索してviewの初期化
        foreach (var chData in characterData.characters)
        {
            if(chData.default_id == character.id)
            {
                character.view.Init(chData, this, smallWindow, isPlayer1, index);
                break;
            }
        }

        // 位置の初期化
        character.view.SetPosition(character.position, false);
    }

    public void SelectedCharacter(bool is1p, int charactedIndex)
    {
        if (battleModel.isAttack) return;

        if(is1p && battleModel.is1pTurn)
        {
            if (battleModel.selected != -1)
            {
                var selectedCharacter = battleModel.player1.characters[battleModel.selected];
                if(selectedCharacter != null)
                {
                    selectedCharacter.view.HideAttackWindow();
                    UpdateGridState(selectedCharacter.position, GridStateEnum.Character);
                }
            }

            battleModel.selected = charactedIndex;

            // 選択したキャラクターの攻撃ボタンを表示
            if(charactedIndex != -1)
            {
                var selectedCharacter = battleModel.player1.characters[charactedIndex];
                if(selectedCharacter != null)
                {
                    UpdateGridState(selectedCharacter.position, GridStateEnum.Selected);
                    selectedCharacter.view.ShowAttackWindow();
                }
            }
        }
        else
        {
            battleModel.selected = -1;
        }
    }

    private void Move()
    {
        if(battleModel.selected != -1)
        {
            var selectedCharacter = battleModel.player1.characters[battleModel.selected];
            var movedPos = selectedCharacter.position;
                
            if(inputData.left_key_ispressed) movedPos.x--;
            if(inputData.right_key_ispressed) movedPos.x++;
            if(inputData.up_key_ispressed) movedPos.y--;
            if(inputData.down_key_ispressed) movedPos.y++;
            
            if(movedPos == selectedCharacter.position) return;

            if(0 <= movedPos.x && movedPos.x < 8 && 0 <= movedPos.y && movedPos.y < 5)
            {
                var state = battleModel.nowGridState[movedPos.x, movedPos.y];
                if(state != GridStateEnum.Prohibit && state != GridStateEnum.Base && state != GridStateEnum.Character && state != GridStateEnum.Stop)
                {
                    if(battleModel.player1.remainingCost < selectedCharacter.moveCost)
                    {
                        // コストが足りなかった時の効果音
                        SEManager.instance.PlayBackSE(); // 仮
                        return;
                    }

                    UpdateGridState(selectedCharacter.position, battleModel.defaultGridState[selectedCharacter.position.x, selectedCharacter.position.y]);
                    UpdateGridState(movedPos, GridStateEnum.Selected);
                    selectedCharacter.position = movedPos;
                    selectedCharacter.view.SetPosition(movedPos, true);

                    battleModel.player1.remainingCost -= selectedCharacter.moveCost;
                }
            }
        }
    }

    public void AttackButtonClicked(int attackIndex)
    {
        if(battleModel.selected != -1)
        {
            var selectedCharacter = battleModel.player1.characters[battleModel.selected];
            if(selectedCharacter != null)
            {
                battleModel.isAttack = true;
                selectedCharacter.view.HideAttackWindow();
                attackManager.SetAttack(battleModel.is1pTurn, selectedCharacter.attacks[attackIndex], selectedCharacter.position);
            }
        }
    }

    public void EndTurn()
    {
        if (battleView.IsMoveRoutineRunning)
        {
            return;
        }

        battleModel.is1pTurn = !battleModel.is1pTurn;
        battleModel.selected = -1;
        battleModel.isAttack = false;
        battleModel.remainingTime = battleModel.maxTime;

        void resetView(BattleModel.CharacterBattleData character)
        {
            if(character != null)
            {
                character.view.HideAttackWindow();
                UpdateGridState(character.position, GridStateEnum.Character);
            }
        }

        foreach(var character in battleModel.player1.characters)
        {
            resetView(character);
        }

        foreach(var character in battleModel.player2.characters)
        {
            resetView(character);
        }

        if(battleModel.is1pTurn)
        {
            // 次が自分のターンのときの処理
            battleModel.player2.remainingCost = battleModel.player2.maxCost;

            battleView.SetGameText("Enemy Turn");
            StartCoroutine(battleView.MoveRoutine());
        }
        else
        {
            // 次が敵のターンのときの処理
            battleModel.player1.remainingCost = battleModel.player1.maxCost;
            
            battleView.SetGameText("Your Turn");
            StartCoroutine(battleView.MoveRoutine());

            StartCoroutine(enemyManager.EnemyTurn());
        }
    }

    private void UpdateTimer()
    {
        battleModel.remainingTime -= Time.deltaTime;
        if(battleModel.remainingTime <= 0)
        {
            battleModel.remainingTime = 0;
            EndTurn();
        }
    }

    private void TimerReset()
    {
        battleModel.remainingTime = battleModel.maxTime;
    }

    private void TimerStop()
    {
        battleModel.isTimerRunning = false;
    }

    private void TimerStart()
    {
        battleModel.isTimerRunning = true;
    }


    private void UpdateGridState(Vector2Int pos, GridStateEnum gridState)
    {
        battleModel.nowGridState[pos.x, pos.y] = gridState;
        gridView.UpdateGridImage(pos.x, pos.y, gridState);
    }

    private void BaseHpChanged1(int previousHp) => BaseHpChanged(true, previousHp);
    private void BaseHpChanged2(int previousHp) => BaseHpChanged(false, previousHp);
    private void BaseHpChanged(bool isPlayer1, int previousHp)
    {
        battleView.SetBaseHp(isPlayer1, isPlayer1 ? battleModel.player1.baseHp : battleModel.player2.baseHp);
    }

    private void RemainingCostChanged1(int previousCost) => RemainingCostChanged(true, previousCost);
    private void RemainingCostChanged2(int previousCost) => RemainingCostChanged(false, previousCost);
    private void RemainingCostChanged(bool isPlayer1, int previousCost)
    {
        battleView.SetRemainingCost(isPlayer1, isPlayer1 ? battleModel.player1.remainingCost : battleModel.player2.remainingCost);
    }

    private void RemainingTimeChanged()
    {
        battleView.SetRemainingTime(battleModel.remainingTime);
    }

    private void CharacterHpChanged(BattleCharacterView view, int previousHp, int currentHp)
    {
        view.SetHp(currentHp);
    }


    public void ShowStatus(bool isPlayer1, int characterIndex)
    {
        Sprite statusSprite = (isPlayer1 ? battleModel.player1 : battleModel.player2).characters[characterIndex].statusSprite;
        battleView.ShowStatus(statusSprite);
    }
}


[System.Serializable]
public class DefaultCharactersData
{
    public int characterId;
    public BattleCharacterView view;
    public Vector2Int pos;
    public Sprite statusSprite;
    public GameObject smallWindow;
}
