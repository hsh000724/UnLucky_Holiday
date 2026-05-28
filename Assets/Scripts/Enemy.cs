using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private GameObject Critical_damageTextPrefab;

    private Slider healthSlider;
    public GameObject healthBarPrefab;
    private GameObject healthBarInstance;

    public float speed; // 몬스터의 속도
    public float health; // 몬스터의 현재체력
    public float maxHealth; // 몬스터의 최대체력
    public float attackDamage = 10f;  // 몬스터의 공격력 
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;
    Animator anim;
    private Vector3 deathPosition;

    public static float dropProbability = 0.05f;
    public static float dropProbability2 = 0.02f;
    public static float dropProbability3 = 0.006f;
    public static float dropProbability4 = 0.0025f;
    public static float dropProbability5 = 0.001f;

    public GameObject itemBoxPrefab;
    public GameObject itemBoxPrefab2;
    public GameObject itemBoxPrefab3;
    public GameObject itemBoxPrefab4;
    public GameObject itemBoxPrefab5;

    bool isLive;

    Rigidbody2D rigid;
    SpriteRenderer sprite;


    void Start()
    {
        StartCoroutine(EvolveVisuals());

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, GameObject.Find("World Canvas").transform);
            HUD hud = healthBarInstance.GetComponent<HUD>();
            hud.enemyTarget = this;
        }
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (!isLive)
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive)
            return;

        sprite.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable()
{
    target = GameManager.instance.player.GetComponent<Rigidbody2D>();
    isLive = true;
    health = maxHealth;

    if (healthBarInstance == null)
    {
        GameObject canvas = GameObject.Find("World Canvas");
        healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);

        // ✅ HUD를 통해 healthSlider 연결
        HUD hud = healthBarInstance.GetComponent<HUD>();
        hud.enemyTarget = this;
        healthSlider = healthBarInstance.GetComponent<Slider>();
    }
    else
    {
        // ✅ healthSlider를 다시 연결
        healthSlider = healthBarInstance.GetComponent<Slider>();
    }

    UpdateHealthBar();  // ← 이 때 healthSlider가 null이 아니도록 보장
}


    void Update()
    {
        if (healthBarInstance != null)
        {
            // 체력바 위치를 Enemy 머리 위로 맞춤
            Vector3 worldPos = transform.position + new Vector3(0, -0.8f, 0);
            healthBarInstance.transform.position = worldPos;
            UpdateHealthBar();
        }
    }

    IEnumerator EvolveVisuals()
    {
        float interval = 60f; // 1분마다
        float r = sprite.color.r;
        float g = sprite.color.g;
        float b = sprite.color.b;
        Vector3 originalScale = transform.localScale;

        // 단계 구분
        bool turningRed = true;
        bool turningBlack = false;
        bool scalingUp = false;

        while (isLive)
        {
            yield return new WaitForSeconds(interval);

            if (turningRed)
            {
                // G와 B 감소 (붉어짐)
                g = Mathf.Max(0, g - 0.2f);
                b = Mathf.Max(0, b - 0.2f);
                sprite.color = new Color(1f, g, b);

                if (g <= 0 && b <= 0)
                {
                    turningRed = false;
                    turningBlack = true;
                }
            }
            else if (turningBlack)
            {
                // R 감소 (검게 변함)
                r = Mathf.Max(0, r - 0.2f);
                sprite.color = new Color(r, 0f, 0f);

                if (r <= 0)
                {
                    turningBlack = false;
                    scalingUp = true;
                }
            }
            else if (scalingUp)
            {
                // 크기 증가
                transform.localScale += new Vector3(0.2f, 0.2f, 0f);
            }
        }
    }


    void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = health / maxHealth;
        }
    }

    void OnDisable()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    public void Init(SpawnData data)
{
    anim.runtimeAnimatorController = animCon[data.spriteType];
    speed = data.speed;
    maxHealth = data.health;
    health = data.health;  // ✅ 추가적으로 체력값 직접 반영
    UpdateHealthBar();     // ✅ 체력바 갱신
}

    void OnTriggerEnter2D(Collider2D collision)
{
    Player player = GameObject.FindWithTag("Player").GetComponent<Player>();

    // ✅ SpecialWeapons 무기 충돌 처리
    if (collision.CompareTag("SpecialWeapons"))
    {
        float damage = collision.GetComponent<Special_Weapon>().damage;
        SoundManager.instance.PlaySFX(SoundManager.instance.Enemy_HurtClip);
        health -= (int)damage;
        UpdateHealthBar();
        DrawDamage((int)damage, false);

        if (health <= 0)
        {
            Dead();
        }
        return;
    }

    // ✅ Bullet (TestBullet) 충돌 처리
    TestBullet bullet = collision.GetComponent<TestBullet>();
    if (bullet != null)
    {
        
        SoundManager.instance.PlaySFX(SoundManager.instance.Enemy_HurtClip);
        health -= bullet.damage;
        DrawDamage(bullet.damage, bullet.isCritical);

        // ✅ 폭발 탄환이면 폭발 실행
        if (TestBullet.Explosion_Bullet)
        {
            bullet.Explosion(transform.position);
        }


        if (health <= 0)
            Dead();

        Destroy(collision.gameObject);
    }

}


    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Enemy_HurtClip);
        health -= damage;
        UpdateHealthBar();
        DrawDamage(damage, false);

        if (health <= 0)
        {
            Dead();
        }
    }

    public void TakeExplosionDamage(int damage)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Enemy_HurtClip);
        health -= damage;
        UpdateHealthBar();
        Debug.Log("폭발데미지 :" + damage);
        DrawDamage(damage, false);

        if (health <= 0)
        {
            Dead();
        }
    }

    private void DrawDamage(int damage, bool isCriticalHit)
    {
        Vector3 worldPosition = transform.position + new Vector3(0, 1.5f, 0); // 머리 위
        GameObject canvas = GameObject.Find("World Canvas"); // 월드 스페이스 캔버스

        if (canvas == null)
        {
            Debug.LogWarning("Canvas를 찾을 수 없습니다.");
            return;
        }

        GameObject prefab = isCriticalHit ? Critical_damageTextPrefab : damageTextPrefab;

        GameObject dmgText = Instantiate(prefab, canvas.transform);
        dmgText.transform.position = worldPosition;

        var textComponent = dmgText.GetComponent<DamageText>();
        if (textComponent != null)
        {
            textComponent.SetDamage(damage);
        }
        else
        {
            Debug.LogWarning("DamageText 컴포넌트를 찾을 수 없습니다.");
        }
    }

    void Dead()
    {
        deathPosition = transform.position;

        EnemyManager.Instance.RemoveEnemy();
        SoundManager.instance.PlaySFX(SoundManager.instance.Enemy_DiedClip);

        if (Random.value < dropProbability) DropItemBox();
        if (Random.value < dropProbability2) DropItemBox2();
        if (Random.value < dropProbability3) DropItemBox3();
        if (Random.value < dropProbability4) DropItemBox4();
        if (Random.value < dropProbability5) DropItemBox5();

        StartCoroutine(DisableAfterDelay(0.1f));
    }

    void DropItemBox() => Instantiate(itemBoxPrefab, deathPosition, Quaternion.identity);
    void DropItemBox2() => Instantiate(itemBoxPrefab2, deathPosition, Quaternion.identity);
    void DropItemBox3() => Instantiate(itemBoxPrefab3, deathPosition, Quaternion.identity);
    void DropItemBox4() => Instantiate(itemBoxPrefab4, deathPosition, Quaternion.identity);
    void DropItemBox5() => Instantiate(itemBoxPrefab5, deathPosition, Quaternion.identity);

    IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
