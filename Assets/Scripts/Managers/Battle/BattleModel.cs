

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
    public float maxTime = 10.0f; // 暫定
    public float remainingTime;
    public int maxCost = 50;
    public PlayerState player1;
    public PlayerState player2;

    public BattleModel(DefaultCharactersData[] defaultCharactersDatas1, DefaultCharactersData[] defaultCharactersDatas2, CharacterData characterDatas)
    {
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

        player1 = new PlayerState(new Vector2Int(0,2), maxCost, defaultCharactersDatas1, characterDatas);
        player2 = new PlayerState(new Vector2Int(7,2), maxCost, defaultCharactersDatas2, characterDatas);

        foreach(var character in player1.characters)
        {
            nowGridState[character.position.x, character.position.y] = GridStateEnum.Character;
        }
        foreach(var character in player2.characters)
        {
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
            private set
            {
                int previousHp = baseHpValue;
                baseHpValue = Mathf.Clamp(value, 0, maxBaseHp);

                if (previousHp != baseHpValue)
                {
                    BaseHpChanged?.Invoke(previousHp);
                }
            }
        }
        public void DamageBaseHp(int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            baseHp -= damage;
        }
        public void HealBaseHp(int healAmount)
        {
            if (healAmount <= 0)
            {
                return;
            }

            baseHp += healAmount;
        }
        
        public int remainingCost;
        public CharacterBattleData[] characters = new CharacterBattleData[3];


        public PlayerState(Vector2Int basePos, int maxCost, DefaultCharactersData[] defaultCharactersDatas, CharacterData characterDatas)
        {
            playerId = "playerId";
            playerName = "playerName";
            this.basePos = basePos;
            baseHpValue = maxBaseHp;
            remainingCost = maxCost;

            for (int i = 0; i < defaultCharactersDatas.Length; i++)
            {
                if(i >= 3) break;
                characters[i] = new CharacterBattleData(defaultCharactersDatas[i], characterDatas);
            }
        }
    }

    public class CharacterBattleData
    {
        public int id {get; private set;}
        public BattleCharacterView view {get; private set;}
        public int maxHp;

        Action<int> hpChanged;
        private int hpValue;
        public int hp
        {
            get => hpValue;

            private set
            {
                int previousHp = hpValue;
                hpValue = Mathf.Clamp(value, 0, maxHp);

                if (previousHp != hpValue)
                {
                    hpChanged?.Invoke(previousHp);
                }
            }
        }
        public void damage(int damage)
        {
            if(damage <= 0)
            {
                return;
            }

            hp -= damage;
        }
        public void heal(int heal)
        {
            if(heal <= 0)
            {
                return;
            }

            hp += heal;
        }

        public int moveCost;
        public AttackModel[] attacks = new AttackModel[3];
        public bool[] buffAndDebuffs = new bool[10];
        public Vector2Int position;
        public Sprite statusSprite;


        public CharacterBattleData(DefaultCharactersData data, CharacterData characterDatas)
        {
            id = data.characterId;
            view = data.view;

            CharactersData characterData = null;
            foreach (var chData in characterDatas.characters)
            {
                if(chData.default_id == id)
                {
                    characterData = chData;
                    break;
                }
            }

            maxHp = characterData.default_hp;
            hpValue = maxHp;
            moveCost = characterData.default_move_cost;

            for(int i = 0; i < 3; i++)
            {
                attacks[i] = new AttackModel(characterData.attacks[i]);
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