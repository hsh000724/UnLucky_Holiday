using UnityEngine;

public class InfiniteTilemapManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public int gridSize = 3;           // 홀수로 설정 (예: 3,5,7)
    public float tileSize = 20f;
    public Transform player;

    private GameObject[,] tiles;
    private Vector2Int currentCenter;

    void Start()
    {
        if (tilePrefab == null || player == null)
        {
            Debug.LogError("TilePrefab 또는 Player가 설정되지 않았습니다.");
            return;
        }

        tiles = new GameObject[gridSize, gridSize];
        int half = gridSize / 2;

        // 초기 타일 생성 및 배치
        for (int x = -half; x <= half; x++)
        {
            for (int y = -half; y <= half; y++)
            {
                Vector3 spawnPos = new Vector3(x * tileSize, y * tileSize, 0);
                GameObject tile = Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
                tiles[x + half, y + half] = tile;
            }
        }

        currentCenter = WorldToTileCoord(player.position);
    }

    void Update()
    {
        Vector2Int playerTile = WorldToTileCoord(player.position);

        if (playerTile != currentCenter)
        {
            Vector2Int delta = playerTile - currentCenter;
            ShiftTiles(delta);
            currentCenter = playerTile;
        }
    }

    Vector2Int WorldToTileCoord(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / tileSize);
        int y = Mathf.FloorToInt(pos.y / tileSize);
        return new Vector2Int(x, y);
    }

    void ShiftTiles(Vector2Int delta)
    {
        int half = gridSize / 2;
        GameObject[,] newTiles = new GameObject[gridSize, gridSize];
        Vector2Int newCenter = currentCenter + delta;

        // 1) 배열 인덱스 순환 이동 (모듈로 연산)
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int newX = (x - delta.x + gridSize) % gridSize;
                int newY = (y - delta.y + gridSize) % gridSize;

                newTiles[newX, newY] = tiles[x, y];
            }
        }

        // 2) 모든 타일 위치를 정확한 월드 좌표로 재배치
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int tileCoord = newCenter + new Vector2Int(x - half, y - half);
                Vector3 newPos = new Vector3(tileCoord.x * tileSize, tileCoord.y * tileSize, 10);

                newTiles[x, y].transform.position = newPos;
            }
        }

        tiles = newTiles;
    }
}
