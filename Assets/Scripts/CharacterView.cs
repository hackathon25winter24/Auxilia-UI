using UnityEngine;
using System.Collections;

public class CharacterView : MonoBehaviour
{
    public CurrentCharacterData currentCharacterData;
    void Start()
    {
        currentCharacterData = GetComponent<CurrentCharacterData>();
        currentCharacterData.OnPositionChanged += HandleMove;
    }
    void OnDestroy()
    {
        currentCharacterData.OnPositionChanged -= HandleMove;
    }
    void HandleMove(Vector2Int position)
    {
        StartCoroutine(MoveAnimation(position));
    }
    IEnumerator MoveAnimation(Vector2Int targetPosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        float duration = 0.5f;//移動時間
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, endPosition, t / duration);
            yield return null;
        }
        transform.position = endPosition;
    }

    void Update()
    {
        
    }
}
