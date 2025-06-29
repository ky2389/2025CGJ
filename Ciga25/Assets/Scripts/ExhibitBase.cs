using UnityEngine;

public abstract class ExhibitBase : MonoBehaviour
{
    [Header("Exhibit Settings")]
    [SerializeField] protected Color exhibitColor = Color.red;
    
    [Header("Initial Position Marker")]
    [SerializeField] protected Sprite signSprite; // Sprite for marking initial position
    
    [Header("Movement Arrow")]
    [SerializeField] protected GameObject movementArrow; // Arrow showing next move direction
    [SerializeField] protected float flickerSpeed = 5f; // Speed of alpha flickering
    
    protected Vector2Int gridPosition;
    protected SpriteRenderer spriteRenderer;
    protected int patternStep = 0; // Current step in movement pattern
    [Header("Animation Settings")]
    [SerializeField] protected Sprite frame1;
    [SerializeField] protected Sprite frame2;
    [SerializeField] protected float animationSpeed = 1f; // Time between frames

    private float animationTimer = 0f;
    private bool isFrame1 = true;
    private GameObject signMarker; // Reference to the sign marker GameObject
    private SpriteRenderer arrowSpriteRenderer; // Reference to arrow's sprite renderer
    private float flickerTimer = 0f; // Timer for flickering effect

    protected virtual void Update()
    {
        // Simple sprite swapping animation
        animationTimer += Time.deltaTime;
        if (animationTimer >= animationSpeed)
        {
            animationTimer = 0f;
            isFrame1 = !isFrame1;
            spriteRenderer.sprite = isFrame1 ? frame1 : frame2;
        }
        
        // Handle arrow flickering
        UpdateArrowFlickering();
    }
    
