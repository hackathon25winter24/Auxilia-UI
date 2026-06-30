using UnityEngine;
using UnityEngine.EventSystems; 

public class ImageClick : MonoBehaviour, IPointerClickHandler
{
    public UserData userData;
    public int CharacuerID;
    // クリックされた時に自動で呼ばれる
    public void OnPointerClick(PointerEventData eventData)
    {
        if (userData == null)
        {
            Debug.LogError($"ImageClick: playerData is null on GameObject {gameObject.name}!");
            return;
        }

        SEManager.instance?.PlaySelectSE();

        userData.home_character_id = CharacuerID;
    }
}
