using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int gridWidth = 9;
    public int gridHeight = 9;
    public float tileSize = 32f;
    public int maxTurns = 20;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject horizontalExhibitPrefab;
    public GameObject circularExhibitPrefab;
    
    // Game state
    private int currentTurn = 0;
    private bool gameEnded = false;
    private bool isProcessingTurn = false;
    
    // Grid and entities
    private GridManager gridManager;
    private PlayerController player;
    private List<ExhibitBase> exhibits = new List<ExhibitBase>();
    private Dictionary<Vector2Int, Vector2Int> exhibitStartPositions = new Dictionary<Vector2Int, Vector2Int>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // Get grid manager
        gridManager = GridManager.Instance;
        
        // Create entities
        CreatePlayer();
        CreateExhibits();
        
        // Store initial positions for win condition
        StoreInitialPositions();
        
        Debug.Log("Game initialized! Use WASD to move. Turn: " + currentTurn + "/" + maxTurns);
    }
    
    private void CreatePlayer()
    {
        // Place player at center of grid
        Vector2Int playerPos = new Vector2Int(gridWidth / 2+1, gridHeight / 2);
        Vector3 worldPos = gridManager.GridToWorldPosition(playerPos);
        
        GameObject playerObj = Instantiate(playerPrefab, worldPos, Quaternion.identity);
        player = playerObj.GetComponent<PlayerController>();
        player.Initialize(playerPos);
    }
    
    private void CreateExhibits()
    {
        // Create horizontal moving exhibit
        Vector2Int horizontalPos = new Vector2Int(2, 2);
        Vector3 worldPos1 = gridManager.GridToWorldPosition(horizontalPos);
        GameObject horizontalObj = Instantiate(horizontalExhibitPrefab, worldPos1, Quaternion.identity);
        HorizontalExhibit horizontalExhibit = horizontalObj.GetComponent<HorizontalExhibit>();
        horizontalExhibit.Initialize(horizontalPos);
        exhibits.Add(horizontalExhibit);
        
        // Create circular moving exhibit
        Vector2Int circularPos = new Vector2Int(6, 6);
        Vector3 worldPos2 = gridManager.GridToWorldPosition(circularPos);
        GameObject circularObj = Instantiate(circularExhibitPrefab, worldPos2, Quaternion.identity);
        CircularExhibit circularExhibit = circularObj.GetComponent<CircularExhibit>();
        circularExhibit.Initialize(circularPos);
        exhibits.Add(circularExhibit);
    }
    
    private void StoreInitialPositions()
    {
        // Store exhibit starting positions for win condition
        foreach (ExhibitBase exhibit in exhibits)
        {
            exhibitStartPositions[exhibit.GridPosition] = exhibit.GridPosition;
        }
    }
    
    public void ProcessPlayerInput(Vector2Int direction)
    {
        if (gameEnded || isProcessingTurn) return;
        
        Vector2Int newPlayerPos = player.GridPosition + direction;
        
        // Check if there's an exhibit at the target position
        ExhibitBase exhibitAtTarget = GetExhibitAtPosition(newPlayerPos);
        
        if (exhibitAtTarget != null)
        {
            // This is a push action - check if the exhibit can be pushed
            Vector2Int exhibitNewPos = newPlayerPos + direction;
            
            // Check if exhibit can be pushed to the new position
            if (!GridManager.Instance.IsValidPosition(exhibitNewPos))
            {
                Debug.Log("Cannot push exhibit - would go out of bounds!");
                return;
            }
            
            // // Check if there's another exhibit where we want to push
            // if (GetExhibitAtPosition(exhibitNewPos) != null)
            // {
            //     Debug.Log("Cannot push exhibit - another exhibit is in the way!");
            //     return;
            // }
        }
        
        StartCoroutine(ProcessTurn(direction));
    }
    
    public bool IsExhibitAtPosition(Vector2Int position)
    {
        return GetExhibitAtPosition(position) != null;
    }
    
    private ExhibitBase GetExhibitAtPosition(Vector2Int position)
    {
        foreach (ExhibitBase exhibit in exhibits)
        {
            if (exhibit.GridPosition == position)
            {
                return exhibit;
            }
        }
        return null;
    }
    
    private IEnumerator ProcessTurn(Vector2Int playerDirection)
    {
        isProcessingTurn = true;
        currentTurn++;
        
        // Step 1: Calculate all movements
        Vector2Int newPlayerPos = player.GridPosition + playerDirection;
        ExhibitBase pushedExhibit = GetExhibitAtPosition(newPlayerPos);
        
        // Calculate exhibit movements and handle conflicts with player
        List<Vector2Int> exhibitNewPositions = new List<Vector2Int>();
        List<bool> exhibitCanMove = new List<bool>(); // Track which exhibits can move
        
        for (int i = 0; i < exhibits.Count; i++)
        {
            Vector2Int intendedPos;
            bool canMove = true;
            
            if (exhibits[i] == pushedExhibit)
            {
                // Pushed exhibit moves in player direction
                intendedPos = exhibits[i].GridPosition + playerDirection;
            }
            else
            {
                // Normal exhibit movement
                intendedPos = exhibits[i].GetNextPosition();
                
                // Check if exhibit wants to move to where player is going
                if (intendedPos == newPlayerPos)
                {
                    // Exhibit blocked by player - stays in place
                    intendedPos = exhibits[i].GridPosition;
                    canMove = false;
                    Debug.Log($"Exhibit at {exhibits[i].GridPosition} blocked by player moving to {newPlayerPos}");
                }
                // Check if exhibit wants to move to where pushed exhibit is going
                else if (pushedExhibit != null && intendedPos == pushedExhibit.GridPosition + playerDirection)
                {
                    // Exhibit blocked by pushed exhibit - stays in place
                    intendedPos = exhibits[i].GridPosition;
                    canMove = false;
                    Debug.Log($"Exhibit at {exhibits[i].GridPosition} blocked by pushed exhibit moving to {pushedExhibit.GridPosition + playerDirection}");
                }
            }
            
            exhibitNewPositions.Add(intendedPos);
            exhibitCanMove.Add(canMove);
        }
        
        // Step 2: Check for collisions between exhibits
        if (CheckExhibitCollisions(exhibitNewPositions))
        {
            EndGame(false, "Exhibits collided!");
            yield break;
        }
        
        // Step 3: Execute movements
        player.MoveToPosition(newPlayerPos);
        
        for (int i = 0; i < exhibits.Count; i++)
        {
            if (exhibits[i] == pushedExhibit)
            {
                exhibits[i].SetPosition(exhibitNewPositions[i]);
            }
            else if (exhibitCanMove[i])
            {
                // Normal movement - advance pattern
                exhibits[i].MoveToNextPosition();
            }
            else
            {
                // Blocked movement - don't advance pattern, stay in place
                // This simulates hitting a wall
            }
        }
        
        // Wait for movement animations (if any)
        yield return new WaitForSeconds(0.1f);
        
        // Step 4: Check win/lose conditions
        if (currentTurn == maxTurns)
        {
            // Check win condition
            if (CheckWinCondition())
            {
                EndGame(true, "All exhibits returned to original positions!");
            }
            else
            {
                EndGame(false, "Time's up!");
            }
        }
        
        Debug.Log("Turn: " + currentTurn + "/" + maxTurns);
        isProcessingTurn = false;
    }
    
    private bool CheckExhibitCollisions(List<Vector2Int> exhibitPositions)
    {
        // Check exhibit-to-exhibit collisions
        for (int i = 0; i < exhibitPositions.Count; i++)
        {
            for (int j = i + 1; j < exhibitPositions.Count; j++)
            {
                if (exhibitPositions[i] == exhibitPositions[j])
                {
                    return true; // Collision detected
                }
            }
        }
        
        return false;
    }
    
    private bool CheckCollisions(Vector2Int playerPos, List<Vector2Int> exhibitPositions)
    {
        // Check exhibit-to-exhibit collisions
        return CheckExhibitCollisions(exhibitPositions);
    }
    
    private bool CheckWinCondition()
    {
        foreach (ExhibitBase exhibit in exhibits)
        {
            if (!exhibitStartPositions.ContainsKey(exhibit.GridPosition))
            {
                return false;
            }
        }
        return true;
    }
    
    private void EndGame(bool won, string message)
    {
        gameEnded = true;
        Debug.Log(won ? "Victory! " + message : "Game Over! " + message);
        
        // You can add UI feedback here
        if (won)
        {
            Debug.Log("Congratulations! You restored the museum!");
        }
        else
        {
            Debug.Log("The museum remains in chaos...");
        }
    }
    
    public bool IsGameEnded()
    {
        return gameEnded;
    }
    
    public void RestartGame()
    {
        // Reset game state
        currentTurn = 0;
        gameEnded = false;
        isProcessingTurn = false;
        exhibits.Clear();
        exhibitStartPositions.Clear();
        
        // Destroy existing entities
        if (player != null)
            DestroyImmediate(player.gameObject);
        
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Reinitialize
        InitializeGame();
    }
}