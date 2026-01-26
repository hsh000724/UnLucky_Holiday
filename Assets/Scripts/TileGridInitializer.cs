using UnityEngine;

public class TileGridInitializer : MonoBehaviour
{
    public GameObject tilePrefab;         // 배치할 타일 프리팹
    public float tileSize = 20f;          // 타일 한 변의 길이
    public Transform player;              // 중심 기준이 될 플레이어

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("❗ 'Player' 태그가 지정된 오브젝트를 찾을 수 없습니다.");
                return;
            }
        }

        if (tilePrefab == null)
        {
            Debug.LogError("❗ tilePrefab이 설정되지 않았습니다.");
            return;
        }

        // 3x3 그리드 배치
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 spawnPos = player.position + new Vector3(x * tileSize, y * tileSize, 0);
                Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
            }
        }
    }
}
