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
    [SerializeField] StoryManagerData sotryManagerData;

    [Space]

    [Header("初期状態")]
    [SerializeField] DefaultCharactersData[] player1CharactersData;
    [SerializeField] DefaultCharactersData[] player2CharactersData;
    [SerializeField] Transform gridsParentTs;
    [SerializeField] GameObject gridPrefab;
    
    private void Start()
    {
        InitBattleModel();
        attackManager = new AttackManager(inputData, battleModel);

        InitGrids();
        InitCharacters();
    }

    private void Update()
    {
        if(battleModel.is1pTurn)
        {
            if(!battleModel.isAttack) Move();
        }
    }

    private void InitBattleModel()
    {
        battleModel = new BattleModel(player1CharactersData, player2CharactersData, characterData);

        battleModel.player1.BaseHpChanged += baseHpChanged1;
        battleModel.player2.BaseHpChanged += baseHpChanged2;

        // ## 各CharacterのHpChangedイベントも登録する
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
                character.view.Init(chData);
                break;
            }
        }

        // 位置の初期化
        character.view.SetPosition(character.position);
    }

    private void Move()
    {
        if(battleModel.selected != -1)
        {
            var selectedCharacter = battleModel.player1.characters[battleModel.selected];
            var movedPos = selectedCharacter.position;
                
            if(inputData.left_key_ispressed) movedPos.x--;
            if(inputData.right_key_ispressed) movedPos.x++;
            if(inputData.up_key_ispressed) movedPos.y++;
            if(inputData.down_key_ispressed) movedPos.y--;

            if(0 <= movedPos.x && movedPos.x < 8 && 0 <= movedPos.y && movedPos.y < 5)
            {
                var state = battleModel.nowGridState[movedPos.x, movedPos.y];
                if(state != GridStateEnum.Prohibit && state != GridStateEnum.Base && state != GridStateEnum.Character && state != GridStateEnum.Stop)
                {
                    UpdateGridState(selectedCharacter.position, battleModel.defaultGridState[selectedCharacter.position.x, selectedCharacter.position.y]);
                    UpdateGridState(movedPos, GridStateEnum.Character);
                    selectedCharacter.position = movedPos;
                }
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