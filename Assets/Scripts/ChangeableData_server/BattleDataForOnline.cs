using UnityEngine;

[CreateAssetMenu(fileName = "BattleDataForOnline", menuName = "Scriptable Objects/BattleDataForOnline")]
public class BattleDataForOnline : ScriptableObject
{
    public int turn_number;
    public bool is_1p_turn;
    public bool is_finished;
    public string winner_player_id;
    public PlayerState player1;
    public PlayerState player2;
}

[System.Serializable]
public class PlayerState
{
    public string player_id;
    public string player_name;
    public Vector2Int base_position;// 拠点位置はGridが持ってもいいと思うが、暫定的にここに記述。最終設計はGridをいじる時に決めたい。初期値を代入する設定は未作成なので注意
    public int base_hp;
    public int current_cost_remaining;
    public CharactersBattleData[] characters;

    public int rate;
    public int rate_updown;
}


[System.Serializable]
public class CharactersBattleData
{
    public int unique_id;
    public int now_character_hp;
    public bool character_isSelected;// フロントだけで使用しているブール。現在動かしているキャラがtrueになる
    public int now_character_move_cost;// コスト変更はどこでやるか要検討。デバフ情報だけ貰ってローカルで計算してもよい？
    public bool[] debuffs = new bool[8];// 0: 威力上昇, 1: 俊足, 2: 俊敏化, 3: 毒, 4: 麻痺, 5: 鈍足, 6: 鈍化, 7: 出血
    public Vector2Int now_character_position;// サーバーのキャラのPositionX,Yを参照して代入される。指定したキャラが現在いるマス目を表すものとして使われています。グリッドとの関係は未調査
}