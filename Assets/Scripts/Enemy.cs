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
    public float attackDamage = 10f; // 몬스터의 공격력

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
            healthBarInstance = Instantiate(
                healthBarPrefab,
                GameObject.Find("World Canvas").transform
            );

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

        Vector2 nextVec =
            dirVec.normalized *
            speed *
            Time.fixedDeltaTime;

        rigid.MovePosition(
            rigid.position + nextVec
        );

        rigid.linearVelocity = Vector2.zero;
    }


    void LateUpdate()
    {
        if (!isLive)
            return;

        sprite.flipX =
            target.position.x < rigid.position.x;
    }


    void OnEnable()
    {
        target =
            GameManager.instance.player
                .GetComponent<Rigidbody2D>();

        isLive = true;
        health = maxHealth;

        if (healthBarInstance == null)
        {
            GameObject canvas =
                GameObject.Find("World Canvas");

            healthBarInstance =
                Instantiate(
                    healthBarPrefab,
                    canvas.transform
                );

            // HUD를 통해 healthSlider 연결
            HUD hud =
                healthBarInstance.GetComponent<HUD>();

            hud.enemyTarget = this;

            healthSlider =
                healthBarInstance.GetComponent<Slider>();
        }
        else
        {
            healthSlider =
                healthBarInstance.GetComponent<Slider>();
        }

        UpdateHealthBar();
    }


    void Update()
    {
        if (healthBarInstance != null)
        {
            Vector3 worldPos =
                transform.position +
                new Vector3(0, -0.8f, 0);

            healthBarInstance.transform.position =
                worldPos;

            UpdateHealthBar();
        }
    }


    IEnumerator EvolveVisuals()
    {
        float interval = 60f;

        float r = sprite.color.r;
        float g = sprite.color.g;
        float b = sprite.color.b;

        Vector3 originalScale =
            transform.localScale;

        bool turningRed = true;
        bool turningBlack = false;
        bool scalingUp = false;

        while (isLive)
        {
            yield return new WaitForSeconds(
                interval
            );

            if (turningRed)
            {
                g = Mathf.Max(
                    0,
                    g - 0.2f
                );

                b = Mathf.Max(
                    0,
                    b - 0.2f
                );

                sprite.color =
                    new Color(
                        1f,
                        g,
                        b
                    );

                if (g <= 0 && b <= 0)
                {
                    turningRed = false;
                    turningBlack = true;
                }
            }
            else if (turningBlack)
            {
                r = Mathf.Max(
                    0,
                    r - 0.2f
                );

                sprite.color =
                    new Color(
                        r,
                        0f,
                        0f
                    );

                if (r <= 0)
                {
                    turningBlack = false;
                    scalingUp = true;
                }
            }
            else if (scalingUp)
            {
                transform.localScale +=
                    new Vector3(
                        0.2f,
                        0.2f,
                        0f
                    );
            }
        }
    }


    void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value =
                health / maxHealth;
        }
    }


    void OnDisable()
    {
        if (healthBarInstance != null)
        {
            Destroy(
                healthBarInstance
            );
        }
    }


    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController =
            animCon[data.spriteType];

        speed = data.speed;
        maxHealth = data.health;
        health = data.health;

        UpdateHealthBar();
    }


    // =========================================================
    // 공격 충돌
    // =========================================================

    void OnTriggerEnter2D(
        Collider2D collision)
    {
        // 이미 죽은 적에게 추가 로그가 들어가는 것 방지
        if (!isLive)
            return;


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
                // 기존 코드가 int로 피해를 적용하고 있으므로
                // 실제 적용 피해도 int 기준
                int damage =
                    (int)specialWeapon.damage;

                // 오버킬 피해를 로그에 포함하지 않기 위해
                // 현재 남은 HP까지만 실제 피해로 기록
                float actualDamage =
                    Mathf.Min(
                        damage,
                        health
                    );

                SoundManager.instance.PlaySFX(
                    SoundManager.instance
                        .Enemy_HurtClip
                );

                health -= damage;

                // ✅ 특수무기 피해 로그
                GameDataLogger.Instance?
                    .LogSpecialWeaponDamage(
                        actualDamage
                    );

                UpdateHealthBar();

                DrawDamage(
                    damage,
                    false
                );


                if (health <= 0)
                {
                    Dead();
                }
            }

            return;
        }


        // =====================================================
        // 기본 탄환
        // =====================================================

        TestBullet bullet =
            collision.GetComponent<TestBullet>();

        if (bullet != null)
        {
            // 남은 체력보다 큰 공격이 들어오더라도
            // 실제 HP에서 감소한 양만 데이터로 기록
            float actualDamage =
                Mathf.Min(
                    bullet.damage,
                    health
                );


            SoundManager.instance.PlaySFX(
                SoundManager.instance
                    .Enemy_HurtClip
            );


            health -= bullet.damage;


            // ✅ 기본탄 피해 기록
            // critical 여부도 같이 전달
            GameDataLogger.Instance?
                .LogBulletDamage(
                    actualDamage,
                    bullet.isCritical
                );


            UpdateHealthBar();


            DrawDamage(
                bullet.damage,
                bullet.isCritical
            );


            // 폭발탄이면 폭발 생성
            if (TestBullet.Explosion_Bullet)
            {
                bullet.Explosion(
                    transform.position
                );
            }


            if (health <= 0)
            {
                Dead();
            }


            Destroy(
                collision.gameObject
            );
        }
    }


    // =========================================================
    // 외부 공격 데미지
    // =========================================================

    public void TakeDamage(
        int damage,
        Vector3 hitPosition)
    {
        if (!isLive)
            return;


        float actualDamage =
            Mathf.Min(
                damage,
                health
            );


        SoundManager.instance.PlaySFX(
            SoundManager.instance
                .Enemy_HurtClip
        );


        health -= damage;


        /*
         * 현재 제공된 스크립트만으로는
         * 이 TakeDamage()를 호출하는 공격원이
         * 무엇인지 확인되지 않았기 때문에
         * 피해 종류를 임의로 기록하지 않음.
         *
         * 만약 자기장 공격이 이 함수를 호출하는 것이
         * 확인되면 아래 코드를 활성화하면 됨.
         *
         * GameDataLogger.Instance?
         *     .LogMagneticFieldDamage(actualDamage);
         */


        UpdateHealthBar();

        DrawDamage(
            damage,
            false
        );


        if (health <= 0)
        {
            Dead();
        }
    }


    // =========================================================
    // 폭발 데미지
    // =========================================================

    public void TakeExplosionDamage(
        int damage)
    {
        if (!isLive)
            return;


        float actualDamage =
            Mathf.Min(
                damage,
                health
            );


        SoundManager.instance.PlaySFX(
            SoundManager.instance
                .Enemy_HurtClip
        );


        health -= damage;


        // ✅ 폭발 피해 로그
        GameDataLogger.Instance?
            .LogExplosionDamage(
                actualDamage
            );


        UpdateHealthBar();


        Debug.Log(
            "폭발데미지 :" +
            damage
        );


        DrawDamage(
            damage,
            false
        );


        if (health <= 0)
        {
            Dead();
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
                "World Canvas"
            );


        if (canvas == null)
        {
            Debug.LogWarning(
                "Canvas를 찾을 수 없습니다."
            );

            return;
        }


        GameObject prefab =
            isCriticalHit
                ? Critical_damageTextPrefab
                : damageTextPrefab;


        GameObject dmgText =
            Instantiate(
                prefab,
                canvas.transform
            );


        dmgText.transform.position =
            worldPosition;


        DamageText textComponent =
            dmgText.GetComponent
                <DamageText>();


        if (textComponent != null)
        {
            textComponent.SetDamage(
                damage
            );
        }
        else
        {
            Debug.LogWarning(
                "DamageText 컴포넌트를 " +
                "찾을 수 없습니다."
            );
        }
    }


    // =========================================================
    // 적 사망
    // =========================================================

    void Dead()
    {
        // ✅ 중복 사망 방지
        //
        // 폭발이나 여러 공격이 거의 동시에 들어오면
        // Dead()가 여러 번 호출될 가능성이 있으므로
        // 킬 / 드롭 데이터 중복 방지
        if (!isLive)
            return;

        isLive = false;


        deathPosition =
            transform.position;


        EnemyManager.Instance
            .RemoveEnemy();


        SoundManager.instance.PlaySFX(
            SoundManager.instance
                .Enemy_DiedClip
        );


        // ✅ 적 처치 로그
        GameDataLogger.Instance?
            .LogEnemyKill();


        // 방해몬스터 여부
        bool isHinder =
            gameObject.CompareTag(
                "HinderEnemy"
            );


        BattleGameManager.Instance?
            .OnEnemyKilled(
                isHinder
            );


        // 방해몬스터는 아이템 드롭 없음
        if (!isHinder)
        {
            if (Random.value <
                dropProbability)
            {
                DropItemBox();
            }


            if (Random.value <
                dropProbability2)
            {
                DropItemBox2();
            }


            if (Random.value <
                dropProbability3)
            {
                DropItemBox3();
            }


            if (Random.value <
                dropProbability4)
            {
                DropItemBox4();
            }


            if (Random.value <
                dropProbability5)
            {
                DropItemBox5();
            }
        }


        StartCoroutine(
            DisableAfterDelay(
                0.1f
            )
        );
    }


    // =========================================================
    // 아이템 드롭
    // =========================================================

    void DropItemBox()
    {
        Instantiate(
            itemBoxPrefab,
            deathPosition,
            Quaternion.identity
        );


        // ✅ 1번 상자 실제 드롭
        GameDataLogger.Instance?
            .LogBoxDrop(
                "ItemBox1"
            );
    }


    void DropItemBox2()
    {
        Instantiate(
            itemBoxPrefab2,
            deathPosition,
            Quaternion.identity
        );


        // ✅ 2번 상자 실제 드롭
        GameDataLogger.Instance?
            .LogBoxDrop(
                "ItemBox2"
            );
    }


    void DropItemBox3()
    {
        Instantiate(
            itemBoxPrefab3,
            deathPosition,
            Quaternion.identity
        );


        // ✅ 3번 상자 실제 드롭
        GameDataLogger.Instance?
            .LogBoxDrop(
                "ItemBox3"
            );
    }


    void DropItemBox4()
    {
        Instantiate(
            itemBoxPrefab4,
            deathPosition,
            Quaternion.identity
        );


        // ✅ 4번 상자 실제 드롭
        GameDataLogger.Instance?
            .LogBoxDrop(
                "ItemBox4"
            );
    }


    void DropItemBox5()
    {
        Instantiate(
            itemBoxPrefab5,
            deathPosition,
            Quaternion.identity
        );


        // ✅ 5번 상자 실제 드롭
        GameDataLogger.Instance?
            .LogBoxDrop(
                "ItemBox5"
            );
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
}