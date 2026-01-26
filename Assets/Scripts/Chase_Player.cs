using UnityEngine;

public class Chase_Player : MonoBehaviour
{
    public float moveSpeed = 3f; // 몬스터 이동 속도

    private Transform player; // 플레이어의 Transform
    private Rigidbody2D rb; // 몬스터의 Rigidbody2D 컴포넌트

    void Start()
    {
        // 플레이어의 Transform 가져오기
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 플레이어를 향해 이동하기
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // 플레이어를 바라보도록 몬스터 회전하기
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
