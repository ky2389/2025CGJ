using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Color playerColor = Color.blue;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    private Vector2Int gridPosition;
    private Vector2Int lastDirection = Vector2Int.down;
    private bool isPushing = false;

    private SpriteRenderer spriteRenderer;

    // 新增缓存动画状态变量
    private Vector2Int cachedDirection = Vector2Int.down;
    private bool cachedIsPushing = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("SpriteRenderer component missing!");

        if (animator == null)
            animator = GetComponent<Animator>();

        float scale = GridManager.Instance.TileSize / 32f;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void Update()
    {
        HandleInput();
        // Debug状态打印可根据需要开启
        // PrintPlayerState();
    }

    public void Initialize(Vector2Int startPosition)
    {
        gridPosition = startPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);

        // 初始化缓存状态
        cachedDirection = lastDirection;
        cachedIsPushing = false;

        ApplyCachedAnimationState();
    }

    public void HandleInput()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameEnded()) return;

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
        {
            Vector2Int newPos = gridPosition + direction;

            if (GridManager.Instance.IsWalkablePosition(newPos))
            {
                // Check if we're pushing something and if it can be pushed
                if (GameManager.Instance.IsPushableAtPosition(newPos))
                {
                    if (!CanPushEntityAtPosition(newPos, direction))
                    {
                        Debug.Log("Blocked by wall or obstacle.");
                        return;
                    }
                }
                
                lastDirection = direction;
                
                // 这里不直接更新动画方向和IsPushing
                // 改为缓存动画状态，动画切换滞后一帧由GameManager控制
                bool isPushAction = GameManager.Instance.IsPushableAtPosition(newPos);
                CacheAnimationState(direction, isPushAction);

                GameManager.Instance.ProcessPlayerInput(direction);
            }
            else
            {
                Debug.Log("Blocked by wall or obstacle.");
            }
        }
    }

    // Check if an entity at the given position can be pushed in the given direction
    private bool CanPushEntityAtPosition(Vector2Int position, Vector2Int direction)
    {
        if (GameManager.Instance == null) return false;
        
        Vector2Int pushTargetPos = position + direction;
        
        // Check if the target position is walkable
        if (!GridManager.Instance.IsWalkablePosition(pushTargetPos))
        {
            return false;
        }
        
        // Check what type of entity is at the target position
        if (GameManager.Instance.IsPositionOccupied(pushTargetPos))
        {
            // Allow pushing an exhibit into another exhibit (will cause collision and game over)
            if (GameManager.Instance.IsExhibitAtPosition(position) && GameManager.Instance.IsExhibitAtPosition(pushTargetPos))
            {
                return true;
            }
            
            // Prevent pushing into candle holders, player, or other non-exhibit entities
            return false;
        }
        
        return true;
    }

    // 缓存动画状态（方向和推动状态）
    public void CacheAnimationState(Vector2Int direction, bool pushing)
    {
        cachedDirection = direction;
        cachedIsPushing = pushing;
    }

    // 在下一次行动开始时调用，应用缓存的动画状态
    public void ApplyCachedAnimationState()
    {
        UpdateAnimatorDirection(cachedDirection);

        if (animator != null)
            animator.SetBool("IsPushing", cachedIsPushing);

        // 同步当前状态变量
        isPushing = cachedIsPushing;
        lastDirection = cachedDirection;
    }

    private void UpdateAnimatorDirection(Vector2Int direction)
    {
        if (animator == null || direction == Vector2Int.zero) return;

        // Reset all direction bools
        animator.SetBool("FacingUp", false);
        animator.SetBool("FacingDown", false);
        animator.SetBool("FacingLeft", false);
        animator.SetBool("FacingRight", false);

        if (direction == Vector2Int.up)
            animator.SetBool("FacingUp", true);
        else if (direction == Vector2Int.down)
            animator.SetBool("FacingDown", true);
        else if (direction == Vector2Int.left)
            animator.SetBool("FacingLeft", true);
        else if (direction == Vector2Int.right)
            animator.SetBool("FacingRight", true);
    }

    public void MoveToPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        
        // 不在这里结束推动动画，交由GameManager下一行动时更新
    }

    public Vector2Int GridPosition => gridPosition;
    public Vector2Int LastDirection => lastDirection;

    private void PrintPlayerState()
    {
        string direction = "None";

        if (animator.GetBool("FacingUp"))
            direction = "Up";
        else if (animator.GetBool("FacingDown"))
            direction = "Down";
        else if (animator.GetBool("FacingLeft"))
            direction = "Left";
        else if (animator.GetBool("FacingRight"))
            direction = "Right";

        bool pushing = animator.GetBool("IsPushing");

        Debug.Log($"[Player State] Direction: {direction}, IsPushing: {pushing}");
    }
}
