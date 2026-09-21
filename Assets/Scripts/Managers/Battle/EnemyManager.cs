using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EnemyManager
{
    private BattleManager battleManager;
    private BattleModel battleModel;
    private BattleView battleView;

    public EnemyManager(BattleManager _battleManager, BattleModel _battleModel, BattleView _battleView)
    {
        battleManager = _battleManager;
        battleModel = _battleModel;
        battleView = _battleView;
    }

    public IEnumerator EnemyTurn()
    {
        // ターン変化演出が終わるまで待機
        yield return new WaitUntil(() => !battleModel.isTimerRunning);
        yield return new WaitForSeconds(0.5f);

        // 敵の行動処理（ToDo）

        battleManager.EndTurn();
    }
}