using Unity.VisualScripting;
using UnityEngine;

public class CharacterViewManager : MonoBehaviour
{
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private GameObject[] characters;
    [SerializeField] private float character_move_time;
    public Vector2[] characterPosition;//ここはScriptableobjectを参照するようにする
    Vector2 character_now_position;//ここはScriptableobjectを参照するようにする
    Vector2 character_past_position;//ここはScriptableobjectを参照するようにする

    public void Start() // 始めにこのクラス内のメソッドをイベントに登録する
    {
        viewManager.moved += MoveCharacter;
        viewManager.skill_used += CharacterAttecked;
        viewManager.damaged += CharacterDamaged;
        viewManager.revived += CharacterRevived;
        viewManager.defeated += CharacterDefeated;
        viewManager.attack_blocked += CharacterAttackBlocked;
        viewManager.effect_blocked += CharacterEffectBlocked;
        viewManager.form_changed += CharacterFormChanged;
    }

    public void PutCharacter() // キャラクターの位置の変更及びその演出
    {
        if(characterPosition.Length == 6)
        {
            for(int i = 0; i < 6; i++)
            {
                characters[i].transform.position = characterPosition[i];
            }
        }
        else
        {
            Debug.Log("位置が不明なキャラが存在します");
        }
    }
    public void MoveCharacter() // キャラクターが移動するときの位置の変更及びその演出
    {
        float time = 0;
        while(time < character_move_time)
        {
            time += Time.deltaTime;
            characters[0].transform.position = character_now_position + (character_past_position - character_now_position)* time / character_move_time;
        }
    }
    public void CharacterDamaged() // キャラクターがダメージを受けたときの演出
    {}
    public void CharacterAttecked() // キャラクターが攻撃をした時の演出
    {}
    public void CharacterRevived() // キャラクターが復活したときの演出
    {}
    public void CharacterDefeated() // キャラクターがやられたときの演出
    {}
    public void CharacterAttackBlocked() // キャラクターが攻撃を防いだ時の演出
    {}
    public void CharacterEffectBlocked() // キャラクターがデバフを防いだ時の演出
    {}
    public void CharacterFormChanged() // キャラクターの形態変化
    {}
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        viewManager.moved -= MoveCharacter;
        viewManager.skill_used -= CharacterAttecked;
        viewManager.damaged -= CharacterDamaged;
        viewManager.revived -= CharacterRevived;
        viewManager.defeated -= CharacterDefeated;
        viewManager.attack_blocked -= CharacterAttackBlocked;
        viewManager.effect_blocked -= CharacterEffectBlocked;
        viewManager.form_changed -= CharacterFormChanged;
    }
}
