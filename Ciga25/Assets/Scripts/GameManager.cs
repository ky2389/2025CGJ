using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
   
    [System.Serializable]
    public class ExhibitSpawnData
    {
        public GameObject prefab;
        public Vector2Int spawnPosition;
        public Vector2Int targetPosition;
    }

    [System.Serializable]
    public class ExhibitTargetPair
    {
        public ExhibitBase exhibit;
        public Vector2Int targetPosition;
    }
    
    [Header("Game Settings")]
    [SerializeField] private int maxTurns = 20;
    
    [Header("Player Settings")]
    [SerializeField] private Vector2Int playerStartPosition = new Vector2Int(5, 4); // Center of 9x9 grid
    
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
   
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject candleHolderPrefab;
    
    [Header("Obstacle Settings")]
    [SerializeField] private Vector2Int[] obstaclePositions = new Vector2Int[0]; // Array of obstacle positions
    
    [Header("Candle Holder Settings")]
    [SerializeField] private Vector2Int candleHolderStartPosition = new Vector2Int(4, 4);
    
    [Header("Exhibit Target Settings")]
    [SerializeField] private List<ExhibitSpawnData> exhibitSpawnList;
    private List<ExhibitTargetPair> exhibitTargetPairs = new List<ExhibitTargetPair>();
    [SerializeField] private GameObject exhibitTargetMarkerPrefab;
    
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
        CreateExhibitTargets();
        
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
            GameObject prefab = null;

            if (data.prefab == null)
            {
                Debug.LogWarning("Exhibit prefab is missing in spawn data");
                continue;
            }

            
            if (prefab != null)
            {
                Vector3 worldPos = gridManager.GridToWorldPosition(data.spawnPosition);
                GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity);
                ExhibitBase exhibit = obj.GetComponent<ExhibitBase>();
                exhibit.Initialize(data.spawnPosition);
                exhibits.Add(exhibit);

                // 保存 exhibit 到目标位置的映射（用于判定是否成功）
                exhibitTargetPairs.Add(new ExhibitTargetPair
                {
                    exhibit = exhibit,
                    targetPosition = data.targetPosition
                });
            }
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
        
        // Check if position is valid and not occupied by player or exhibits
        if (GridManager.Instance.IsValidPosition(candleHolderStartPosition) && 
            candleHolderStartPosition != playerStartPosition &&
            !IsOccupiedByExhibit(candleHolderStartPosition))
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

    // 3. 检查展品间是否有碰撞
    if (CheckExhibitCollisions(exhibitNewPositions))
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

    // 6. 推动烛台移动（如果有）
    if (pushedCandleHolder != null)
    {
        Vector2Int candleHolderNewPos = pushedCandleHolder.GridPosition + playerDirection;
        pushedCandleHolder.SetPosition(candleHolderNewPos);
    }

    // 7. 不再立即结束推动动画，等待下一行动开始时更新动画状态

    // 8. 等待动画或效果时间（例如移动动画时长）
    yield return new WaitForSeconds(0.1f);

    // 9. 检查游戏结束条件
    if (currentTurn == maxTurns)
    {
        if (CheckWinCondition())
        {
            EndGame(true, "All exhibits returned to original positions!");
        }
        else
        {
            EndGame(false, "Time's up!");
        }
    }

    if (CheckAllTargetsReached())
    {
        EndGame(true, "All target exhibits reached their target positions!");
        yield break;
    }

    Debug.Log("Turn: " + currentTurn + "/" + maxTurns);

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