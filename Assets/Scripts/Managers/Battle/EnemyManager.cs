using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EnemyManager
{
    private BattleManager battleManager;
    private BattleModel battleModel;
    private BattleView battleView;

    private int turn = 0;

    public EnemyManager(BattleManager _battleManager, BattleModel _battleModel, BattleView _battleView)
    {
        battleManager = _battleManager;
        battleModel = _battleModel;
        battleView = _battleView;

        turn = 0;
    }

    public IEnumerator EnemyTurn()
    {
        // ターン変化演出が終わるまで待機
        yield return new WaitUntil(() => !battleView.IsMoveRoutineRunning);

        // 敵の行動処理
        if (battleModel.player2.characters[0].hp > 0)
        {
            battleModel.selected = 0;
            var character = battleModel.player2.characters[0];

            battleManager.UpdateGridState(character.position, GridStateEnum.Selected);
            character.view.ShowAttackWindow();

            yield return new WaitForSeconds(1.0f);

            if(turn % 2 == 0)
            {
                battleManager.TryMove(false, character, new Vector2Int(character.position.x - 1, character.position.y));
            }
            else
            {
                battleManager.TryMove(false, character, new Vector2Int(character.position.x + 1, character.position.y));
            }
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(1.0f);

        turn++;
        battleManager.EndTurn();
    }
}