using Unity.VisualScripting;
using UnityEngine;

public class CharacterViewManager : MonoBehaviour
{
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private GameObject[] characters;
    [SerializeField] private Transform canvas;
    [SerializeField] private GameObject character_button;
    [SerializeField] private float character_move_time;
    [SerializeField] private CharacterData characterData;
    Vector2 character_now_position;//ここはScriptableobjectを参照するようにする
    Vector2 character_past_position;//ここはScriptableobjectを参照するようにする
    int character; // 選択されたキャラクター。ここはScriptableobjectを参照するようにする
    public void Awake()
    {
        PutCharacter(new Vector2(-175, 60), 0, 0);
        PutCharacter(new Vector2(-125, -40), 0, 1);
        PutCharacter(new Vector2(-175, -140), 0, 2);
        PutCharacter(new Vector2(175, 60), 0, 3);
        PutCharacter(new Vector2(125, -40), 0, 4);
        PutCharacter(new Vector2(175, -140), 0, 5);
    }
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

    public void PutCharacter(Vector2 position, int character_id, int object_id) // キャラクターの生成
    {
        if(0 <= character_id && character_id < characterData.characters.Length)
        {
            GameObject characterObj = Instantiate(character_button, canvas);
            RectTransform rectTransform = characterObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
                rectTransform.localScale = Vector3.one;
                characterObj.transform.SetSiblingIndex(3);
            }
            CharacterObject character = characterObj.GetComponent<CharacterObject>();
            if (character != null)
            {
                character.Initialize(this, characterData, character_id);
            }
            character.object_num = object_id;
            character.move_time = character_move_time;
        }
    }
    public void MoveCharacter() // キャラクターが移動するときの位置の変更及びその演出
    {
        float time = 0;
        while(time < character_move_time)
        {
            time += Time.deltaTime;
            characters[character].transform.position = character_now_position + (character_past_position - character_now_position)* time / character_move_time;
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
