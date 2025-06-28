using UnityEngine;

public class HorizontalExhibit : ExhibitBase
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] private int moveDistance = 3; // How many tiles to move in each direction
    
    [Header("Animation Settings")]
    [SerializeField] private Animator animator; // Reference to the Animator component
    
    private bool movingRight = true; // Current movement direction
    private int currentMoveCount = 0; // Current steps in current direction
    
    protected override void Awake()
    {
        // Set the color for horizontal exhibits (red)
        exhibitColor = Color.red;
        base.Awake();
        
        // Get the animator component if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    public override void Initialize(Vector2Int startPosition)
    {
        base.Initialize(startPosition);
        movingRight = true;
        currentMoveCount = 0;
        
        // Start the animation
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }
    
    public override Vector2Int GetNextPosition()
    {
        Vector2Int direction = movingRight ? Vector2Int.right : Vector2Int.left;
        return gridPosition + direction;
    }
    
    protected override void AdvancePattern()
    {
        currentMoveCount++;
        
        // Check if we need to change direction
        if (currentMoveCount >= moveDistance)
        {
            movingRight = !movingRight; // Change direction
            currentMoveCount = 0; // Reset move counter
            
            // Update animation direction if needed
            if (animator != null)
            {
                animator.SetBool("MovingRight", movingRight);
            }
        }
    }
    
    // Debug info for pattern state
    public string GetPatternInfo()
    {
        return $"Moving {(movingRight ? "Right" : "Left")}, Step: {currentMoveCount}/{moveDistance}";
    }
    
    // Public getter for move distance
    public int MoveDistance
    {
        get { return moveDistance; }
    }
}