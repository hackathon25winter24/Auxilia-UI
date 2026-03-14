using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    public BattleDataforOmline battleDataforOnline;
    public BattleDataforLocal battleDataforLocal;

    void Update()
    {
        for (int i = 0; i <= 5; i++)
        {
            battleDataforLocal.character_id[i] = battleDataforOnline.selected_character[i];
        }
    }
}
