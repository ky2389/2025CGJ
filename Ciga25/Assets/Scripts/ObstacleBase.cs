using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private Color obstacleColor = Color.gray;
    
    private Vector2Int gridPosition;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        // Setup visual representation
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        CreateObstacleSprite();
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
    
    private void CreateObstacleSprite()
    {
        // Create a simple texture for the obstacle
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, obstacleColor);
        texture.Apply();
        
        // Create sprite from texture
        Sprite obstacleSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        spriteRenderer.sprite = obstacleSprite;
        
        // Scale to fit tile size
        float scale = GridManager.Instance.TileSize * 0.9f; // 90% of tile size to be slightly smaller than full tile
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    public Vector2Int GridPosition
    {
        get { return gridPosition; }
    }
} 