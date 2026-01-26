using UnityEngine;

public class Playe_Attack : MonoBehaviour
{
    private Animator animator; // 애니메이터 컴포넌트에 접근하기 위한 변수

    void Start()
    {
        // Animator 컴포넌트 가져오기
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // "Space" 키를 누르면 공격 애니메이션 재생
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Attack");
        }
    }
}
