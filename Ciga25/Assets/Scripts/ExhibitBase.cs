using UnityEngine;

public abstract class ExhibitBase : MonoBehaviour
{
    [Header("Exhibit Settings")]
    [SerializeField] protected Color exhibitColor = Color.red;
    
    [Header("Initial Position Marker")]
    [SerializeField] protected Sprite signSprite; // Sprite for marking initial position
    
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
    }
    protected virtual void Awake()
    {
        // Get the sprite renderer component (user will add it manually)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on exhibit! Please add it manually.");
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
    
    // Create a sign marker at the initial position
    protected virtual void CreateSignMarker(Vector2Int initialPosition)
    {
        if (signSprite == null) return;
        
        // Create the sign marker GameObject
        signMarker = new GameObject($"SignMarker_{initialPosition.x}_{initialPosition.y}");
        
        // Position it 15 units below the initial position on y-axis
        Vector3 signPosition = GridManager.Instance.GridToWorldPosition(initialPosition);
        signPosition.y -= 10f;
        signMarker.transform.position = signPosition;
        
        // Add sprite renderer and set the sign sprite
        SpriteRenderer signRenderer = signMarker.AddComponent<SpriteRenderer>();
        signRenderer.sprite = signSprite;
        
        signMarker.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        signRenderer.sortingOrder = 1;
        
        Debug.Log($"Created sign marker for exhibit at {initialPosition}");
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
    public virtual void MoveToNextPosition()
    {
        Vector2Int nextPos = GetNextPosition();
        
        // Check if movement is blocked by walls, obstacles, or other entities
        if (CanMoveToPosition(nextPos))
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
            Debug.LogWarning($"Cannot set exhibit position to {newPosition} - position occupied by another entity");
            return;
        }
        
        // Position is valid, update it
        gridPosition = newPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        // When pushed, continue from current pattern step
    }
    
    // Advance to next step in movement pattern
    protected abstract void AdvancePattern();
    
    public Vector2Int GridPosition
    {
        get { return gridPosition; }
    }
    
    public int PatternStep
    {
        get { return patternStep; }
    }
}