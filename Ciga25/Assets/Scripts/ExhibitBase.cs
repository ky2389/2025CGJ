using UnityEngine;

public abstract class ExhibitBase : MonoBehaviour
{
    [Header("Exhibit Settings")]
    [SerializeField] protected Color exhibitColor = Color.red;
    
    protected Vector2Int gridPosition;
    protected SpriteRenderer spriteRenderer;
    protected int patternStep = 0; // Current step in movement pattern
    [Header("Animation Settings")]
    [SerializeField] protected Sprite frame1;
    [SerializeField] protected Sprite frame2;
    [SerializeField] protected float animationSpeed = 1f; // Time between frames

    private float animationTimer = 0f;
    private bool isFrame1 = true;

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
    }
    
    // Abstract method for getting next position in movement pattern
    public abstract Vector2Int GetNextPosition();
    
    // Move to next position in pattern
    public virtual void MoveToNextPosition()
    {
        Vector2Int nextPos = GetNextPosition();
        
        // Check if movement is blocked by walls or obstacles
        if (GridManager.Instance.IsWalkablePosition(nextPos))
        {
            gridPosition = nextPos;
            transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
            AdvancePattern();
        }
        else
        {
            // Blocked by wall or obstacle, don't advance pattern step
            Debug.Log($"Exhibit at {gridPosition} blocked by wall/obstacle, trying to move to {nextPos}");
        }
    }
    
    // Set position directly (for when pushed by player)
    public virtual void SetPosition(Vector2Int newPosition)
    {
        if (GridManager.Instance.IsWalkablePosition(newPosition))
        {
            gridPosition = newPosition;
            transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
            // When pushed, continue from current pattern step
        }
        else
        {
            Debug.Log($"Cannot set exhibit position to {newPosition} - out of bounds or blocked by obstacle");
        }
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