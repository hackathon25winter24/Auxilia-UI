using System.Collections;
using UnityEngine;

public class EnemyManager
{
    private BattleModel battleModel;

    public EnemyManager(BattleModel _battleModel)
    {
        battleModel = _battleModel;
    }

    public IEnumerator EnemyTurn()
    {
        return null;
    }
}