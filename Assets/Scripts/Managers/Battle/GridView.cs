using UnityEngine;
using UnityEngine.UI;

public class GridView : MonoBehaviour
{
    private Image[,] images = new Image[8, 5];
    [SerializeField] private GridSprite[] gridSprites;
    public void Init(int x, int y, Image image)
    {
        images[x,y] = image;
    }

    public void UpdateGridImage(int x, int y, GridStateEnum gridState)
    {
        images[x,y].sprite = GetSprite(gridState);
    }

    private Sprite GetSprite(GridStateEnum state)
    {
        foreach (GridSprite gSpr in gridSprites)
        {
            if(gSpr.gridState == state)
            {
                return gSpr.sprite;
            }
        }

        return null;
    }

    [System.Serializable]
    private class GridSprite
    {
        public GridStateEnum gridState;
        public Sprite sprite;
    }
}