using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    public BossDialogueManager dialogueManager;

    public enum BossPattern
    {
        None,
        Charge,
        AreaAttack,
        Teleport
        // 향후 패턴 추가 가능
    }

    public Transform player;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.25f;

    private BossPattern currentPattern = BossPattern.None;
    private float patternCooldown = 5f;
    private float lastPatternTime;
    public float chargeSpeed = 8f;
    public float chargeDuration = 2f;

    public float teleportRange = 15f;               // 텔레포트 거리 조건
    public GameObject teleportEffectPrefab;         // 순간이동 이펙트 프리팹
    private int teleportCount = 0;                  // 순간이동 실행 횟수
    private int maxTeleportCount = 5;               // 최대 횟수
    private bool isTeleporting = false;             // 텔레포트 실행 여부

    private float lastAttackTime;
    private Animator animator;
    private Rigidbody2D rb;
    private BossState currentState = BossState.Idle;

    private BossEnemy bossEnemy;

    public enum BossState
    {
        Idle = 0,
        Walk = 1,
        Attack = 2,
        Hit = 3,
        Death = 4,
        Rush = 5
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bossEnemy = GetComponent<BossEnemy>();
    }

    void Update()
    {
        if (bossEnemy == null || player == null || !bossEnemy.IsAlive()) return;
        if (currentState == BossState.Hit || currentState == BossState.Death) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 공격 중이거나 패턴 동작 중이면 일반 추적 X
        if (currentState == BossState.Attack || currentPattern != BossPattern.None) return;

        // 패턴 시도
        if (Time.time - lastPatternTime >= patternCooldown)
        {
            lastPatternTime = Time.time;
            int patternIndex = Random.Range(0, 2); // 현재 0: 돌진, 1: 기본공격
            if (patternIndex == 0)
            {
                StartCoroutine(ChargePattern());
                return;
            }
            // 다른 패턴도 여기에 추가 가능
        }

        if (distance <= attackRange)
        {
            ChangeState(BossState.Attack);
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                StartCoroutine(PerformAttack());
            }
        }
        else
        {
            // 텔레포트 조건: 일정 거리 이상 + 아직 텔레포트 중이 아님 + 텔레포트 횟수 제한 미도달
            if (!isTeleporting && distance > teleportRange && teleportCount < maxTeleportCount)
            {
                StartCoroutine(TeleportPattern());
            }
            else
            {
                Invoke(nameof(MoveTowardsPlayer), 0.3f);
            }
        }
    }


    void MoveTowardsPlayer()
    {
        ChangeState(BossState.Walk);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

        if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    void ChangeState(BossState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        if (animator != null)
            animator.SetInteger("State", (int)newState);
    }

    IEnumerator PerformAttack()
    {

        moveSpeed = 0;
        // 공격 애니메이션 실행
        ChangeState(BossState.Attack);

        // 애니메이션 길이만큼 대기 (예: 공격 준비 시간)
        yield return new WaitForSeconds(attackCooldown *0.5f);
        moveSpeed = 2f;

        // 애니메이션 후속 처리 시간 (나머지 시간 대기)
        yield return new WaitForSeconds(attackCooldown);

        // Idle 상태로 전환
        if (bossEnemy != null && bossEnemy.IsAlive())
        {
            ChangeState(BossState.Idle);
        }

    }

    IEnumerator ChargePattern()
    {
        // 대사 출력
        StartCoroutine(dialogueManager.ShowDialogueById("Rush"));

        currentPattern = BossPattern.Charge;
        ChangeState(BossState.Rush); // 돌진 애니메이션 트리거

        Vector2 chargeDir = (player.position - transform.position).normalized;
        float startTime = Time.time;

        // 돌진 전 준비 시간
        yield return new WaitForSeconds(1f);

        while (Time.time - startTime < chargeDuration)
        {
            // 키네마틱이므로 MovePosition 대신 직접 위치 변경
            transform.position += (Vector3)(chargeDir * chargeSpeed * Time.deltaTime);
            yield return null;
            Debug.Log("돌진패턴실행");
        }

        // 돌진 종료 후 Idle 상태 복귀
        ChangeState(BossState.Idle);
        currentPattern = BossPattern.None;
    }

    IEnumerator TeleportPattern()
    {
        currentPattern = BossPattern.Teleport;
        StartCoroutine(dialogueManager.ShowDialogueById("Teleport"));

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < teleportRange)
        {
            currentPattern = BossPattern.None;
            yield break; // 범위 안에 있으면 취소
        }

        // 텔레포트 이펙트 생성 (현재 위치)
        Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity, transform);

        // 실제 텔레포트
        transform.position = player.position;

        yield return new WaitForSeconds(1f);

        teleportCount++;
        Debug.Log("텔레포트 실행: " + teleportCount);

        if (teleportCount >= maxTeleportCount)
        {
            // '종말' 대사 출력
            yield return StartCoroutine(dialogueManager.ShowDialogueById("End"));

            // 게임오버 처리 (생존 여부 무시)
            StartCoroutine(GameManager.instance.WaitAndLoadGameOverScene(5f));
        }

        currentPattern = BossPattern.None;
    }

    public void DealDamage()
    {
        // 공격 타이밍에 플레이어가 아직 범위 안에 있는지 확인
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange)
        {
            // 플레이어가 범위 밖으로 나갔으면 데미지 없음
            return;
        }
        // 실제 데미지 적용
        if (player != null)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null && bossEnemy != null)
            {
                playerScript.TakeDamage(bossEnemy.attackDamage);
            }
        }

    }


    // 외부에서 피격 트리거 시 사용
    public void OnHit()
    {
        if (bossEnemy == null || !bossEnemy.IsAlive()) return;

        ChangeState(BossState.Hit);
        Invoke(nameof(BackToIdle), 0.5f);
    }

    public void Death_anim()
    {
        ChangeState(BossState.Death);
    }

    public void BackToIdle()
    {
        if (bossEnemy != null && bossEnemy.IsAlive())
        {
            ChangeState(BossState.Idle);
        }
    }

}
