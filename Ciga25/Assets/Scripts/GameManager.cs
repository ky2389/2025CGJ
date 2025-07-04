using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
   
    [System.Serializable]
    public class ExhibitSpawnData
    {
        public GameObject prefab;
        public Vector2Int spawnPosition;
        public Vector2Int targetPosition;
        
        // Property that defaults target position to spawn position
       // public Vector2Int targetPosition
        //{
            //get
            //{
                // If target position is zero (unset), return spawn position
                //if (_targetPosition == Vector2Int.zero)
                //{
                    // spawnPosition;
                //}
                //return _targetPosition;
            //}
            //set
            //{
                //_targetPosition = value;
            //}
//        }
    }

    [System.Serializable]
    public class ExhibitTargetPair
    {
        public ExhibitBase exhibit;
        public Vector2Int targetPosition;
    }
    
    [System.Serializable]
    public class CandleHolderSpawnData
    {
        public Vector2Int spawnPosition;
        [Header("Light Settings")]
        public bool isLit = true;
        public int lightRadius = 1;
    }
    
    // Collision detection result
    [System.Serializable]
    public struct CollisionResult
    {
        public bool hasCollision;
        public string collisionMessage;
        
        public CollisionResult(bool hasCollision, string message)
        {
            this.hasCollision = hasCollision;
            this.collisionMessage = message;
        }
    }
    
    [Header("Game Settings")]
    [SerializeField] private int maxTurns = 20;
    [SerializeField] private int maxRelightUses = 3; // Maximum number of relight uses allowed
    [SerializeField] private string nextSceneName;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private RectTransform gameOverPanelTime;
    [SerializeField] private RectTransform gameOverPanelCrush;
    
    [Header("Player Settings")]
    [SerializeField] private Vector2Int playerStartPosition = new Vector2Int(5, 4); // Center of 9x9 grid
    
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
   
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject candleHolderPrefab;
    
    [Header("Obstacle Settings")]
    [SerializeField] private Vector2Int[] obstaclePositions = new Vector2Int[0]; // Array of obstacle positions
    
    [Header("Candle Holder Settings")]
    [SerializeField] private List<CandleHolderSpawnData> candleHolderSpawnList;
    
    [Header("Exhibit Target Settings")]
    [SerializeField] private List<ExhibitSpawnData> exhibitSpawnList;
    private List<ExhibitTargetPair> exhibitTargetPairs = new List<ExhibitTargetPair>();
    [SerializeField] private GameObject exhibitTargetMarkerPrefab;
    
    [Header("UI Settings")]
    [SerializeField] private UnityEngine.UI.Button relightButton; // Button to activate relight mode
    [SerializeField] private UnityEngine.UI.Button restartButton; // Button to restart the current level
    [SerializeField] private UnityEngine.UI.Button restartButtonTime; // Restart button on time game over panel
    [SerializeField] private UnityEngine.UI.Button restartButtonCrush; // Restart button on crush game over panel
    [SerializeField] private Camera gameCamera; // Reference to the game camera for mouse input
    [SerializeField] private TMPro.TextMeshProUGUI relightUsesText; // Text to display remaining relight uses
    [SerializeField] private TMPro.TextMeshProUGUI turnText; // Text to display current turn and max turns
    
    // Game state
    private int currentTurn = 0;
    private bool gameEnded = false;
    private bool isProcessingTurn = false;
    private bool isRelightModeActive = false; // Whether relight mode is active
    private bool isArrowKeyPressed = false; // Whether R key is pressed for showing arrows
    
    // Relight system
    private int relightUsesRemaining; // Number of relight uses remaining
    
    // Grid and entities
    private GridManager gridManager;
    private PlayerController player;
    private List<ExhibitBase> exhibits = new List<ExhibitBase>();
    private List<CandleHolder> candleHolders = new List<CandleHolder>();
    private Dictionary<Vector2Int, Vector2Int> exhibitStartPositions = new Dictionary<Vector2Int, Vector2Int>();
    
    // Public property for accessing player
    public PlayerController Player
    {
        get { return player; }
    }
    
    // Public property for accessing relight uses
    public int RelightUsesRemaining
    {
        get { return relightUsesRemaining; }
    }
    
    
    private void Start()
    {
        // Set the static reference for this scene
        Instance = this;
        
        InitializeGame();
        SetupRelightButton();
        SetupRestartButton();
        SetupGameOverRestartButtons();
    }
    
    private void OnDestroy()
    {
        // Clear the static reference when this GameManager is destroyed
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    private void Update()
    {
        if (gameEnded || isProcessingTurn) return;
        
        // Handle player movement input
        player.HandleInput();
        
        // Handle relight input
        HandleRelightInput();
        
        // Handle arrow visibility input
        HandleArrowVisibilityInput();
    }
    
    private void InitializeGame()
    {
        // Get grid manager
        gridManager = GridManager.Instance;
        
        // Initialize relight uses
        relightUsesRemaining = maxRelightUses;
        
        // Create entities
        CreatePlayer();
        CreateExhibits();
        CreateObstacles();
        CreateCandleHolders();
        
        // Store initial positions for win condition
        StoreInitialPositions();
        CreateExhibitTargets();
        
        // Initialize arrows as hidden
        UpdateExhibitMovementArrows();
        
        // Update UI
        UpdateTurnDisplay();
        UpdateRelightUsesUI();
        
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
        foreach (var data in exhibitSpawnList)
        {
            if (data.prefab == null)
            {
                Debug.LogWarning("Exhibit prefab is missing in spawn data");
                continue;
            }

            Vector3 worldPos = gridManager.GridToWorldPosition(data.spawnPosition);
            GameObject obj = Instantiate(data.prefab, worldPos, Quaternion.identity);
            ExhibitBase exhibit = obj.GetComponent<ExhibitBase>();
            exhibit.Initialize(data.spawnPosition, data.targetPosition);
            exhibits.Add(exhibit);

            exhibitTargetPairs.Add(new ExhibitTargetPair
            {
                exhibit = exhibit,
                targetPosition = data.targetPosition
            });

            Debug.Log($"[CreateExhibits] Spawned exhibit at {data.spawnPosition} with target at {data.targetPosition} using {data.prefab.name}");
        }
    }
    
    
    
    private void CreateExhibitTargets()
    {
        if (exhibitTargetMarkerPrefab == null) return;

        foreach (var data in exhibitSpawnList)
        {
            Vector3 worldPos = gridManager.GridToWorldPosition(data.targetPosition);
            Instantiate(exhibitTargetMarkerPrefab, worldPos, Quaternion.identity);
        }
    }
    
    private bool IsOccupiedByExhibit(Vector2Int pos)
    {
        foreach (var exhibit in exhibits)
        {
            if (exhibit.GridPosition == pos)
                return true;
        }
        return false;
    }
    
    private void CreateObstacles()
    {
        if (obstaclePrefab == null) return;
        
        foreach (Vector2Int obstaclePos in obstaclePositions)
        {
            // Check if position is valid and not occupied by player or exhibits
            if (GridManager.Instance.IsValidPosition(obstaclePos) &&
                obstaclePos != playerStartPosition &&
                !IsOccupiedByExhibit(obstaclePos))
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
        
        // Use spawn list if available, otherwise don't create any candle holders
        if (candleHolderSpawnList != null && candleHolderSpawnList.Count > 0)
        {
            CreateCandleHoldersFromSpawnList();
        }
        else
        {
            Debug.Log("No candle holders configured in spawn list - skipping candle holder creation");
        }
    }
    
    private void CreateCandleHoldersFromSpawnList()
    {
        foreach (var data in candleHolderSpawnList)
        {
            // Check if position is valid and not occupied
            if (IsValidCandleHolderPosition(data.spawnPosition))
            {
                Vector3 worldPos = gridManager.GridToWorldPosition(data.spawnPosition);
                GameObject candleHolderObj = Instantiate(candleHolderPrefab, worldPos, Quaternion.identity);
                CandleHolder candleHolder = candleHolderObj.GetComponent<CandleHolder>();
                
                // Initialize with custom settings
                candleHolder.Initialize(data.spawnPosition);
                candleHolder.SetLightState(data.isLit);
                candleHolder.SetLightRadius(data.lightRadius);
                
                candleHolders.Add(candleHolder);
                
                Debug.Log($"[CreateCandleHolders] Spawned candle holder at {data.spawnPosition} with light radius {data.lightRadius}");
            }
            else
            {
                Debug.LogWarning($"Cannot place candle holder at {data.spawnPosition} - position invalid or occupied");
            }
        }
    }
    
    private bool IsValidCandleHolderPosition(Vector2Int position)
    {
        return GridManager.Instance.IsValidPosition(position) && 
               position != playerStartPosition &&
               !IsOccupiedByExhibit(position) &&
               !IsOccupiedByCandleHolder(position);
    }
    
    private bool IsOccupiedByCandleHolder(Vector2Int pos)
    {
        foreach (var candleHolder in candleHolders)
        {
            if (candleHolder.GridPosition == pos)
                return true;
        }
        return false;
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

        // 不再直接检查推动动作，改为缓存动画状态在Player内部

        // 由Player缓存动画状态（已在PlayerController.HandleInput中做）

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
    
    // Check if a position is occupied by any entity (player, exhibit, or candle holder)
    public bool IsPositionOccupied(Vector2Int position)
    {
        // Check if player is at this position
        if (player != null && player.GridPosition == position)
            return true;
            
        // Check if any exhibit is at this position
        if (IsExhibitAtPosition(position))
            return true;
            
        // Check if any candle holder is at this position
        if (IsCandleHolderAtPosition(position))
            return true;
            
        return false;
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

        // 1. 在行动开始时让玩家应用缓存的动画状态，动画状态滞后于移动一步
        player.ApplyCachedAnimationState();

        Vector2Int newPlayerPos = player.GridPosition + playerDirection;

        // 获取推动的展品和烛台（如果有）
        ExhibitBase pushedExhibit = GetExhibitAtPosition(newPlayerPos);
        CandleHolder pushedCandleHolder = GetCandleHolderAtPosition(newPlayerPos);

        // 检查烛台推动是否有效
        if (pushedCandleHolder != null)
        {
            Vector2Int candleHolderNewPos = pushedCandleHolder.GridPosition + playerDirection;
            if (!GridManager.Instance.IsWalkablePosition(candleHolderNewPos))
            {
                // 烛台无法被推动到目标位置，阻止玩家移动
                Debug.Log($"Cannot push candle holder to {candleHolderNewPos} - position blocked");
                isProcessingTurn = false;
                yield break;
            }
        }

        // 2. 计算所有展品的新位置和移动许可状态
        List<Vector2Int> exhibitNewPositions = new List<Vector2Int>();
        List<bool> exhibitCanMove = new List<bool>();

        for (int i = 0; i < exhibits.Count; i++)
        {
            Vector2Int intendedPos;
            bool canMove = true;

            if (exhibits[i] == pushedExhibit)
            {
                intendedPos = exhibits[i].GridPosition + playerDirection;
            }
            else
            {
                intendedPos = exhibits[i].GetNextPosition();

                // 检查展品是否被烛光冻结
                if (IsExhibitInLightArea(exhibits[i].GridPosition))
                {
                    intendedPos = exhibits[i].GridPosition;
                    canMove = false;
                    Debug.Log($"Exhibit at {exhibits[i].GridPosition} frozen by light");
                    // Hide arrow for frozen exhibit
                    exhibits[i].HideMovementArrow();
                }
                // 如果想移动的位置被玩家占据
                else if (intendedPos == newPlayerPos)
                {
                    intendedPos = exhibits[i].GridPosition;
                    canMove = false;
                    Debug.Log($"Exhibit at {exhibits[i].GridPosition} blocked by player moving to {newPlayerPos}");
                }
                // 被推动展品阻挡
                else if (pushedExhibit != null && intendedPos == pushedExhibit.GridPosition + playerDirection)
                {
                    intendedPos = exhibits[i].GridPosition;
                    canMove = false;
                    Debug.Log($"Exhibit at {exhibits[i].GridPosition} blocked by pushed exhibit moving to {pushedExhibit.GridPosition + playerDirection}");
                }
                // 被推动烛台阻挡
                else if (pushedCandleHolder != null && intendedPos == pushedCandleHolder.GridPosition + playerDirection)
                {
                    intendedPos = exhibits[i].GridPosition;
                    canMove = false;
                    Debug.Log($"Exhibit at {exhibits[i].GridPosition} blocked by pushed candle holder moving to {pushedCandleHolder.GridPosition + playerDirection}");
                }
            }

            exhibitNewPositions.Add(intendedPos);
            exhibitCanMove.Add(canMove);
        }

        // 3. 检查展品间是否有碰撞 - only check exhibits that can actually move
        List<Vector2Int> actualMovingPositions = new List<Vector2Int>();
        for (int i = 0; i < exhibits.Count; i++)
        {
            if (exhibits[i] == pushedExhibit || exhibitCanMove[i])
            {
                // Only add positions of exhibits that will actually move
                actualMovingPositions.Add(exhibitNewPositions[i]);
            }
        }
        
        if (CheckExhibitCollisions(actualMovingPositions))
        {
            EndGame(false, "Exhibits collided!");
            yield break;
        }

        // 4. 玩家瞬移到新位置
        player.MoveToPosition(newPlayerPos);

        // 5. 展品移动
        for (int i = 0; i < exhibits.Count; i++)
        {
            if (exhibits[i] == pushedExhibit)
            {
                exhibits[i].SetPosition(exhibitNewPositions[i]);
            }
            else if (exhibitCanMove[i])
            {
                exhibits[i].MoveToNextPosition();
            }
            else
            {
                // 受阻展品保持原位，不更新位置
            }
        }

        // 5.5. 更新展品移动箭头（移动后）
        UpdateExhibitMovementArrows();

        // 6. 推动烛台移动（如果有）
        if (pushedCandleHolder != null)
        {
            Vector2Int candleHolderNewPos = pushedCandleHolder.GridPosition + playerDirection;
            pushedCandleHolder.SetPosition(candleHolderNewPos);
        }

        // 7. 检查所有实体碰撞（移动后的位置）
        CollisionResult collisionResult = CheckAllEntityCollisions();
        if (collisionResult.hasCollision)
        {
            EndGame(false, collisionResult.collisionMessage);
            yield break;
        }

        // 8. 不再立即结束推动动画，等待下一行动开始时更新动画状态

        // 9. 等待动画或效果时间（例如移动动画时长）
        yield return new WaitForSeconds(0.1f);

        // 10. 更新烛台火焰状态
        UpdateCandleHolderFlames();

        // 11. 检查游戏结束条件

        if (CheckAllTargetsReached())
        {
            EndGame(true, "All target exhibits reached their target positions!");
            yield break;
        }
        if (currentTurn == maxTurns)
        {
            {
                EndGame(false, "Time's up!");
            }
        }
        Debug.Log("Turn: " + currentTurn + "/" + maxTurns);

        // Update turn display UI
        UpdateTurnDisplay();

        isProcessingTurn = false;
    }

    
    private bool CheckAllTargetsReached()
    {
        foreach (var pair in exhibitTargetPairs)
        {
            if (pair.exhibit == null) continue;
            if (pair.exhibit.GridPosition != pair.targetPosition)
            {
                return false; // 还有未达成目标的
            }
        }
        return true; // 所有目标都完成
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
    
    //private bool CheckCollisions(Vector2Int playerPos, List<Vector2Int> exhibitPositions)
    //{
        // Check exhibit-to-exhibit collisions
        //return CheckExhibitCollisions(exhibitPositions);
    //}
    
    // private bool CheckWinCondition()
    // {
    //     foreach (ExhibitBase exhibit in exhibits)
    //     {
    //         if (!exhibitStartPositions.ContainsKey(exhibit.GridPosition))
    //         {
    //             return false;
    //         }
    //     }
    //     return true;
    // }
    
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
    
    private CollisionResult CheckAllEntityCollisions()
    {
        // Check exhibit-to-exhibit collisions (Game Over)
        for (int i = 0; i < exhibits.Count; i++)
        {
            for (int j = i + 1; j < exhibits.Count; j++)
            {
                if (exhibits[i].GridPosition == exhibits[j].GridPosition)
                {
                    Debug.Log($"Exhibit collision detected at position {exhibits[i].GridPosition}");
                    return new CollisionResult(true, "Exhibits collided!");
                }
            }
        }
        
        // Check candle holder collisions with exhibits (Game Over)
        foreach (CandleHolder candleHolder in candleHolders)
        {
            foreach (ExhibitBase exhibit in exhibits)
            {
                if (candleHolder.GridPosition == exhibit.GridPosition)
                {
                    Debug.Log($"Candle holder collided with exhibit at position {candleHolder.GridPosition}");
                    return new CollisionResult(true, "Candle holder collided with exhibit!");
                }
            }
        }
        
        // Check player collisions with exhibits (should not happen after movement)
        foreach (ExhibitBase exhibit in exhibits)
        {
            if (player.GridPosition == exhibit.GridPosition)
            {
                Debug.Log($"Player collided with exhibit at position {player.GridPosition}");
                return new CollisionResult(true, "Player collided with exhibit!");
            }
        }
        
        // Check player collisions with candle holders (should not happen after movement)
        foreach (CandleHolder candleHolder in candleHolders)
        {
            if (player.GridPosition == candleHolder.GridPosition)
            {
                Debug.Log($"Player collided with candle holder at position {player.GridPosition}");
                return new CollisionResult(true, "Player collided with candle holder!");
            }
        }
        
        return new CollisionResult(false, "");
    }
    
    private void EndGame(bool won, string message)
    {
        gameEnded = true;
        Debug.Log(won ? "Victory! " + message : "Game Over! " + message);
        
        // You can add UI feedback here
        if (won)
        {
            Debug.Log("Congratulations! You restored the museum!");
            sceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("The museum remains in chaos...");
            
            // Show appropriate game over panel based on failure type
            if (message.Contains("Time's up"))
            {
                // Time-based failure
                gameOverPanelTime.gameObject.SetActive(true);
                gameOverPanelCrush.gameObject.SetActive(false);
            }
            else
            {
                // Collision-based failure (exhibits collided, etc.)
                gameOverPanelCrush.gameObject.SetActive(true);
                gameOverPanelTime.gameObject.SetActive(false);
            }
        }
    }
    
    public bool IsGameEnded()
    {
        return gameEnded;
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

    // Update all candle holder flames at the end of each turn
    private void UpdateCandleHolderFlames()
    {
        foreach (CandleHolder candleHolder in candleHolders)
        {
            candleHolder.OnTurnEnd();
        }
    }
    
    // Update all exhibit movement arrows at the end of each turn
    private void UpdateExhibitMovementArrows()
    {
        foreach (ExhibitBase exhibit in exhibits)
        {
            if (isArrowKeyPressed)
            {
                exhibit.UpdateMovementArrow();
            }
            else
            {
                exhibit.HideMovementArrow();
            }
        }
    }

    private void SetupRelightButton()
    {
        if (relightButton != null)
        {
            relightButton.onClick.AddListener(ToggleRelightMode);
        }
        
        // Initialize the relight uses UI
        UpdateRelightUsesUI();
    }
    
    private void ToggleRelightMode()
    {
        // Check if we have any relight uses remaining
        if (relightUsesRemaining <= 0)
        {
            Debug.Log("No relight uses remaining!");
            return;
        }
        
        isRelightModeActive = !isRelightModeActive;
        
        if (isRelightModeActive)
        {
            Debug.Log($"Relight mode activated. Click on a candle holder to relight it. Uses remaining: {relightUsesRemaining}");
            // You can add visual feedback here (change button color, show cursor, etc.)
        }
        else
        {
            Debug.Log("Relight mode deactivated.");
            // You can add visual feedback here (restore button color, hide cursor, etc.)
        }
    }
    
    private void HandleRelightInput()
    {
        if (!isRelightModeActive || gameEnded || isProcessingTurn) return;
        
        if (Input.GetMouseButtonDown(0)) // Left mouse click
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = gameCamera.ScreenPointToRay(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            
            if (hit.collider != null)
            {
                // Check if we clicked on a candle holder
                CandleHolder clickedCandleHolder = hit.collider.GetComponent<CandleHolder>();
                if (clickedCandleHolder != null)
                {
                    // Check if the candle holder is actually unlit (needs relighting)
                    if (!clickedCandleHolder.IsLit)
                    {
                        // Relight the candle holder
                        clickedCandleHolder.RelightFlame();
                        
                        // Decrease relight uses only on successful relight
                        relightUsesRemaining--;
                        UpdateRelightUsesUI();
                        
                        // Deactivate relight mode
                        isRelightModeActive = false;
                        Debug.Log($"Candle holder relit! Relight mode deactivated. Uses remaining: {relightUsesRemaining}");
                    }
                    else
                    {
                        // Candle holder is already lit, don't count as a use
                        isRelightModeActive = false;
                        Debug.Log("Candle holder is already lit. Relight mode deactivated.");
                    }
                }
                else
                {
                    isRelightModeActive = false;
                    Debug.Log("Relight mode cancelled.");
                }
            }
            else
            {
                isRelightModeActive = false;
                Debug.Log("Relight mode cancelled.");
            }
        }
        
    }

    // Handle arrow visibility input (R key)
    private void HandleArrowVisibilityInput()
    {
        bool rKeyPressed = Input.GetKey(KeyCode.R);
        
        // Only update if the state changed
        if (rKeyPressed != isArrowKeyPressed)
        {
            isArrowKeyPressed = rKeyPressed;
            UpdateExhibitMovementArrows();
        }
    }

    private void UpdateTurnDisplay()
    {
        if (turnText != null)
        {
            turnText.text = $"Turn: {currentTurn}/{maxTurns}";
        }
    }

    private void SetupRestartButton()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }
    }

    private void RestartLevel()
    {
        Debug.Log("Restarting current level...");
        
        // Hide game over panels
        if (gameOverPanelTime != null)
        {
            gameOverPanelTime.gameObject.SetActive(false);
        }
        if (gameOverPanelCrush != null)
        {
            gameOverPanelCrush.gameObject.SetActive(false);
        }
        
        // Stop any ongoing coroutines
        StopAllCoroutines();
        
        // Reset game state
        currentTurn = 0;
        gameEnded = false;
        isProcessingTurn = false;
        isRelightModeActive = false;
        isArrowKeyPressed = false;
        
        // Reset relight uses
        relightUsesRemaining = maxRelightUses;
        
        // Clear all existing entities
        ClearAllEntities();
        
        // Reinitialize the game
        InitializeGame();
        
        // Update UI
        UpdateRelightUsesUI();
        
        Debug.Log("Level restarted! Turn: " + currentTurn + "/" + maxTurns);
    }

    private void ClearAllEntities()
    {
        // Clear player
        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }
        
        // Clear exhibits
        foreach (ExhibitBase exhibit in exhibits)
        {
            if (exhibit != null)
            {
                Destroy(exhibit.gameObject);
            }
        }
        exhibits.Clear();
        exhibitTargetPairs.Clear();
        
        // Clear candle holders
        foreach (CandleHolder candleHolder in candleHolders)
        {
            if (candleHolder != null)
            {
                Destroy(candleHolder.gameObject);
            }
        }
        candleHolders.Clear();
        
        // Clear exhibit start positions
        exhibitStartPositions.Clear();
    }

    private void UpdateRelightUsesUI()
    {
        if (relightUsesText != null)
        {
            relightUsesText.text = $"{relightUsesRemaining}/{maxRelightUses}";
        }
        
        // Update button interactability based on remaining uses
        if (relightButton != null)
        {
            relightButton.interactable = relightUsesRemaining > 0;
        }
    }

    private void SetupGameOverRestartButtons()
    {
        if (restartButtonTime != null)
        {
            restartButtonTime.onClick.AddListener(RestartLevel);
        }
        if (restartButtonCrush != null)
        {
            restartButtonCrush.onClick.AddListener(RestartLevel);
        }
    }
}