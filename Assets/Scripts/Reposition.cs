using UnityEngine;

public class Reposition : MonoBehaviour
{
    public float tileSize = 20f; // 타일 한 변의 길이 (타일의 크기에 맞게 설정)
    private Transform player;

    private void Start()
    {
        player = GameManager.instance?.player?.transform;
        if (player == null)
        {
            Debug.LogError("❗플레이어를 찾을 수 없습니다. GameManager.instance.player를 확인하세요.");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Vector3 diff = player.position - transform.position;
        Vector3 moveDir = Vector3.zero;

        if (Mathf.Abs(diff.x) > tileSize * 0.5f)
        {
            moveDir.x = tileSize * (diff.x > 0 ? 1 : -1);
        }

        if (Mathf.Abs(diff.y) > tileSize * 0.5f)
        {
            moveDir.y = tileSize * (diff.y > 0 ? 1 : -1);
        }

        // 이동 방향이 있을 때만 타일을 이동시킴
        if (moveDir != Vector3.zero)
        {
            transform.position += moveDir * 3f; // 3타일 간격으로 이동시켜야 3x3 유지
        }
    }
}
