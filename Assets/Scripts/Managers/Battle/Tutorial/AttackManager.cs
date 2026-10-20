using System;
using NUnit.Framework.Internal.Commands;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class AttackManager
{
    private BattleModel battleModel;
    private InputManager inputManager;
    private bool isPlayer1;
    private AttackModel attack;
    private Vector2Int characterPos;
    private RotationEnum attackDirection;

    public AttackManager(BattleModel _battleModel, InputManager _inputManager)
    {
        battleModel = _battleModel;
        inputManager = _inputManager;

        inputManager.OnMouseLeftButtonClicked += ConfirmAttack;
        inputManager.OnLeftKeyClicked += SetAttackDirectionL;
        inputManager.OnRightKeyClicked += SetAttackDirectionR;
        inputManager.OnUpKeyClicked += SetAttackDirectionU;
        inputManager.OnDownKeyClicked += SetAttackDirectionD;
    }

    public void SetAttack(bool _isPlayer1, AttackModel _attack, Vector2Int _characterPos)
    {
        isPlayer1 = _isPlayer1;
        attack = _attack;
        characterPos = _characterPos;
        attackDirection = RotationEnum.right;
    }

    public void Attack()
    {
        if(attack == null || characterPos == null) return;

        SetAttackGrid();
    }

    private void SetAttackGrid()
    {
        ClearAttackGrid();

        foreach (Vector2Int pos in attack.attackRange)
        {
            var rotatedPos = characterPos + RotatePosition(pos, attackDirection);
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
        if(!battleModel.isAttack || !battleModel.is1pTurn) return;

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

    private void SetAttackDirectionL() => attackDirection = RotationEnum.left;
    private void SetAttackDirectionR() => attackDirection = RotationEnum.right;
    private void SetAttackDirectionU() => attackDirection = RotationEnum.up;
    private void SetAttackDirectionD() => attackDirection = RotationEnum.down;

    private void OnDestroy()
    {
        inputManager.OnMouseLeftButtonClicked -= ConfirmAttack;
        inputManager.OnLeftKeyClicked -= SetAttackDirectionL;
        inputManager.OnRightKeyClicked -= SetAttackDirectionR;
        inputManager.OnUpKeyClicked -= SetAttackDirectionU;
        inputManager.OnDownKeyClicked -= SetAttackDirectionD;
    }
    
    private enum RotationEnum
    {
        up,
        right,
        down,
        left
    }
}