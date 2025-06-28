using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 9;
    [SerializeField] private int gridHeight = 9;
    [SerializeField] private float tileSize = 32f;
    
    [Header("Visual Debug")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = Color.white;
    
    private Vector3 gridOrigin;
    private bool isInitialized = false;
    private List<ObstacleBase> obstacles = new List<ObstacleBase>();
    
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
        UpdateGridOrigin();
        isInitialized = true;
    }
    
    private void UpdateGridOrigin()
    {
        gridOrigin = new Vector3(
            -(gridWidth * tileSize) / 2f + tileSize / 2f,
            -(gridHeight * tileSize) / 2f + tileSize / 2f,
            0f
        );
    }
    
    // Convert grid coordinates to world position
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        if (!isInitialized)
        {
            UpdateGridOrigin();
        }
        return gridOrigin + new Vector3(gridPos.x * tileSize, gridPos.y * tileSize, 0f);
    }
    
    // Convert world position to grid coordinates
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        if (!isInitialized)
        {
            UpdateGridOrigin();
        }
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
    
    // Check if position is blocked by an obstacle
    public bool IsPositionBlocked(Vector2Int gridPos)
    {
        if(obstacles.Count == 0)
        {
            return false;
        }
        foreach (ObstacleBase obstacle in obstacles)
        {
            if (obstacle.GridPosition == gridPos)
            {
                return true;
            }
        }
        return false;
    }
    
    // Check if position is walkable (within bounds and not blocked by obstacle)
    public bool IsWalkablePosition(Vector2Int gridPos)
    {
        return IsValidPosition(gridPos) && !IsPositionBlocked(gridPos);
    }
    
    // Register an obstacle
    public void RegisterObstacle(ObstacleBase obstacle)
    {
        if (!obstacles.Contains(obstacle))
        {
            obstacles.Add(obstacle);
        }
    }
    
    // Unregister an obstacle
    public void UnregisterObstacle(ObstacleBase obstacle)
    {
        obstacles.Remove(obstacle);
    }
    
    // Get distance between two grid positions
    public int GetGridDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
    }
    
    // Public getters for grid settings
    public int GridWidth
    {
        get { return gridWidth; }
    }
    
    public int GridHeight
    {
        get { return gridHeight; }
    }
    
    public float TileSize
    {
        get { return tileSize; }
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