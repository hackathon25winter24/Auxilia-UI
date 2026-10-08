using UnityEngine;
using UnityEngine.UI;

public class CharacterObject : MonoBehaviour
{
    public int object_num;
    public float move_time;
    public CharacterViewManager characterViewManager;
    public CharacterData characterData;
    public void Initialize(CharacterViewManager Manager, CharacterData Data, int character_id)
    {
        characterViewManager = Manager;
        characterData = Data;
        Image character_image = this.GetComponent<Image>();
        if(character_image != null)
        {
            character_image.sprite = characterData.characters[character_id].default_sprite_mini;
        }
    }
    public void Move(int moved_object_num, Vector2 new_position)
    {
        Vector2 now_position = transform.position;
        if(moved_object_num == object_num)
        {
            float time = 0;
        while(time < move_time)
        {
            time += Time.deltaTime;
            transform.position = now_position + (new_position - now_position)* time / move_time;
        }
        }
    }
    public void CharacterOnClicked()
    {}
}
