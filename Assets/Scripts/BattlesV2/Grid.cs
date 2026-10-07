using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private GridViewManager gridViewManager;
    public Vector2 position;
    public void Initialize(GridViewManager Manager) 
    {
        gridViewManager = Manager;
        gridViewManager.delete_grid += Delete;
        gridViewManager.delete_all_grid += DeleteAll;
    }
    public void Delete(Vector2 delete_position)
    {
        if(delete_position == position)
        {
            gridViewManager.delete_grid -= Delete;
            gridViewManager.delete_all_grid -= DeleteAll;
            Destroy(this.gameObject);
        }
    }
    public void DeleteAll()
    {
        gridViewManager.delete_grid -= Delete;
        gridViewManager.delete_all_grid -= DeleteAll;
        Destroy(this.gameObject);
    }
}
