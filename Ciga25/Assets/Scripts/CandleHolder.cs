using UnityEngine;
using System.Collections.Generic;

public class CandleHolder : MonoBehaviour
{
    [Header("Candle Holder Settings")]
    [SerializeField] private Color candleColor = Color.yellow;
    [SerializeField] private Color lightAreaColor = new Color(1f, 1f, 0.5f, 0.3f); // Semi-transparent yellow
    [SerializeField] private int lightRadius = 1; // Radius of lighting effect (1 = 3x3 area)
    
    [Header("Flame Settings")]
    [SerializeField] private int maxFlameTurns = 5; // Turns before flame goes out
    [SerializeField] private Transform healthBar; // Reference to existing health bar child object
    [SerializeField] private Transform displayGroup; // Reference to existing health bar child object
    
    [Header("Sprite References")]
    [SerializeField] private Sprite litSprite; // Sprite when candle is lit
    [SerializeField] private Sprite extinguishedSprite; // Sprite when candle is extinguished
    [SerializeField] private SpriteRenderer candleSpriteRenderer; // Reference to the child sprite object's SpriteRenderer
    
    private Vector2Int gridPosition;
    private SpriteRenderer spriteRenderer;
    private List<GameObject> lightAreaIndicators = new List<GameObject>();
    private bool isLit = true; // Whether the candle is currently lit
    private int currentFlameTurns = 0; // Current turns the flame has been burning
    
    private void Awake()
    {
        // Get the sprite renderer component (user will add it manually)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on candle holder! Please add it manually.");
        }
        
        // Add collider for mouse interaction if not present
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f); // Adjust size as needed
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
        
        // Initialize health bar
        InitializeHealthBar();
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
        // Validate the new position before setting it
        if (!GridManager.Instance.IsValidPosition(newPosition))
        {
            Debug.LogWarning($"Cannot set candle holder position to {newPosition} - position out of bounds");
            return;
        }
        
        if (GridManager.Instance.IsPositionBlocked(newPosition))
        {
            Debug.LogWarning($"Cannot set candle holder position to {newPosition} - position blocked by obstacle");
            return;
        }
        
        // Check if the new position would overlap with any entity
        if (GameManager.Instance != null && GameManager.Instance.IsPositionOccupied(newPosition))
        {
            Debug.LogWarning($"Cannot set candle holder position to {newPosition} - position occupied by another entity");
            return;
        }
        
        // Position is valid, update it
        gridPosition = newPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        
        // Update light area indicators
        UpdateLightIndicatorPositions();
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
    
    // Set light radius
    public void SetLightRadius(int radius)
    {
        lightRadius = radius;
        CreateLightAreaIndicators();
    }
    
    // Set light color
    public void SetLightColor(Color color)
    {
        lightAreaColor = new Color(color.r, color.g, color.b, 0.3f); // Keep alpha at 0.3
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
    
    // Called at the end of each turn to update flame
    public void OnTurnEnd()
    {
        if (isLit)
        {
            currentFlameTurns++;
            UpdateHealthBar();
            
            // Check if flame should go out
            if (currentFlameTurns >= maxFlameTurns)
            {
                ExtinguishFlame();
            }
        }
    }
    
    // Extinguish the flame
    private void ExtinguishFlame()
    {
        isLit = false;
        currentFlameTurns = maxFlameTurns;
        ClearLightAreaIndicators();
        UpdateHealthBar();
        
        // Change to extinguished sprite
        if (extinguishedSprite != null && candleSpriteRenderer != null)
        {
            candleSpriteRenderer.sprite = extinguishedSprite;
        }
        
        Debug.Log($"Candle holder at {gridPosition} flame went out!");
    }
    
    // Relight the flame (called by player interaction)
    public void RelightFlame()
    {
        isLit = true;
        currentFlameTurns = 0;
        CreateLightAreaIndicators();
        UpdateHealthBar();
        
        // Change back to lit sprite
        if (litSprite != null && candleSpriteRenderer != null)
        {
            candleSpriteRenderer.sprite = litSprite;
        }
        
        Debug.Log($"Candle holder at {gridPosition} flame relit!");
    }
    
    // Initialize health bar display
    private void InitializeHealthBar()
    {
        if (healthBar == null) return;
        
        // Set initial health bar to full
        currentFlameTurns = 0;
        UpdateHealthBar();
        
        // Set initial lit sprite
        if (litSprite != null && candleSpriteRenderer != null)
        {
            candleSpriteRenderer.sprite = litSprite;
        }
        
        Debug.Log($"Initialized health bar for candle holder at {gridPosition}");
    }
    
    // Update health bar display
    private void UpdateHealthBar()
    {
        if (healthBar == null) return;
        
        // Calculate health percentage (0.0 to 1.0)
        float healthPercentage = (float)(maxFlameTurns - currentFlameTurns) / maxFlameTurns;
        
        // Update the health bar scale based on remaining flame turns
        // Scale the parent empty object
        healthBar.transform.localScale = new Vector3(healthPercentage, 1, 1);
        
        // Fix the sprite position to keep it anchored properly
        // The sprite should stay anchored to the left side as the parent scales
        SpriteRenderer healthBarSprite = healthBar.GetComponentInChildren<SpriteRenderer>();
        if (healthBarSprite != null)
        {
            // Get the sprite's bounds to calculate proper positioning
            Bounds spriteBounds = healthBarSprite.bounds;
            float spriteWidth = spriteBounds.size.x;
            
            // Calculate the offset needed to keep the sprite anchored to the left
            // This compensates for the scaling effect
            float offsetX = (spriteWidth * (1f - healthPercentage)) / 2f;
            
            // Apply the offset to keep the sprite anchored to the left
            Vector3 spriteLocalPos = healthBarSprite.transform.localPosition;
            healthBarSprite.transform.localPosition = new Vector3(-offsetX, spriteLocalPos.y, spriteLocalPos.z);
        }
        
        // Optional: Hide health bar completely when flame is out
        if (healthPercentage <= 0)
        {
            displayGroup.gameObject.SetActive(false);
        }
        else
        {
            displayGroup.gameObject.SetActive(true);
        }
    }
} 