    // Update arrow flickering effect
    private void UpdateArrowFlickering()
    {
        if (movementArrow != null && movementArrow.activeInHierarchy && arrowSpriteRenderer != null)
        {
            flickerTimer += Time.deltaTime * flickerSpeed;
            
            // Create a sine wave flickering effect (0.3 to 1.0 alpha)
            float alpha = 0.3f + (0.7f * (Mathf.Sin(flickerTimer) + 1f) / 2f);
            
            Color currentColor = arrowSpriteRenderer.color;
            arrowSpriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }

    protected virtual void Awake()
    {
        // Get the sprite renderer component (user will add it manually)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on exhibit! Please add it manually.");
        }
        
        // Get the arrow sprite renderer reference
        if (movementArrow != null)
        {
            arrowSpriteRenderer = movementArrow.GetComponent<SpriteRenderer>();
            if (arrowSpriteRenderer == null)
            {
                Debug.LogWarning("SpriteRenderer component not found on movement arrow!");
            }
        }
        
        // Scale to fit tile size (32x32 sprites need to be 100x larger)
        float scale = GridManager.Instance.TileSize / 32f; // Scale factor for 32x32 sprites
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    public virtual void Initialize(Vector2Int startPosition)
    {
        gridPosition = startPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        patternStep = 0;
        
        // Create the sign marker at initial position
        CreateSignMarker(startPosition);
    }
    
    public virtual void Initialize(Vector2Int startPosition, Vector2Int targetPosition)
    {
        gridPosition = startPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        patternStep = 0;
        
        // Create the sign marker at target position
        CreateSignMarker(targetPosition);
    }
    
    // Create a sign marker at the specified position
    protected virtual void CreateSignMarker(Vector2Int position)
    {
        if (signSprite == null) return;
        
        // Create the sign marker GameObject
        signMarker = new GameObject($"SignMarker_{position.x}_{position.y}");
        
        // Position it 10 units below the specified position on y-axis
        Vector3 signPosition = GridManager.Instance.GridToWorldPosition(position);
        signPosition.y -= 10f;
        signMarker.transform.position = signPosition;
        
        // Add sprite renderer and set the sign sprite
        SpriteRenderer signRenderer = signMarker.AddComponent<SpriteRenderer>();
        signRenderer.sprite = signSprite;
        
        signMarker.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        signRenderer.sortingOrder = 1;
        
        Debug.Log($"Created sign marker at target position {position}");
    }
    
    // Clean up the sign marker when the exhibit is destroyed
    protected virtual void OnDestroy()
    {
        if (signMarker != null)
        {
            DestroyImmediate(signMarker);
        }
    }
    
    // Abstract method for getting next position in movement pattern
    public abstract Vector2Int GetNextPosition();
    
    // Move to next position in pattern
    public virtual void MoveToNextPosition(bool forceMove = false)
    {
        Vector2Int nextPos = GetNextPosition();
        
        // Check if movement is blocked by walls, obstacles, or other entities
        if (forceMove || CanMoveToPosition(nextPos))
        {
            gridPosition = nextPos;
            transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
            AdvancePattern();
        }
        else
        {
            // Blocked by wall, obstacle, or other entity, don't advance pattern step
            Debug.Log($"Exhibit at {gridPosition} blocked, trying to move to {nextPos}");
            AdvancePattern();
        }
    }
    
    // Check if the exhibit can move to the given position
    protected virtual bool CanMoveToPosition(Vector2Int position)
    {
        // Check if position is walkable (within bounds and not blocked by obstacle)
        if (!GridManager.Instance.IsWalkablePosition(position))
        {
            return false;
        }
        
        // Check if position is occupied by another entity
        if (GameManager.Instance != null && GameManager.Instance.IsPositionOccupied(position))
        {
            return false;
        }
        
        return true;
    }
    
    // Check if the exhibit can move to the given position (for arrow display - ignores player)
    protected virtual bool CanMoveToPositionForArrow(Vector2Int position)
    {
        // Check if position is walkable (within bounds and not blocked by obstacle)
        if (!GridManager.Instance.IsWalkablePosition(position))
        {
            return false;
        }
        
        // Check if position is occupied by another entity (excluding player)
        if (GameManager.Instance != null)
        {
            // Check if any exhibit is at this position
            if (GameManager.Instance.IsExhibitAtPosition(position))
            {
                return false;
            }
            
            // Check if any candle holder is at this position
            if (GameManager.Instance.IsCandleHolderAtPosition(position))
            {
                return false;
            }
            
            // Note: We don't check for player position since player will move next turn
        }
        
        return true;
    }
    
    // Set position directly (for when pushed by player)
    public virtual void SetPosition(Vector2Int newPosition)
    {
        // Validate the new position before setting it
        if (!GridManager.Instance.IsValidPosition(newPosition))
        {
            Debug.LogWarning($"Cannot set exhibit position to {newPosition} - position out of bounds");
            return;
        }
        
        if (GridManager.Instance.IsPositionBlocked(newPosition))
        {
            Debug.LogWarning($"Cannot set exhibit position to {newPosition} - position blocked by obstacle");
            return;
        }
        
        // Check if the new position would overlap with any entity
        if (GameManager.Instance != null && GameManager.Instance.IsPositionOccupied(newPosition))
        {
            // Allow overlapping with other exhibits (collision will be detected by GameManager)
            if (!GameManager.Instance.IsExhibitAtPosition(newPosition))
            {
                Debug.LogWarning($"Cannot set exhibit position to {newPosition} - position occupied by non-exhibit entity");
                return;
            }
        }
        
        // Position is valid, update it
        gridPosition = newPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        // When pushed, continue from current pattern step
    }
    
    // Advance to next step in movement pattern
    protected abstract void AdvancePattern();
    
    // Update the movement arrow to show next move direction
    public virtual void UpdateMovementArrow()
    {
        if (movementArrow == null) return;
        
        Vector2Int nextPos = GetNextPosition();
        
        // Only show arrow if the exhibit can actually move (ignoring player position)
        if (CanMoveToPositionForArrow(nextPos))
        {
            movementArrow.SetActive(true);
            
            // Calculate direction
            Vector2Int direction = nextPos - gridPosition;
            
            // Position arrow based on direction
            Vector3 arrowPosition = Vector3.zero;
            float arrowRotation = 0f;
            
            if (direction == Vector2Int.up)
            {
                arrowPosition = new Vector3(0f, 20f, 0f);
                arrowRotation = 0f; // Point up (prefab is already -90° rotated)
            }
            else if (direction == Vector2Int.down)
            {
                arrowPosition = new Vector3(0f, -20f, 0f);
                arrowRotation = 180f; // Point down
            }
            else if (direction == Vector2Int.left)
            {
                arrowPosition = new Vector3(-14.2f, 0.8f, 0f);
                arrowRotation = 90f; // Point left
            }
            else if (direction == Vector2Int.right)
            {
                arrowPosition = new Vector3(14.2f, 0.8f, 0f);
                arrowRotation = 270f; // Point right
            }
            
            // Apply position and rotation
            movementArrow.transform.localPosition = arrowPosition;
            movementArrow.transform.localRotation = Quaternion.Euler(0f, 0f, arrowRotation);
        }
        else
        {
            // Hide arrow if blocked
            movementArrow.SetActive(false);
        }
    }
    
    // Hide the movement arrow (called when exhibit is frozen or blocked)
    public virtual void HideMovementArrow()
    {
        if (movementArrow != null)
        {
            movementArrow.SetActive(false);
        }
    }
    
    public Vector2Int GridPosition
    {
        get { return gridPosition; }
    }
    
    public int PatternStep
    {
        get { return patternStep; }
    }
}