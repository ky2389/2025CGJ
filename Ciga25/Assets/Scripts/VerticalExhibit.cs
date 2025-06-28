using UnityEngine;
using System.Collections.Generic;

public class VerticalExhibit : ExhibitBase
{
    [Header("Vertical Movement Settings")]
    [SerializeField] private int moveDistance = 3; // 垂直移动的总步数

    private List<Vector2Int> movementPattern = new List<Vector2Int>();

    protected override void Awake()
    {
        exhibitColor = Color.grey;
        base.Awake();
        GenerateMovementPattern();
    }

    public override void Initialize(Vector2Int startPosition)
    {
        base.Initialize(startPosition);
        patternStep = 0;
    }

    private void GenerateMovementPattern()
    {
        movementPattern.Clear();

        if (moveDistance <= 1)
        {
            Debug.LogError("Vertical movement distance must be greater than 1");
            return;
        }

        // 向上移动 moveDistance - 1 格
        for (int i = 0; i < moveDistance - 1; i++)
        {
            movementPattern.Add(Vector2Int.up);
        }

        // 向下移动 moveDistance - 1 格
        for (int i = 0; i < moveDistance - 1; i++)
        {
            movementPattern.Add(Vector2Int.down);
        }

        Debug.Log($"Generated vertical pattern with {movementPattern.Count} steps");
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

    public string GetPatternInfo()
    {
        if (movementPattern.Count == 0) return "No pattern";

        Vector2Int dir = movementPattern[patternStep % movementPattern.Count];
        string dirName = dir == Vector2Int.up ? "Up" :
                         dir == Vector2Int.down ? "Down" : "Unknown";
        return $"Vertical movement - Direction: {dirName}, Step: {patternStep}/{movementPattern.Count}, Distance: {moveDistance}";
    }

    public int MoveDistance
    {
        get { return moveDistance; }
    }
}
