

using System;
using System.Data;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BattleModel
{
    public GridStateEnum[,] nowGridState = new GridStateEnum[8,5];
    public GridStateEnum[,] defaultGridState = new GridStateEnum[8,5];
    
    public bool[,] isAttackGrid = new bool[8,5];
    public bool is1pTurn;
    public int selected;
    public bool isAttack;
    public float maxTime = 60.0f; // 暫定
    public bool isTimerRunning = false;
    public Action RemainingTimeChanged;
    private float remainingTimeValue;
    public float remainingTime
    {
        get => remainingTimeValue;
        set
        {
            float previousTime = remainingTimeValue;
            remainingTimeValue = Mathf.Clamp(value, 0, maxTime);

            if (previousTime != remainingTimeValue)
            {
                RemainingTimeChanged?.Invoke();
            }
        }
    }

    public PlayerState player1;
    public PlayerState player2;

    public BattleModel(DefaultCharactersData[] defaultCharactersDatas1, DefaultCharactersData[] defaultCharactersDatas2, CharacterData characterDatas)
    {
        if (defaultCharactersDatas1 == null)
        {
            defaultCharactersDatas1 = Array.Empty<DefaultCharactersData>();
        }
        if (defaultCharactersDatas2 == null)
        {
            defaultCharactersDatas2 = Array.Empty<DefaultCharactersData>();
        }

        defaultGridState[0,2] = GridStateEnum.Base;
        defaultGridState[7,2] = GridStateEnum.Base;
        
        defaultGridState[3,2] = GridStateEnum.Prohibit;

        for(int x = 0; x < 8; x++)
        {
            for(int y = 0; y < 5; y++)
            {
                nowGridState[x,y] = defaultGridState[x,y];

                isAttackGrid[x,y] = false;
            }
        }

        is1pTurn = true;
        selected = -1;
        isAttack = false;
        remainingTime = maxTime;

        player1 = new PlayerState(new Vector2Int(0,2), 50, defaultCharactersDatas1, characterDatas);
        player2 = new PlayerState(new Vector2Int(7,2), 50, defaultCharactersDatas2, characterDatas);

        foreach(var character in player1.characters)
        {
            if (character == null) continue;
            nowGridState[character.position.x, character.position.y] = GridStateEnum.Character;
        }
        foreach(var character in player2.characters)
        {
            if (character == null) continue;
            nowGridState[character.position.x, character.position.y] = GridStateEnum.Character;
        }
    }

    public class PlayerState
    {
        public string playerId;
        public string playerName;
        public Vector2Int basePos;
        public int maxBaseHp = 200;
        public event Action<int> BaseHpChanged;
        private int baseHpValue;
        public int baseHp
        {
            get => baseHpValue;
            set
            {
                int previousHp = baseHpValue;
                baseHpValue = Mathf.Clamp(value, 0, maxBaseHp);

                if (previousHp != baseHpValue)
                {
                    BaseHpChanged?.Invoke(previousHp);
                }
            }
        }
        
        public int maxCost;
        public Action<int> RemainingCostChanged;
        private int remainingCostValue;
        public int remainingCost
        {
            get => remainingCostValue;
            set
            {
                int previousCost = remainingCostValue;
                remainingCostValue = Mathf.Clamp(value, 0, maxCost);

                if (previousCost != remainingCostValue)
                {
                    RemainingCostChanged?.Invoke(previousCost);
                }
            }
        }

        public CharacterBattleData[] characters;


        public PlayerState(Vector2Int basePos, int maxCost, DefaultCharactersData[] defaultCharactersDatas, CharacterData characterDatas)
        {
            playerId = "playerId";
            playerName = "playerName";
            this.basePos = basePos;
            baseHpValue = maxBaseHp;
            this.maxCost = maxCost;
            remainingCost = maxCost;

            if (defaultCharactersDatas == null)
            {
                return;
            }

            characters = new CharacterBattleData[defaultCharactersDatas.Length];

            for (int i = 0; i < defaultCharactersDatas.Length; i++)
            {
                if(i >= 3) break;
                if (defaultCharactersDatas[i] == null)
                {
                    continue;
                }
                characters[i] = new CharacterBattleData(defaultCharactersDatas[i], characterDatas);
            }
        }
    }

    public class CharacterBattleData
    {
        public int id {get; private set;}
        public BattleCharacterView view {get; private set;}
        public int maxHp;

        public Action<BattleCharacterView, int, int> hpChanged;
        private int hpValue;
        public int hp
        {
            get => hpValue;

            set
            {
                int previousHp = hpValue;
                hpValue = Mathf.Clamp(value, 0, maxHp);

                if (previousHp != hpValue)
                {
                    hpChanged?.Invoke(view, previousHp, hpValue);
                }
            }
        }

        public int moveCost;
        public AttackModel[] attacks = new AttackModel[3];
        public bool[] buffAndDebuffs = new bool[10];
        public Vector2Int position;
        public Sprite statusSprite;


        public CharacterBattleData(DefaultCharactersData data, CharacterData characterDatas)
        {
            if (data == null)
            {
                return;
            }

            id = data.characterId;
            view = data.view;

            CharactersData characterData = null;
            if (characterDatas != null)
            {
                foreach (var chData in characterDatas.characters)
                {
                    if(chData.default_id == id)
                    {
                        characterData = chData;
                        break;
                    }
                }
            }

            if (characterData == null)
            {
                maxHp = 0;
                hpValue = 0;
                moveCost = 0;
                position = data.pos;
                statusSprite = data.statusSprite;
                return;
            }

            maxHp = characterData.default_hp;
            hpValue = maxHp;
            moveCost = characterData.default_move_cost;

            for(int i = 0; i < 3; i++)
            {
                if (i < characterData.attacks.Length)
                {
                    attacks[i] = new AttackModel(characterData.attacks[i]);
                }
            }
            
            for(int i = 0; i < buffAndDebuffs.Length; i++)
            {
                buffAndDebuffs[i] = false;
            }

            position = data.pos;
            statusSprite = data.statusSprite;
        }
    }
}

public enum GridStateEnum
{
    Normal,
    Attack,
    Prohibit,
    Base,
    Character,
    Selected,
    Makibishi,
    Landmine,
    Poison,
    Stop
}

public class AttackModel
{
    public string attackName;
    public int attackCost;
    public int attackPower;
    public int attackTargetId;
    public int[] debuffs = new int[8]; // 参照渡し
    public Sprite attackButton; // 参照渡し
    public Vector2Int[] attackRange;
    public Sprite attackRangeImage;


    public AttackModel(Attacks attack)
    {
        attackName = attack.default_attack_name;
        attackCost = attack.default_attack_cost;
        attackPower = attack.default_attack_power;
        attackTargetId = attack.default_attack_target;
        debuffs = attack.debuffs;
        attackButton = attack.attack_button;
        attackRange = (Vector2Int[])attack.default_attack_range.Clone();
        attackRangeImage = attack.attack_range_image;
    }
}