using UnityEngine;
using System.Collections.Generic;

public class CircularExhibit : ExhibitBase
{
    [Header("Circular Movement Settings")]
    [SerializeField] private int circleSize = 3; // Size of the square movement pattern (3x3)
    
    // Movement pattern will be generated dynamically based on circleSize
    private List<Vector2Int> movementPattern = new List<Vector2Int>();
    
    protected override void Awake()
    {
        // Set the color for circular exhibits (green)
        exhibitColor = Color.green;
        base.Awake();
        GenerateMovementPattern();
    }
    
    public override void Initialize(Vector2Int startPosition)
    {
        base.Initialize(startPosition);
        patternStep = 0; // Start at beginning of pattern
    }
    
    private void GenerateMovementPattern()
    {
        movementPattern.Clear();
        
        if (circleSize <= 0)
        {
            Debug.LogError("Circle size must be greater than 0!");
            return;
        }
        
        // Generate a square pattern: Right -> Down -> Left -> Up
        // Each direction is repeated (circleSize - 1) times to complete the square
        
        // Right movement (top edge)
        for (int i = 0; i < circleSize - 1; i++)
        {
            movementPattern.Add(Vector2Int.right);
        }
        
        // Down movement (right edge)
        for (int i = 0; i < circleSize - 1; i++)
        {
            movementPattern.Add(Vector2Int.down);
        }
        
        // Left movement (bottom edge)
        for (int i = 0; i < circleSize - 1; i++)
        {
            movementPattern.Add(Vector2Int.left);
        }
        
        // Up movement (left edge)
        for (int i = 0; i < circleSize - 1; i++)
        {
            movementPattern.Add(Vector2Int.up);
        }
        
        Debug.Log($"Generated circular pattern with {movementPattern.Count} steps for circle size {circleSize}");
    }
    
    public override Vector2Int GetNextPosition()
    {
        if (movementPattern.Count == 0) return gridPosition;
        
        Vector2Int direction = movementPattern[patternStep % movementPattern.Count];
        return gridPosition + direction;
    }
    
    protected override void AdvancePattern()
    {
        if (movementPattern.Count > 0)
        {
            patternStep = (patternStep + 1) % movementPattern.Count;
        }
    }
    
    // Debug info for pattern state
    public string GetPatternInfo()
    {
        if (movementPattern.Count == 0) return "No pattern";
        
        Vector2Int currentDirection = movementPattern[patternStep % movementPattern.Count];
        string directionName = "Unknown";
        
        if (currentDirection == Vector2Int.right) directionName = "Right";
        else if (currentDirection == Vector2Int.down) directionName = "Down";
        else if (currentDirection == Vector2Int.left) directionName = "Left";
        else if (currentDirection == Vector2Int.up) directionName = "Up";
        
        return $"Circular movement - Direction: {directionName}, Step: {patternStep}/{movementPattern.Count}, Circle Size: {circleSize}";
    }
    
    // Public getter for circle size
    public int CircleSize
    {
        get { return circleSize; }
    }
}