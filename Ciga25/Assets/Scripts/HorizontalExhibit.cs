using UnityEngine;

public class HorizontalExhibit : ExhibitBase
{
    [Header("Horizontal Movement Settings")]
    public int moveDistance = 3; // How many tiles to move in each direction
    
    private bool movingRight = true; // Current movement direction
    private int currentMoveCount = 0; // Current steps in current direction
    
    protected override void Awake()
    {
        exhibitColor = Color.red; // Red color for horizontal exhibits
        base.Awake();
    }
    
    public override void Initialize(Vector2Int startPosition)
    {
        base.Initialize(startPosition);
        movingRight = true;
        currentMoveCount = 0;
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
        }
    }
    
    // Debug info for pattern state
    public string GetPatternInfo()
    {
        return $"Moving {(movingRight ? "Right" : "Left")}, Step: {currentMoveCount}/{moveDistance}";
    }
}