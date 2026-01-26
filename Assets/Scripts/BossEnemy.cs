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
    public GameObject bossHealthBarUI; // 보스 체력바 UI 오브젝트
    public Slider bossHealthSlider;    // 보스 체력 슬라이더
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
        dialogueManager = dialogueManagerObject.GetComponent<BossDialogueManager>();
    }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        bossAI = GetComponent<BossAI>();
    }

    void OnEnable()
    {
        health = maxHealth;
        isAlive = true;
        UpdateHealthUI();

        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(true);
        }
    }

    void Update()
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = health / maxHealth;
        }
    }

    public void TakeDamage(float damage, bool isCritical = false)
    {
        if (!isAlive) return;

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        UpdateHealthUI();
        DrawDamage((int)damage, isCritical);
        SoundManager.instance.PlaySFX(SoundManager.instance.Enemy_HurtClip);

        if (health > 0 && bossAI != null)
        {
            // bossAI.OnHit();
        }

        if (health <= 0)
        {
            Die();
        }
    }

    public void UpdateHealthUI()
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHealth;
            bossHealthSlider.value = health;
        }
    }

    private void DrawDamage(int damage, bool isCriticalHit)
    {
        Vector3 worldPosition = transform.position + new Vector3(0, 1.5f, 0); // 머리 위
        GameObject canvas = GameObject.Find("Canvas");

        if (canvas == null) return;

        GameObject prefab = isCriticalHit ? criticalDamageTextPrefab : damageTextPrefab;
        GameObject dmgText = Instantiate(prefab, canvas.transform);
        dmgText.transform.position = worldPosition;

        var textComponent = dmgText.GetComponent<DamageText>();
        if (textComponent != null)
        {
            textComponent.SetDamage(damage);
        }
    }

    private void Die()
    {
        isAlive = false;

        // 모든 Invoke된 메서드 취소
        CancelInvoke(); // BossEnemy.cs에서 호출 시 자기 자신에만 적용됨
        bossAI.CancelInvoke(); // BossAI에서 예약된 MoveTowardsPlayer를 취소

        // 애니메이션 처리
        bossAI.Death_anim();

        // 보스 사망 대사
        StartCoroutine(dialogueManager.ShowDialogueById("Death"));

        // 보스 체력바 비활성화
        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(false);

        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {
        // ✅ [수정] 게임 클리어 처리를 GameManager에 위임하고 바로 호출
        if (GameManager.instance != null)
        {
            // GameManager에 클리어 상태를 알리고 스코어 저장 및 씬 로드를 위임
            GameManager.instance.HandleGameClear();
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다. 게임 클리어 로직이 실행되지 않았습니다.");
        }
        // -----------------------------------------------------

        // 실제 제거 처리 (비활성화 등)
        StartCoroutine(DisableAfterDelay(3f)); // 만약 바로 비활성화 하고 싶으면 0초

        // 씬 로드는 GameManager에서 담당하므로 여기서는 대기만 수행
        yield return null;
    }

    IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();

        if (!isAlive) return;

        // Bullet 데미지 처리
        TestBullet bullet = collision.GetComponent<TestBullet>();
        if (bullet != null)
        {
            TakeDamage(bullet.damage, bullet.isCritical);

            if (TestBullet.Explosion_Bullet)
            {
                bullet.Explosion(transform.position);
            }

            Destroy(collision.gameObject);
        }

        // 특수무기
        if (collision.CompareTag("SpecialWeapons"))
        {
            float damage = collision.GetComponent<Special_Weapon>().damage;
            TakeDamage(damage);
        }
    }

    public bool IsAlive()
    {
        return isAlive;  // 혹은 내부에서 사용하는 생존 여부 변수
    }
}