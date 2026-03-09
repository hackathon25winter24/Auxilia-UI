using UnityEngine;

public class TestBattleState : MonoBehaviour
{
    //仮のバトル管理スクリプト。後で消す

    public CurrentCharacterData[] characters;
    void Start()
    {
        characters[0].SpawnCharacter(0, new Vector2Int(0,4));
        characters[1].SpawnCharacter(1, new Vector2Int(1,2));
        characters[2].SpawnCharacter(0, new Vector2Int(0,0));
        characters[3].SpawnCharacter(1, new Vector2Int(7,4));
        characters[4].SpawnCharacter(0, new Vector2Int(6,2));
        characters[5].SpawnCharacter(1, new Vector2Int(7,0));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            characters[0].Damage(10);   
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            characters[1].Damage(10);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            characters[0].Heal(10);   
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            characters[0].MovePosition(Vector2Int.up);   
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            characters[0].MovePosition(Vector2Int.left);   
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            characters[0].MovePosition(Vector2Int.down);   
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            characters[0].MovePosition(Vector2Int.right);   
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            characters[1].MovePosition(Vector2Int.up);   
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            characters[1].MovePosition(Vector2Int.left);   
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            characters[1].MovePosition(Vector2Int.down);   
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            characters[1].MovePosition(Vector2Int.right);   
        }
    }
}
