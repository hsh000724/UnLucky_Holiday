using UnityEngine;

public class Player_Move : MonoBehaviour
{
    public float moveSpeed = 5f; // 플레이어 이동 속도

    private Rigidbody2D rb; // Rigidbody2D 컴포넌트에 접근하기 위한 변수
    private Animator animator; // 애니메이터 컴포넌트에 접근하기 위한 변수
    private Vector2 moveDirection; // 플레이어 이동 방향

    void Start()
    {
        // Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        
        // Animator 컴포넌트 가져오기
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 플레이어 이동 방향 설정
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        moveDirection.y = Input.GetAxisRaw("Vertical");
        
        // 이동 애니메이션 재생 여부 설정
        animator.SetBool("Run", moveDirection.magnitude > 0);
        
        // 왼쪽으로 이동할 때 플레이어 스케일 반전
        if (moveDirection.x < 0)
        {
            transform.localScale = new Vector3(-4, 4, 1);
        }
        // 오른쪽으로 이동할 때 플레이어 스케일 원래대로 설정
        else if (moveDirection.x > 0)
        {
            transform.localScale = new Vector3(4, 4, 1);
        }
    }

    void FixedUpdate()
    {
        // 플레이어 이동하기
        rb.linearVelocity = moveDirection.normalized * moveSpeed;
    }
}
