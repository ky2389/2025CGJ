using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Color playerColor = Color.blue;
    
    [Header("Animation Settings")]
    [SerializeField] private Animator animator; // Reference to the Animator component
    
    private Vector2Int gridPosition;
    private SpriteRenderer spriteRenderer;
    private Vector2Int lastDirection = Vector2Int.down; // Default facing direction
    private bool isPushing = false;
    
    private void Awake()
    {
        // Get the sprite renderer component (user will add it manually)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on player! Please add it manually.");
        }
        
        // Get the animator component if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Scale to fit tile size (32x32 sprites need to be 100x larger)
        float scale = GridManager.Instance.TileSize / 32f; // Scale factor for 32x32 sprites
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    public void Initialize(Vector2Int startPosition)
    {
        gridPosition = startPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        
        // Set initial animation state
        if (animator != null)
        {
            UpdateAnimationDirection(lastDirection);
            animator.SetBool("IsPushing", false);
        }
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
        UpdateAnimationDirection(direction);
        // Send input to game manager if valid direction
        if (direction != Vector2Int.zero)
        {
            Vector2Int newPos = gridPosition + direction;
            
            // Check bounds and obstacles
            if (GridManager.Instance.IsWalkablePosition(newPos))
            {
                // Update direction for animation
                lastDirection = direction;
                UpdateAnimationDirection(direction);
                
                // Check if this is a push action
                bool isPushAction = GameManager.Instance.IsPushableAtPosition(newPos);
                
                if (isPushAction)
                {
                    // Trigger push animation
                    StartPushAnimation();
                }
                
                GameManager.Instance.ProcessPlayerInput(direction);
            }
            else
            {
                Debug.Log("Cannot move - blocked by wall or obstacle!");
            }
        }
    }
    
    private void UpdateAnimationDirection(Vector2Int direction)
    {
        if (animator == null) return;
        
        // Set direction parameters for the animator
        animator.SetFloat("DirectionX", direction.x);
        animator.SetFloat("DirectionY", direction.y);
        
        // Set specific direction booleans
        animator.SetBool("FacingUp", direction.y > 0);
        animator.SetBool("FacingDown", direction.y < 0);
        animator.SetBool("FacingLeft", direction.x < 0);
        animator.SetBool("FacingRight", direction.x > 0);
    }
    
    private void StartPushAnimation()
    {
        if (animator == null) return;
        
        isPushing = true;
        animator.SetBool("IsPushing", true);
        
        // Return to idle after push animation completes
        // This will be handled by the animator transitions
    }
    
    public void EndPushAnimation()
    {
        if (animator == null) return;
        
        isPushing = false;
        animator.SetBool("IsPushing", false);
    }
    
    public void MoveToPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        
        // End push animation after movement completes
        if (isPushing)
        {
            EndPushAnimation();
        }
    }
    
    public Vector2Int GridPosition
    {
        get { return gridPosition; }
    }
    
    public Vector2Int LastDirection
    {
        get { return lastDirection; }
    }
}