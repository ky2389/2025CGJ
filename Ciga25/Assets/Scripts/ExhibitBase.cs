using UnityEngine;

public abstract class ExhibitBase : MonoBehaviour
{
    [Header("Exhibit Settings")]
    public Color exhibitColor = Color.red;
    
    protected Vector2Int gridPosition;
    protected SpriteRenderer spriteRenderer;
    protected int patternStep = 0; // Current step in movement pattern
    
    protected virtual void Awake()
    {
        // Setup visual representation
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        CreateExhibitSprite();
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
        
        // Check if movement is blocked by walls
        if (GridManager.Instance.IsValidPosition(nextPos))
        {
            gridPosition = nextPos;
            transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
            AdvancePattern();
        }
        else
        {
            // Blocked by wall, don't advance pattern step
            Debug.Log($"Exhibit at {gridPosition} blocked by wall, trying to move to {nextPos}");
        }
    }
    
    // Set position directly (for when pushed by player)
    public virtual void SetPosition(Vector2Int newPosition)
    {
        if (GridManager.Instance.IsValidPosition(newPosition))
        {
            gridPosition = newPosition;
            transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
            // When pushed, continue from current pattern step
        }
        else
        {
            Debug.Log($"Cannot set exhibit position to {newPosition} - out of bounds");
        }
    }
    
    // Advance to next step in movement pattern
    protected abstract void AdvancePattern();
    
    private void CreateExhibitSprite()
    {
        // Create a simple texture for the exhibit
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, exhibitColor);
        texture.Apply();
        
        // Create sprite from texture
        Sprite exhibitSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        spriteRenderer.sprite = exhibitSprite;
        
        // Scale to fit tile size
        float scale = GridManager.Instance.tileSize * 0.7f; // 70% of tile size
        transform.localScale = new Vector3(scale, scale, 1f);
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