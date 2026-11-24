using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossEnemy : MonoBehaviour
{
    [Header("보스 스탯")]
    public float maxHealth = 1000f;
    public float health;
    public float attackDamage = 20f;


    [Header("UI 및 컴포넌트")]
    public GameObject bossHealthBarUI;
    public Slider bossHealthSlider;

    public GameObject damageTextPrefab;
    public GameObject criticalDamageTextPrefab;

    private BossDialogueManager dialogueManager;

    public GameObject dialogueManagerObject;


    private bool isAlive = true;

    private Animator anim;
    private Rigidbody2D rigid;
    private BossAI bossAI;


    void Start()
    {
        dialogueManager =
            dialogueManagerObject
                .GetComponent<BossDialogueManager>();
    }


    void Awake()
    {
        anim =
            GetComponentInChildren<Animator>();

        rigid =
            GetComponent<Rigidbody2D>();

        bossAI =
            GetComponent<BossAI>();
    }


    void OnEnable()
    {
        health = maxHealth;
        isAlive = true;

        UpdateHealthUI();


        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(
                true
            );
        }
    }


    void Update()
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.value =
                health / maxHealth;
        }
    }


    // =========================================================
    // 보스 피격
    // =========================================================

    public void TakeDamage(
        float damage,
        bool isCritical = false)
    {
        if (!isAlive)
            return;


        // 실제로 깎일 수 있는 체력만 계산
        float actualDamage =
            Mathf.Min(
                damage,
                Mathf.Max(
                    health,
                    0f
                )
            );


        // 데미지 적용
        health -= damage;


        health =
            Mathf.Clamp(
                health,
                0f,
                maxHealth
            );


        // =====================================================
        // ✅ 보스에게 준 피해 기록
        // =====================================================

        GameDataLogger.Instance?
            .LogBossDamage(
                actualDamage,
                isCritical
            );


        UpdateHealthUI();


        DrawDamage(
            (int)damage,
            isCritical
        );


        SoundManager.instance.PlaySFX(
            SoundManager.instance
                .Enemy_HurtClip
        );


        if (health > 0 &&
            bossAI != null)
        {
            // 필요 시 피격 상태
            // bossAI.OnHit();
        }


        if (health <= 0)
        {
            Die();
        }
    }


    // =========================================================
    // 체력 UI
    // =========================================================

    public void UpdateHealthUI()
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue =
                maxHealth;


            bossHealthSlider.value =
                health;
        }
    }


    // =========================================================
    // 데미지 텍스트
    // =========================================================

    private void DrawDamage(
        int damage,
        bool isCriticalHit)
    {
        Vector3 worldPosition =
            transform.position +
            new Vector3(
                0,
                1.5f,
                0
            );


        GameObject canvas =
            GameObject.Find(
                "Canvas"
            );


        if (canvas == null)
            return;


        GameObject prefab =
            isCriticalHit
                ? criticalDamageTextPrefab
                : damageTextPrefab;


        GameObject dmgText =
            Instantiate(
                prefab,
                canvas.transform
            );


        dmgText.transform.position =
            worldPosition;


        DamageText textComponent =
            dmgText.GetComponent<DamageText>();


        if (textComponent != null)
        {
            textComponent.SetDamage(
                damage
            );
        }
    }


    // =========================================================
    // 보스 사망
    // =========================================================

    private void Die()
    {
        // 중복 사망 방지
        if (!isAlive)
            return;


        isAlive = false;


        // =====================================================
        // ✅ 보스 처치 기록
        // GameManager.HandleGameClear()보다 먼저 실행해야 함
        // =====================================================

        GameDataLogger.Instance?
            .LogBossKill();


        Debug.Log(
            "[GameDataLogger] 보스 처치 기록"
        );


        // Invoke 취소
        CancelInvoke();


        if (bossAI != null)
        {
            bossAI.CancelInvoke();


            // 사망 애니메이션
            bossAI.Death_anim();
        }


        // 보스 사망 대사
        if (dialogueManager != null)
        {
            StartCoroutine(
                dialogueManager
                    .ShowDialogueById(
                        "Death"
                    )
            );
        }


        // 체력바 비활성화
        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(
                false
            );
        }


        StartCoroutine(
            HandleDeathSequence()
        );
    }


    // =========================================================
    // 보스 사망 후 게임 클리어
    // =========================================================

    private IEnumerator HandleDeathSequence()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance
                .HandleGameClear();
        }
        else
        {
            Debug.LogError(
                "GameManager 인스턴스를 찾을 수 없습니다. " +
                "게임 클리어 로직이 실행되지 않았습니다."
            );
        }


        StartCoroutine(
            DisableAfterDelay(
                3f
            )
        );


        yield return null;
    }


    IEnumerator DisableAfterDelay(
        float delay)
    {
        yield return
            new WaitForSeconds(
                delay
            );


        gameObject.SetActive(
            false
        );
    }


    // =========================================================
    // 공격 충돌
    // =========================================================

    private void OnTriggerEnter2D(
        Collider2D collision)
    {
        if (!isAlive)
            return;


        // =====================================================
        // 기본 탄환
        // =====================================================

        TestBullet bullet =
            collision.GetComponent<TestBullet>();


        if (bullet != null)
        {
            TakeDamage(
                bullet.damage,
                bullet.isCritical
            );


            // 폭발탄일 경우 폭발 생성
            if (TestBullet.Explosion_Bullet)
            {
                bullet.Explosion(
                    transform.position
                );
            }


            Destroy(
                collision.gameObject
            );


            return;
        }


        // =====================================================
        // 특수무기
        // =====================================================

        if (collision.CompareTag(
            "SpecialWeapons"))
        {
            Special_Weapon specialWeapon =
                collision.GetComponent
                    <Special_Weapon>();


            if (specialWeapon != null)
            {
                TakeDamage(
                    specialWeapon.damage,
                    false
                );
            }
        }
    }


    public bool IsAlive()
    {
        return isAlive;
    }
}