using System;
using NUnit.Framework.Internal.Commands;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class AttackManager
{
    private InputData inputData;
    private BattleModel battleModel;
    private bool isPlayer1;
    private AttackModel attack;
    private Vector2Int characterPos;

    public AttackManager(InputData _inputData, BattleModel _battleModel)
    {
        inputData = _inputData;
        battleModel = _battleModel;
    }

    public void SetAttack(bool _isPlayer1, AttackModel _attack, Vector2Int _characterPos)
    {
        isPlayer1 = _isPlayer1;
        attack = _attack;
        characterPos = _characterPos;
    }

    public void Attack()
    {
        if(attack == null || characterPos == null) return;

        SetAttackGrid();

        if(inputData.left_mouse_button_ispressed)
        {
            ConfirmAttack();
        }
    }

    private void SetAttackGrid()
    {
        ClearAttackGrid();

        var dir = GetMouseDirection();
        foreach (Vector2Int pos in attack.attackRange)
        {
            var rotatedPos = characterPos + RotatePosition(pos, dir);
            if(0 <= rotatedPos.x && rotatedPos.x < 8 && 0 <= rotatedPos.y && rotatedPos.y < 5) 
                    battleModel.isAttackGrid[rotatedPos.x, rotatedPos.y] = true;
        }
    }

    private void ClearAttackGrid()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                battleModel.isAttackGrid[i,j] = false;
            }
        }
    }

    public void ConfirmAttack()
    {
        var playerData =  isPlayer1 ? battleModel.player1 : battleModel.player2;
        var enemyData = !isPlayer1 ? battleModel.player1 : battleModel.player2;

        if(playerData.remainingCost < attack.attackCost)
        {
            // コストが足りなかった時の効果音
            SEManager.instance.PlayBackSE(); // 仮
            battleModel.isAttack = false;
            ClearAttackGrid();
            return;
        }

        bool isHit = false;

        // enemy(攻撃していない側)に攻撃が当たるかどうか
        if (attack.attackTargetId == 0 || attack.attackTargetId == 2)
        {
            // 拠点の処理
            if (battleModel.isAttackGrid[enemyData.basePos.x, enemyData.basePos.y])
            {
                enemyData.baseHp -= attack.attackPower;
                isHit = true;
            }

            foreach (var character in enemyData.characters)
            {
                if(character != null)
                {
                    if(battleModel.isAttackGrid[character.position.x, character.position.y])
                    {
                        character.hp -= attack.attackPower;
                        isHit = true;
                    }
                }
            }
        }

        // 自陣営に攻撃が当たるかどうか
        if (attack.attackTargetId == 1 || attack.attackTargetId == 2)
        {
            if (battleModel.isAttackGrid[playerData.basePos.x, playerData.basePos.y])
            {
                playerData.baseHp -= attack.attackPower;
                isHit = true;
            }

            foreach (var character in playerData.characters)
            {
                if(character != null)
                {
                    if(battleModel.isAttackGrid[character.position.x, character.position.y])
                    {
                        character.hp -= attack.attackPower;
                        isHit = true;
                    }
                }
            }
        }

        if(isHit)
        {
            playerData.remainingCost -= attack.attackCost;
            battleModel.isAttack = false;
            ClearAttackGrid();
        }
        else
        {
            // 攻撃が当たらなかった時の効果音
            SEManager.instance.PlayBackSE(); // 仮
            battleModel.isAttack = false;
            ClearAttackGrid();
        }
    }

    private Vector2Int RotatePosition(Vector2Int pos, RotationEnum dir)
    {
        if (dir == RotationEnum.right) return pos; 
        if (dir == RotationEnum.left)  return new Vector2Int(-pos.x, -pos.y); 
    
        // 上下方向：XとYを入れ替える
        // グリッド座標系で「上」が Y-1 の場合は符号に注意
        if (dir == RotationEnum.up)    return new Vector2Int(pos.y, -pos.x); 
        if (dir == RotationEnum.down)  return new Vector2Int(-pos.y, pos.x); 

        return pos;
    }

    private RotationEnum GetMouseDirection()
    {
        Vector2 characterWorldPos = new Vector2(characterPos.x * 50f - 175f, characterPos.y * -50f + 60f);
        Vector2 mousePos = inputData.mouse_position;
        Vector2 diff = mousePos - characterWorldPos;

        // XとYの絶対値を比較して、どちらの方向に大きく動いているか判定
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            // 左右方向
            return diff.x > 0 ? RotationEnum.right : RotationEnum.left;
        }
        else
        {
            // 上下方向
            // UIの座標系に合わせる（上がプラスならup、下がマイナスならdown）
            return diff.y > 0 ? RotationEnum.up : RotationEnum.down;
        }
    }
    
    private enum RotationEnum
    {
        up,
        right,
        down,
        left
    }
}