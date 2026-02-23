using UnityEngine;
using UnityEngine.EventSystems; 

public class ImageClick : MonoBehaviour, IPointerClickHandler
{
    public PlayerData playerData;
    public int CharacuerID;
    // クリックされた時に自動で呼ばれる
    public void OnPointerClick(PointerEventData eventData)
    {
        playerData.home_character_ID = CharacuerID;
    }
}
