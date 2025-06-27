using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private int maxTurns = 20;
    
    [Header("Player Settings")]
    [SerializeField] private Vector2Int playerStartPosition = new Vector2Int(5, 4); // Center of 9x9 grid
    
    [Header("Exhibit Settings")]
    [SerializeField] private Vector2Int horizontalExhibitStartPosition = new Vector2Int(2, 2);
    [SerializeField] private Vector2Int circularExhibitStartPosition = new Vector2Int(6, 6);
    
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject horizontalExhibitPrefab;
    [SerializeField] private GameObject circularExhibitPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject candleHolderPrefab;
    
    [Header("Obstacle Settings")]
    [SerializeField] private Vector2Int[] obstaclePositions = new Vector2Int[0]; // Array of obstacle positions
    
    [Header("Candle Holder Settings")]
    [SerializeField] private Vector2Int candleHolderStartPosition = new Vector2Int(4, 4);
    
    // Game state
    private int currentTurn = 0;
    private bool gameEnded = false;
    private bool isProcessingTurn = false;
    
    // Grid and entities
    private GridManager gridManager;
    private PlayerController player;
    private List<ExhibitBase> exhibits = new List<ExhibitBase>();
    private List<CandleHolder> candleHolders = new List<CandleHolder>();
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
        CreateObstacles();
        CreateCandleHolders();
        
        // Store initial positions for win condition
        StoreInitialPositions();
        
        Debug.Log("Game initialized! Use WASD to move. Turn: " + currentTurn + "/" + maxTurns);
    }
    
    private void CreatePlayer()
    {
        // Place player at configured position
        Vector3 worldPos = gridManager.GridToWorldPosition(playerStartPosition);
        
        GameObject playerObj = Instantiate(playerPrefab, worldPos, Quaternion.identity);
        player = playerObj.GetComponent<PlayerController>();
        player.Initialize(playerStartPosition);
    }
    
    private void CreateExhibits()
    {
        // Create horizontal moving exhibit
        Vector3 worldPos1 = gridManager.GridToWorldPosition(horizontalExhibitStartPosition);
        GameObject horizontalObj = Instantiate(horizontalExhibitPrefab, worldPos1, Quaternion.identity);
        HorizontalExhibit horizontalExhibit = horizontalObj.GetComponent<HorizontalExhibit>();
        horizontalExhibit.Initialize(horizontalExhibitStartPosition);
        exhibits.Add(horizontalExhibit);
        
        // Create circular moving exhibit
        Vector3 worldPos2 = gridManager.GridToWorldPosition(circularExhibitStartPosition);
        GameObject circularObj = Instantiate(circularExhibitPrefab, worldPos2, Quaternion.identity);
        CircularExhibit circularExhibit = circularObj.GetComponent<CircularExhibit>();
        circularExhibit.Initialize(circularExhibitStartPosition);
        exhibits.Add(circularExhibit);
    }
    
    private void CreateObstacles()
    {
        if (obstaclePrefab == null) return;
        
        foreach (Vector2Int obstaclePos in obstaclePositions)
        {
            // Check if position is valid and not occupied by player or exhibits
            if (GridManager.Instance.IsValidPosition(obstaclePos) && 
                obstaclePos != playerStartPosition &&
                obstaclePos != horizontalExhibitStartPosition &&
                obstaclePos != circularExhibitStartPosition)
            {
                Vector3 worldPos = gridManager.GridToWorldPosition(obstaclePos);
                GameObject obstacleObj = Instantiate(obstaclePrefab, worldPos, Quaternion.identity);
                ObstacleBase obstacle = obstacleObj.GetComponent<ObstacleBase>();
                obstacle.Initialize(obstaclePos);
            }
            else
            {
                Debug.LogWarning($"Cannot place obstacle at {obstaclePos} - position invalid or occupied");
            }
        }
    }
    
    private void CreateCandleHolders()
    {
        if (candleHolderPrefab == null) return;
        
        // Check if position is valid and not occupied by player or exhibits
        if (GridManager.Instance.IsValidPosition(candleHolderStartPosition) && 
            candleHolderStartPosition != playerStartPosition &&
            candleHolderStartPosition != horizontalExhibitStartPosition &&
            candleHolderStartPosition != circularExhibitStartPosition)
        {
            Vector3 worldPos = gridManager.GridToWorldPosition(candleHolderStartPosition);
            GameObject candleHolderObj = Instantiate(candleHolderPrefab, worldPos, Quaternion.identity);
            CandleHolder candleHolder = candleHolderObj.GetComponent<CandleHolder>();
            candleHolder.Initialize(candleHolderStartPosition);
            candleHolders.Add(candleHolder);
        }
        else
        {
            Debug.LogWarning($"Cannot place candle holder at {candleHolderStartPosition} - position invalid or occupied");
        }
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
        
        // Check if there's an exhibit or candle holder at the target position
        ExhibitBase exhibitAtTarget = GetExhibitAtPosition(newPlayerPos);
        CandleHolder candleHolderAtTarget = GetCandleHolderAtPosition(newPlayerPos);
        
        if (exhibitAtTarget != null || candleHolderAtTarget != null)
        {
            // This is a push action - check if the object can be pushed
            Vector2Int objectNewPos = newPlayerPos + direction;
            
            // Check if object can be pushed to the new position (not blocked by wall or obstacle)
            if (!GridManager.Instance.IsWalkablePosition(objectNewPos))
            {
                Debug.Log("Cannot push object - would go out of bounds or hit obstacle!");
                return;
            }
            
            // Check if there's another exhibit or candle holder where we want to push
            if (GetExhibitAtPosition(objectNewPos) != null || GetCandleHolderAtPosition(objectNewPos) != null)
            {
                Debug.Log("Cannot push object - another object is in the way!");
                return;
            }
        }
        
        StartCoroutine(ProcessTurn(direction));
    }
    
    public bool IsExhibitAtPosition(Vector2Int position)
    {
        return GetExhibitAtPosition(position) != null;
    }
    
    public bool IsCandleHolderAtPosition(Vector2Int position)
    {
        return GetCandleHolderAtPosition(position) != null;
    }
    
    public bool IsPushableAtPosition(Vector2Int position)
    {
        return IsExhibitAtPosition(position) || IsCandleHolderAtPosition(position);
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
    
    private CandleHolder GetCandleHolderAtPosition(Vector2Int position)
    {
        foreach (CandleHolder candleHolder in candleHolders)
        {
            if (candleHolder.GridPosition == position)
            {
                return candleHolder;
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
        CandleHolder pushedCandleHolder = GetCandleHolderAtPosition(newPlayerPos);
        
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
                
                // Check if exhibit is in a light area (frozen)
                if (IsExhibitInLightArea(exhibits[i].GridPosition))
                {
                    // Exhibit is frozen by light - stays in place
                    intendedPos = exhibits[i].GridPosition;
                    canMove = false;
                    Debug.Log($"Exhibit at {exhibits[i].GridPosition} frozen by light");
                }
                // Check if exhibit wants to move to where player is going
                else if (intendedPos == newPlayerPos)
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
                // Check if exhibit wants to move to where pushed candle holder is going
                else if (pushedCandleHolder != null && intendedPos == pushedCandleHolder.GridPosition + playerDirection)
                {
                    // Exhibit blocked by pushed candle holder - stays in place
                    intendedPos = exhibits[i].GridPosition;
                    canMove = false;
                    Debug.Log($"Exhibit at {exhibits[i].GridPosition} blocked by pushed candle holder moving to {pushedCandleHolder.GridPosition + playerDirection}");
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
                // This simulates hitting a wall, obstacle, or being frozen by light
            }
        }
        
        // Handle candle holder pushing
        if (pushedCandleHolder != null)
        {
            Vector2Int candleHolderNewPos = pushedCandleHolder.GridPosition + playerDirection;
            pushedCandleHolder.SetPosition(candleHolderNewPos);
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
    
    // Check if an exhibit is in the light area of any candle holder
    private bool IsExhibitInLightArea(Vector2Int exhibitPosition)
    {
        foreach (CandleHolder candleHolder in candleHolders)
        {
            if (candleHolder.IsPositionInLightArea(exhibitPosition))
            {
                return true;
            }
        }
        return false;
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
        candleHolders.Clear();
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
    
    // Public getters for other scripts to access
    public int GridWidth
    {
        get { return gridManager.GridWidth; }
    }
    
    public int GridHeight
    {
        get { return gridManager.GridHeight; }
    }
    
    public float TileSize
    {
        get { return gridManager.TileSize; }
    }
    
    public int MaxTurns
    {
        get { return maxTurns; }
    }
    
    public int CurrentTurn
    {
        get { return currentTurn; }
    }
}