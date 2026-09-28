using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private GridViewManager gridViewManager;
    public Vector2 position;
    void Start()
    {
        gridViewManager.grid_set += Delete;
    }
    public void Initialize(GridViewManager Manager)
    {
        gridViewManager = Manager;
    }
    public void Delete(Vector2 delete_position)
    {
        if(delete_position == position)
        {
            gridViewManager.grid_set -= Delete;
            Destroy(this.gameObject);
        }
    }
}
