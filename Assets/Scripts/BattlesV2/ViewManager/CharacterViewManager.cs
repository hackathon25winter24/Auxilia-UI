using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CharacterViewManager : MonoBehaviour
{
    public GameObject[] characters;
    public Vector2[] characterPosition;//ここはScriptableobjectを参照するようにする

    public void ChangeView(BattleDataForOnline data, UserData user, CharacterData assets)
    {
        bool flip = data.State.Players.Count > 1 && user != null && data.State.Players[1].Id == user.user_id;
        var first = flip ? data.player2 : data.player1;
        var second = flip ? data.player1 : data.player2;
        var all = first.characters.Concat(second.characters).ToArray();
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null) continue;
            characters[i].SetActive(i < all.Length && all[i].now_character_hp > 0);
            if (i >= all.Length) continue;
            var c = all[i];
            var p = c.now_character_position;
            if (flip) p.x = 7 - p.x;
            if (characters[i].transform is RectTransform rect)
                rect.anchoredPosition = new Vector2(-175f + p.x * 50f, 60f - p.y * 50f);
            var image = characters[i].GetComponent<Image>();
            if (image != null && assets != null && c.unique_id >= 0 && c.unique_id < assets.characters.Length)
                image.sprite = assets.characters[c.unique_id].default_sprite_mini;
        }
    }

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
    private void OnDestroy() // ゲーム終了時にイベント登録を解除する
    {
        if (NetworkManager.Instance != null)
        {
            
        }
    }
}
