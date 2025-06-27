using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [Header("Grid Settings")]
    public int gridWidth = 9;
    public int gridHeight = 9;
    public float tileSize = 32f;
    
    [Header("Visual Debug")]
    public bool showGrid = true;
    public Color gridColor = Color.white;
    
    private Vector3 gridOrigin;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Calculate grid origin (center the grid)
        gridOrigin = new Vector3(
            -(gridWidth * tileSize) / 2f + tileSize / 2f,
            -(gridHeight * tileSize) / 2f + tileSize / 2f,
            0f
        );
    }
    
    // Convert grid coordinates to world position
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return gridOrigin + new Vector3(gridPos.x * tileSize, gridPos.y * tileSize, 0f);
    }
    
    // Convert world position to grid coordinates
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - gridOrigin;
        return new Vector2Int(
            Mathf.RoundToInt(localPos.x / tileSize),
            Mathf.RoundToInt(localPos.y / tileSize)
        );
    }
    
    // Check if grid position is valid (within bounds)
    public bool IsValidPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth && 
               gridPos.y >= 0 && gridPos.y < gridHeight;
    }
    
    // Get distance between two grid positions
    public int GetGridDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
    }
    
    // Visual debug - draw grid in scene view
    private void OnDrawGizmos()
    {
        if (!showGrid) return;
        
        Gizmos.color = gridColor;
        
        // Calculate grid bounds
        Vector3 origin = gridOrigin;
        float width = gridWidth * tileSize;
        float height = gridHeight * tileSize;
        
        // Draw vertical lines
        for (int i = 0; i <= gridWidth; i++)
        {
            Vector3 start = origin + new Vector3(i * tileSize - tileSize/2f, -tileSize/2f, 0f);
            Vector3 end = origin + new Vector3(i * tileSize - tileSize/2f, height - tileSize/2f, 0f);
            Gizmos.DrawLine(start, end);
        }
        
        // Draw horizontal lines
        for (int i = 0; i <= gridHeight; i++)
        {
            Vector3 start = origin + new Vector3(-tileSize/2f, i * tileSize - tileSize/2f, 0f);
            Vector3 end = origin + new Vector3(width - tileSize/2f, i * tileSize - tileSize/2f, 0f);
            Gizmos.DrawLine(start, end);
        }
    }
}