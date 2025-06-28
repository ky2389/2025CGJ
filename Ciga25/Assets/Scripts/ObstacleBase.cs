using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private Color obstacleColor = Color.gray;
    
    private Vector2Int gridPosition;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        // Get the sprite renderer component (user will add it manually)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on obstacle! Please add it manually.");
        }
        
        // Scale to fit tile size (32x32 sprites need to be 100x larger)
        float scale = GridManager.Instance.TileSize / 32f; // Scale factor for 32x32 sprites
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    public void Initialize(Vector2Int startPosition)
    {
        gridPosition = startPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        
        // Register this obstacle with the grid manager
        GridManager.Instance.RegisterObstacle(this);
    }
    
    private void OnDestroy()
    {
        // Unregister this obstacle when destroyed
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterObstacle(this);
        }
    }
    
    public Vector2Int GridPosition
    {
        get { return gridPosition; }
    }
} 