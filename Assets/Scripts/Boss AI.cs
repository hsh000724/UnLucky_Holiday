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
    }


    public Transform player;

    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.25f;
    // 실제 보스전 시작 여부
    private bool bossBattleStarted = false;


    private BossPattern currentPattern =
        BossPattern.None;


    private float patternCooldown = 5f;
    private float lastPatternTime;


    public float chargeSpeed = 8f;
    public float chargeDuration = 2f;


    public float teleportRange = 15f;
    public GameObject teleportEffectPrefab;

    private int teleportCount = 0;
    private int maxTeleportCount = 5;
    private bool isTeleporting = false;


    private float lastAttackTime;

    private Animator animator;
    private Rigidbody2D rb;

    private BossState currentState =
        BossState.Idle;


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
        animator =
            GetComponentInChildren<Animator>();


        rb =
            GetComponent<Rigidbody2D>();


        bossEnemy =
            GetComponent<BossEnemy>();
    }


    void Update()
    {
        // 보스 소환 전에는 AI 실행 금지
        if (!bossBattleStarted)
            return;

        if (bossEnemy == null ||
            player == null ||
            !bossEnemy.IsAlive())
        {
            return;
        }

        if (bossEnemy == null ||
            player == null ||
            !bossEnemy.IsAlive())
        {
            return;
        }


        if (currentState == BossState.Hit ||
            currentState == BossState.Death)
        {
            return;
        }


        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );


        if (currentState ==
                BossState.Attack ||
            currentPattern !=
                BossPattern.None)
        {
            return;
        }


        // 패턴 시도
        if (Time.time -
            lastPatternTime >=
            patternCooldown)
        {
            lastPatternTime =
                Time.time;


            int patternIndex =
                Random.Range(
                    0,
                    2
                );


            if (patternIndex == 0)
            {
                StartCoroutine(
                    ChargePattern()
                );

                return;
            }
        }


        if (distance <= attackRange)
        {
            ChangeState(
                BossState.Attack
            );


            if (Time.time -
                lastAttackTime >=
                attackCooldown)
            {
                lastAttackTime =
                    Time.time;


                StartCoroutine(
                    PerformAttack()
                );
            }
        }
        else
        {
            if (!isTeleporting &&
                distance >
                    teleportRange &&
                teleportCount <
                    maxTeleportCount)
            {
                StartCoroutine(
                    TeleportPattern()
                );
            }
            else
            {
                Invoke(
                    nameof(
                        MoveTowardsPlayer
                    ),
                    0.3f
                );
            }
        }
    }


    // =========================================================
    // 이동
    // =========================================================

    void MoveTowardsPlayer()
    {
        ChangeState(
            BossState.Walk
        );


        Vector2 direction =
            (
                player.position -
                transform.position
            ).normalized;


        rb.MovePosition(
            rb.position +
            direction *
            moveSpeed *
            Time.deltaTime
        );


        if (direction.x < 0)
        {
            transform.localScale =
                new Vector3(
                    -1,
                    1,
                    1
                );
        }
        else
        {
            transform.localScale =
                new Vector3(
                    1,
                    1,
                    1
                );
        }
    }


    // =========================================================
    // 상태 변경
    // =========================================================

    void ChangeState(
        BossState newState)
    {
        if (currentState ==
            newState)
        {
            return;
        }


        currentState =
            newState;


        if (animator != null)
        {
            animator.SetInteger(
                "State",
                (int)newState
            );
        }
    }


    // =========================================================
    // 기본 공격
    // =========================================================

    IEnumerator PerformAttack()
    {
        moveSpeed = 0;


        ChangeState(
            BossState.Attack
        );


        yield return
            new WaitForSeconds(
                attackCooldown *
                0.5f
            );


        moveSpeed = 2f;


        yield return
            new WaitForSeconds(
                attackCooldown
            );


        if (bossEnemy != null &&
            bossEnemy.IsAlive())
        {
            ChangeState(
                BossState.Idle
            );
        }
    }


    // =========================================================
    // 돌진
    // =========================================================

    IEnumerator ChargePattern()
    {
        StartCoroutine(
            dialogueManager
                .ShowDialogueById(
                    "Rush"
                )
        );


        currentPattern =
            BossPattern.Charge;


        ChangeState(
            BossState.Rush
        );


        Vector2 chargeDir =
            (
                player.position -
                transform.position
            ).normalized;


        float startTime =
            Time.time;


        yield return
            new WaitForSeconds(
                1f
            );


        while (Time.time -
            startTime <
            chargeDuration)
        {
            transform.position +=
                (Vector3)(
                    chargeDir *
                    chargeSpeed *
                    Time.deltaTime
                );


            yield return null;


            Debug.Log(
                "돌진패턴실행"
            );
        }


        ChangeState(
            BossState.Idle
        );


        currentPattern =
            BossPattern.None;
    }


    // =========================================================
    // 텔레포트
    // =========================================================

    IEnumerator TeleportPattern()
    {
        currentPattern =
            BossPattern.Teleport;


        StartCoroutine(
            dialogueManager
                .ShowDialogueById(
                    "Teleport"
                )
        );


        float distanceToPlayer =
            Vector2.Distance(
                transform.position,
                player.position
            );


        if (distanceToPlayer <
            teleportRange)
        {
            currentPattern =
                BossPattern.None;


            yield break;
        }


        Instantiate(
            teleportEffectPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );


        transform.position =
            player.position;


        yield return
            new WaitForSeconds(
                1f
            );


        teleportCount++;


        Debug.Log(
            "텔레포트 실행: " +
            teleportCount
        );


        // =====================================================
        // 텔레포트 제한 초과
        // =====================================================

        if (teleportCount >=
            maxTeleportCount)
        {
            yield return
                StartCoroutine(
                    dialogueManager
                        .ShowDialogueById(
                            "End"
                        )
                );


            // ✅ 특수 게임오버 원인 기록
            StartCoroutine(
                GameManager.instance
                    .WaitAndLoadGameOverScene(
                        5f,
                        "BossTeleportLimit"
                    )
            );
        }


        currentPattern =
            BossPattern.None;
    }


    // =========================================================
    // 플레이어 공격
    // =========================================================

    public void DealDamage()
    {
        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );


        if (distance > attackRange)
        {
            return;
        }


        if (player != null)
        {
            Player playerScript =
                player.GetComponent<Player>();


            if (playerScript != null &&
                bossEnemy != null)
            {
                // Player.TakeDamage에서
                // bossDamageTaken 로그 기록
                playerScript.TakeDamage(
                    bossEnemy.attackDamage
                );
            }
        }
    }


    // =========================================================
    // 피격
    // =========================================================

    public void OnHit()
    {
        if (bossEnemy == null ||
            !bossEnemy.IsAlive())
        {
            return;
        }


        ChangeState(
            BossState.Hit
        );


        Invoke(
            nameof(
                BackToIdle
            ),
            0.5f
        );
    }


    public void Death_anim()
    {
        ChangeState(
            BossState.Death
        );
    }


    public void BackToIdle()
    {
        if (bossEnemy != null &&
            bossEnemy.IsAlive())
        {
            ChangeState(
                BossState.Idle
            );
        }
    }

    public void StartBossBattle()
    {
        teleportCount = 0;
        isTeleporting = false;
        currentPattern = BossPattern.None;

        bossBattleStarted = true;

        Debug.Log(
            "[BossAI] 보스전 시작"
        );
    }

    public void StopBossBattle()
    {
        bossBattleStarted = false;

        CancelInvoke();

        Debug.Log(
            "[BossAI] 보스전 종료"
        );
    }
}