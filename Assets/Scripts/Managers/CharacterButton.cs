using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RoomButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEvent<int> onLongPressWithIndex; // インデックスを渡すイベント
    public int myIndex; // 自分の番号

    [SerializeField] private float threshold = 0.5f;
    private float pressTime;
    private bool isPressed;

    void Update()
    {
        if (isPressed)
        {
            if (Time.time - pressTime > threshold)
            {
                isPressed = false; // 連続発動防止
                onLongPressWithIndex.Invoke(myIndex); 
                Debug.Log($"ボタン {myIndex} が長押しされました");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        pressTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    // 指がボタンの外にスライドしたらキャンセル（スクロール開始時などに重要）
    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
    }
}