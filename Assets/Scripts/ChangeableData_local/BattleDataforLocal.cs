using UnityEngine;

[CreateAssetMenu(fileName = "BattleDataforLocal", menuName = "Scriptable Objects/BattleDataforLocal")]
public class BattleDataforLocal : ScriptableObject
{
    public bool is_myturn;
    public int[] character_id;
}
