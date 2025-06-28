using UnityEngine;
using System.Collections.Generic;

public class CandleHolder : MonoBehaviour
{
    [Header("Candle Holder Settings")]
    [SerializeField] private Color candleColor = Color.yellow;
    [SerializeField] private Color lightAreaColor = new Color(1f, 1f, 0.5f, 0.3f); // Semi-transparent yellow
    [SerializeField] private int lightRadius = 1; // Radius of lighting effect (1 = 3x3 area)
    
    private Vector2Int gridPosition;
    private SpriteRenderer spriteRenderer;
    private List<GameObject> lightAreaIndicators = new List<GameObject>();
    private bool isLit = true; // Whether the candle is currently lit
    
    private void Awake()
    {
        // Get the sprite renderer component (user will add it manually)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on candle holder! Please add it manually.");
        }
        
        // Scale to fit tile size (32x32 sprites need to be 100x larger)
        float scale = GridManager.Instance.TileSize / 32f; // Scale factor for 32x32 sprites
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    public void Initialize(Vector2Int startPosition)
    {
        gridPosition = startPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        
        // Create light area indicators
        CreateLightAreaIndicators();
    }
    
    private void CreateLightAreaIndicators()
    {
        // Clear existing indicators
        ClearLightAreaIndicators();
        
        if (!isLit) return;
        
        // Create indicators for the 3x3 area around the candle
        for (int x = -lightRadius; x <= lightRadius; x++)
        {
            for (int y = -lightRadius; y <= lightRadius; y++)
            {
                Vector2Int lightPos = gridPosition + new Vector2Int(x, y);
                
                // Only create indicators for valid positions
                if (GridManager.Instance.IsValidPosition(lightPos))
                {
                    CreateLightIndicator(lightPos);
                }
            }
        }
    }
    
    private void UpdateLightIndicatorPositions()
    {
        if (!isLit) return;
        
        // If the number of indicators doesn't match expected positions, recreate them
        int expectedIndicatorCount = 0;
        for (int x = -lightRadius; x <= lightRadius; x++)
        {
            for (int y = -lightRadius; y <= lightRadius; y++)
            {
                Vector2Int lightPos = gridPosition + new Vector2Int(x, y);
                if (GridManager.Instance.IsValidPosition(lightPos))
                {
                    expectedIndicatorCount++;
                }
            }
        }
        
        // If count doesn't match, recreate all indicators
        if (expectedIndicatorCount != lightAreaIndicators.Count)
        {
            CreateLightAreaIndicators();
            return;
        }
        
        int indicatorIndex = 0;
        
        // Update positions for the 3x3 area around the candle
        for (int x = -lightRadius; x <= lightRadius; x++)
        {
            for (int y = -lightRadius; y <= lightRadius; y++)
            {
                Vector2Int lightPos = gridPosition + new Vector2Int(x, y);
                
                // Only update indicators for valid positions
                if (GridManager.Instance.IsValidPosition(lightPos) && indicatorIndex < lightAreaIndicators.Count)
                {
                    Vector3 worldPos = GridManager.Instance.GridToWorldPosition(lightPos);
                    lightAreaIndicators[indicatorIndex].transform.position = worldPos;
                    indicatorIndex++;
                }
            }
        }
    }
    
    private void CreateLightIndicator(Vector2Int position)
    {
        // Create a simple GameObject to represent the lit area
        GameObject indicator = new GameObject($"LightIndicator_{position.x}_{position.y}");
        // Don't parent to transform to avoid inheriting scale
        indicator.transform.SetParent(null);
        
        // Position the indicator
        Vector3 worldPos = GridManager.Instance.GridToWorldPosition(position);
        indicator.transform.position = worldPos;
        
        // Add sprite renderer for visual representation
        SpriteRenderer indicatorRenderer = indicator.AddComponent<SpriteRenderer>();
        
        // Create a simple texture for the light area
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, lightAreaColor);
        texture.Apply();
        
        // Create sprite from texture
        Sprite indicatorSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        indicatorRenderer.sprite = indicatorSprite;
        
        // Scale to fit tile size and set sorting order to be behind other objects
        float scale = GridManager.Instance.TileSize * 0.95f;
        indicator.transform.localScale = new Vector3(scale, scale, 1f);
        indicatorRenderer.sortingOrder = -1; // Render behind other objects
        
        lightAreaIndicators.Add(indicator);
    }
    
    private void ClearLightAreaIndicators()
    {
        foreach (GameObject indicator in lightAreaIndicators)
        {
            if (indicator != null)
            {
                DestroyImmediate(indicator);
            }
        }
        lightAreaIndicators.Clear();
    }
    
    // Check if a position is within the light area
    public bool IsPositionInLightArea(Vector2Int position)
    {
        if (!isLit) return false;
        
        int distanceX = Mathf.Abs(position.x - gridPosition.x);
        int distanceY = Mathf.Abs(position.y - gridPosition.y);
        
        return distanceX <= lightRadius && distanceY <= lightRadius;
    }
    
    // Set position directly (for when pushed by player)
    public void SetPosition(Vector2Int newPosition)
    {
        if (GridManager.Instance.IsWalkablePosition(newPosition))
        {
            gridPosition = newPosition;
            transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
            
            // Update light area indicators
            UpdateLightIndicatorPositions();
        }
        else
        {
            Debug.Log($"Cannot set candle holder position to {newPosition} - out of bounds or blocked by obstacle");
            // Clear indicators when blocked to prevent old indicators from remaining
            ClearLightAreaIndicators();
            CreateLightAreaIndicators();
        }
    }
    
    // Toggle the light on/off
    public void ToggleLight()
    {
        isLit = !isLit;
        CreateLightAreaIndicators();
    }
    
    // Set light state
    public void SetLightState(bool lit)
    {
        isLit = lit;
        CreateLightAreaIndicators();
    }
    
    public Vector2Int GridPosition
    {
        get { return gridPosition; }
    }
    
    public bool IsLit
    {
        get { return isLit; }
    }
    
    private void OnDestroy()
    {
        ClearLightAreaIndicators();
    }
} 