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
        PrintPlayerState();
    }

    public void Initialize(Vector2Int startPosition)
    {
        gridPosition = startPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);

        if (animator != null)
        {
            UpdateAnimatorDirection(lastDirection);
            animator.SetBool("IsPushing", false);
        }
    }

    private void HandleInput()
    {
        if (GameManager.Instance.IsGameEnded()) return;

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
                lastDirection = direction;
                UpdateAnimatorDirection(direction);

                bool isPushAction = GameManager.Instance.IsPushableAtPosition(newPos);
                if (isPushAction)
                {
                    StartPushAnimation();
                }

                GameManager.Instance.ProcessPlayerInput(direction);
            }
            else
            {
                Debug.Log("Blocked by wall or obstacle.");
            }
           

        }
        
    }

    private void UpdateAnimatorDirection(Vector2Int direction)
    {
        if (animator == null || direction == Vector2Int.zero) return;

        // Reset all
        animator.SetBool("FacingUp", false);
        animator.SetBool("FacingDown", false);
        animator.SetBool("FacingLeft", false);
        animator.SetBool("FacingRight", false);

        // Set current direction
        if (direction == Vector2Int.up)
            animator.SetBool("FacingUp", true);
        else if (direction == Vector2Int.down)
            animator.SetBool("FacingDown", true);
        else if (direction == Vector2Int.left)
            animator.SetBool("FacingLeft", true);
        else if (direction == Vector2Int.right)
            animator.SetBool("FacingRight", true);
    }

    private void StartPushAnimation()
    {
        if (animator == null) return;

        isPushing = true;
        animator.SetBool("IsPushing", true);
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

        if (isPushing)
        {
            EndPushAnimation();
        }
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
