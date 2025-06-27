using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Color playerColor = Color.blue;
    
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
        
        // Create a simple square sprite for the player
        CreatePlayerSprite();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    public void Initialize(Vector2Int startPosition)
    {
        gridPosition = startPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
    }
    
    private void HandleInput()
    {
        if (GameManager.Instance.IsGameEnded()) return;
        
        Vector2Int direction = Vector2Int.zero;
        
        // Get input direction
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = Vector2Int.right;
        
        // Send input to game manager if valid direction
        if (direction != Vector2Int.zero)
        {
            Vector2Int newPos = gridPosition + direction;
            
            // Check bounds and obstacles
            if (GridManager.Instance.IsWalkablePosition(newPos))
            {
                GameManager.Instance.ProcessPlayerInput(direction);
            }
            else
            {
                Debug.Log("Cannot move - blocked by wall or obstacle!");
            }
        }
    }
    
    public void MoveToPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
    }
    
    private void CreatePlayerSprite()
    {
        // Create a simple texture for the player
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, playerColor);
        texture.Apply();
        
        // Create sprite from texture
        Sprite playerSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        spriteRenderer.sprite = playerSprite;
        
        // Scale to fit tile size
        float scale = GridManager.Instance.TileSize * 0.8f; // 80% of tile size
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    public Vector2Int GridPosition
    {
        get { return gridPosition; }
    }
}