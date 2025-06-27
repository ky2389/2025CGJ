using UnityEngine;

public class CircularExhibit : ExhibitBase
{
    [Header("Circular Movement Settings")]
    public int circleSize = 3; // Size of the square movement pattern (3x3)
    
    // Movement pattern for 3x3 square: Right -> Down -> Left -> Up
    private Vector2Int[] movementPattern = new Vector2Int[]
    {
        Vector2Int.right,  // Move right (top edge)
        Vector2Int.right,  // Continue right
        Vector2Int.down,   // Move down (right edge)
        Vector2Int.down,   // Continue down
        Vector2Int.left,   // Move left (bottom edge)
        Vector2Int.left,   // Continue left
        Vector2Int.up,     // Move up (left edge)
        Vector2Int.up      // Continue up (completes the square)
    };
    
    protected override void Awake()
    {
        exhibitColor = Color.green; // Green color for circular exhibits
        base.Awake();
    }
    
    public override void Initialize(Vector2Int startPosition)
    {
        base.Initialize(startPosition);
        patternStep = 0; // Start at beginning of pattern
    }
    
    public override Vector2Int GetNextPosition()
    {
        if (movementPattern.Length == 0) return gridPosition;
        
        Vector2Int direction = movementPattern[patternStep % movementPattern.Length];
        return gridPosition + direction;
    }
    
    protected override void AdvancePattern()
    {
        patternStep = (patternStep + 1) % movementPattern.Length;
    }
    
    // Debug info for pattern state
    public string GetPatternInfo()
    {
        if (movementPattern.Length == 0) return "No pattern";
        
        Vector2Int currentDirection = movementPattern[patternStep % movementPattern.Length];
        string directionName = "Unknown";
        
        if (currentDirection == Vector2Int.right) directionName = "Right";
        else if (currentDirection == Vector2Int.down) directionName = "Down";
        else if (currentDirection == Vector2Int.left) directionName = "Left";
        else if (currentDirection == Vector2Int.up) directionName = "Up";
        
        return $"Circular movement - Direction: {directionName}, Step: {patternStep}/{movementPattern.Length}";
    }
}