using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public Tilemap groundTilemap; // 指向你的地面 Tilemap
    public Tilemap obstacleTilemap; // （可选）指向障碍物图层

    private Vector3Int currentGridPos;

    void Start()
    {
        // 获取角色当前所在的格子位置（Tilemap 坐标）
        currentGridPos = groundTilemap.WorldToCell(transform.position);
        transform.position = groundTilemap.GetCellCenterWorld(currentGridPos);
    }

    void Update()
    {
        Vector3Int direction = Vector3Int.zero;

        if (Input.GetKeyDown(KeyCode.W)) direction = Vector3Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) direction = Vector3Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) direction = Vector3Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) direction = Vector3Int.right;

        if (direction != Vector3Int.zero)
        {
            TryMove(direction);
        }
    }

    void TryMove(Vector3Int dir)
    {
        Vector3Int targetPos = currentGridPos + dir;

        // 检查目标位置是不是地面 & 没有障碍
        bool isGround = groundTilemap.HasTile(targetPos);
        bool isBlocked = obstacleTilemap != null && obstacleTilemap.HasTile(targetPos);

        if (isGround && !isBlocked)
        {
            currentGridPos = targetPos;
            transform.position = groundTilemap.GetCellCenterWorld(currentGridPos);
        }
    }
}
