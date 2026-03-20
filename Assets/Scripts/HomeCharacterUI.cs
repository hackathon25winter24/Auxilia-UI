using UnityEngine;
using UnityEngine.EventSystems; 

public class ImageClick : MonoBehaviour, IPointerClickHandler
{
    public PlayerData playerData;
    public int CharacuerID;
    // クリックされた時に自動で呼ばれる
    public void OnPointerClick(PointerEventData eventData)
    {
        if (playerData == null)
        {
            Debug.LogError($"ImageClick: playerData is null on GameObject {gameObject.name}!");
            return;
        }

        SEManager.instance.PlaySelectSE();

        playerData.home_character_ID = CharacuerID;
    }
}
