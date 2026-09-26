using UnityEngine;
using UnityEngine.Rendering;

public class ShowCharacter : MonoBehaviour
{
    public GameObject[] characters;
    public Vector2[] characterPosition;//ここはScriptableobjectを参照するようにする

    public void Start() // 始めにこのクラス内のメソッドをイベントに登録する
    {}
    
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

    public void DamageCharacter() // キャラクターがダメージを受けたときの演出
    {}
}
