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
    [SerializeField] StoryManagerData storyManagerData;

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

        InitGrids();
        InitCharacters();
        battleView.Init(battleModel.player1.playerName, battleModel.player2.playerName, battleModel.maxCost, this);
    }

    private void InitBattleModel()
    {
        battleModel = new BattleModel(player1CharactersData, player2CharactersData, characterData);

        battleModel.player1.BaseHpChanged += baseHpChanged1;
        battleModel.player2.BaseHpChanged += baseHpChanged2;

        // ## 各CharacterのHpChangedイベントも登録する
    }

    private void Update()
    {
        if(battleModel.is1pTurn)
        {
            if(!battleModel.isAttack) Move();
            else
            {
                attackManager.Attack();
            }
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
                UpdateGridState(new Vector2Int(x, y), battleModel.defaultGridState[x,y]);
            }
        }
    }

    private void InitCharacters()
    {
        foreach (var character in battleModel.player1.characters)
        {
            if(character == null) continue;
            InitCharacter(character);
        }

        foreach (var character in battleModel.player2.characters)
        {
            if(character == null) continue;
            InitCharacter(character);
        }
    }

    private void InitCharacter(BattleModel.CharacterBattleData character)
    {
        // CharactersDataをidから検索してviewの初期化
        foreach (var chData in characterData.characters)
        {
            if(chData.default_id == character.id)
            {
                character.view.Init(chData, this);
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
                    UpdateGridState(selectedCharacter.position, battleModel.defaultGridState[selectedCharacter.position.x, selectedCharacter.position.y]);
                }
            }

            battleModel.selected = charactedIndex;

            // 選択したキャラクターの攻撃ボタンを表示
            if(charactedIndex != -1)
            {
                var selectedCharacter = battleModel.player1.characters[charactedIndex];
                if(selectedCharacter != null)
                {
                    UpdateGridState(selectedCharacter.position, GridStateEnum.Character);
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

            Debug.Log(battleModel.player1.remainingCost);

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
                    UpdateGridState(movedPos, GridStateEnum.Character);
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

    private void UpdateGridState(Vector2Int pos, GridStateEnum gridState)
    {
        battleModel.nowGridState[pos.x, pos.y] = gridState;
        gridView.UpdateGridImage(pos.x, pos.y, gridState);
    }

    private void baseHpChanged1(int previousHp) => baseHpChanged(true, previousHp);
    private void baseHpChanged2(int previousHp) => baseHpChanged(false, previousHp);
    private void baseHpChanged(bool isPlayer1, int previousHp)
    {
        
    }
}


[System.Serializable]
public class DefaultCharactersData
{
    public int characterId;
    public BattleCharacterView view;
    public Vector2Int pos;
    public Sprite statusSprite;
}