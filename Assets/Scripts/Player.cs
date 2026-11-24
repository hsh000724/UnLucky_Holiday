using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using GoogleMobileAds.Api; // AdManager 사용을 위해 추가

public class Player : MonoBehaviour
{
    // [UI 및 조작 변수]
    public Joystick joystick;
    public Button attackButton;
    public Button shieldButton;
    public Vector2 inputVec;
    public CooldownUI cooldownUI;

    // [능력치 및 아이템 변수]
    public float speed;
    public float max_speed = 20;
    public float maxHealth = 100f;
    public float currentHealth;
    public int Resurrection_Count = 1;
    public float bulletSpeed = 20f;
    public float bulletScaleMultiplier = 1f;
    public float bulletLifeTime = 0.2f;
    public int bulletDamage = 10;
    public float criticalChance = 0.2f;
    public float criticalMultiplier = 1.5f;
    public int bulletCount = 1;
    public bool Explosion_Bullet = false;
    public int shieldItemCount = 1;
    public float invincibilityDuration = 3f;
    public bool Have_MagneticField = false;
    public int maxCount = 20;
    public int Lucky_Level = 0;

    // 광고 부활 사용 여부
    private bool _hasUsedAdRevive = false;

    // ✅ 데이터 로그용
    // 마지막으로 체력이 0이 된 원인을 임시 보관
    private string pendingDeathCause = "None";


    // [GameObject 및 이펙트 변수]
    public GameObject ResurrectionObject;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform firePointPrefab;
    public List<Transform> firePoints = new List<Transform>();

    private Vector2 moveDirection;
    private Collider2D playerCollider;

    public GameObject shieldEffectPrefab;
    private GameObject activeShieldEffect;

    private bool isInvincible = false;
    private Color originalColor;

    public float hurtEffectDuration = 0.2f;

    private bool isHurtEffectPlaying = false;
    private bool isShieldEffectPlaying = false;

    public GameObject tombstonePrefab;
    public float fadeDuration = 1f;


    // 광고 부활 로직
    [Header("Ad Revive Settings")]
    public GameObject ReviveUIPanel;
    public Text ReviveCountdownText;
    public float AdPromptDuration = 10f;

    private Coroutine _adPromptCoroutine;


    // [내부 상태 변수]
    int count = 0;

    private bool isShooting = false;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    public Text shieldCountText;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }


    void Start()
    {
        attackButton.onClick.AddListener(OnAttackButtonPressed);
        shieldButton.onClick.AddListener(OnShieldButtonPressed);

        cooldownUI.Initialize(maxCount);

        currentHealth = maxHealth;

        UpdateShieldCountUI();
        UpdateResurrectionState();

        firePoints.Add(firePoint);

        originalColor = spriter.color;
    }


    void Update()
    {
        if (!GameManager.instance.isLive)
            return;


        // 조작 입력 처리
#if UNITY_EDITOR
        if (Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0)
        {
            inputVec.x =
                Input.GetAxisRaw("Horizontal");

            inputVec.y =
                Input.GetAxisRaw("Vertical");
        }
        else
        {
            inputVec.x = joystick.Horizontal;
            inputVec.y = joystick.Vertical;
        }
#else
        inputVec.x = joystick.Horizontal;
        inputVec.y = joystick.Vertical;
#endif


        moveDirection =
            inputVec.normalized;


        if (moveDirection != Vector2.zero)
        {
            float baseAngle =
                Mathf.Atan2(
                    moveDirection.y,
                    moveDirection.x
                ) * Mathf.Rad2Deg;


            int firePointCount =
                firePoints.Count;


            for (int i = 0;
                i < firePointCount;
                i++)
            {
                float angleOffset =
                    (i == 0)
                        ? 0
                        : (i == 1 || i == 2)
                            ? ((i % 2 == 0)
                                ? -15f
                                : 15f)
                            : (i == 3 || i == 4)
                                ? ((i % 2 == 0)
                                    ? -30f
                                    : 30f)
                                : (i == 5 || i == 6)
                                    ? ((i % 2 == 0)
                                        ? -45f
                                        : 45f)
                                    : 0;


                firePoints[i].rotation =
                    Quaternion.Euler(
                        0,
                        0,
                        baseAngle +
                        angleOffset
                    );
            }
        }


        currentHealth =
            GameManager.instance.health;

        maxHealth =
            GameManager.instance.maxHealth;


        // PC 테스트용 쉴드
        if (Input.GetKeyDown(KeyCode.Space) &&
            shieldItemCount > 0)
        {
            UseShield();
        }
    }


    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;


        Vector2 nextVec =
            inputVec.normalized *
            speed *
            Time.fixedDeltaTime;


        rigid.MovePosition(
            rigid.position +
            nextVec
        );


        if (count < maxCount)
        {
            count++;

            cooldownUI.UpdateCooldown(
                count
            );
        }
    }


    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;


        if (inputVec.x != 0)
        {
            spriter.flipX =
                inputVec.x < 0;
        }


        if (Mathf.Abs(inputVec.y) >
            Mathf.Abs(inputVec.x))
        {
            if (inputVec.y > 0)
            {
                SetDirection("Up");
            }
            else if (inputVec.y < 0)
            {
                SetDirection("Down");
            }
        }
        else if (Mathf.Abs(inputVec.x) > 0)
        {
            if (inputVec.x > 0)
            {
                SetDirection("Right");
            }
            else if (inputVec.x < 0)
            {
                SetDirection("Left");
            }
        }
        else
        {
            SetDirection(null);
        }
    }


    // =========================================================
    // 버튼 입력
    // =========================================================

    public void OnAttackButtonPressed()
    {
        if (count >= maxCount &&
            !isShooting)
        {
            cooldownUI.StartCooldown();

            Shoot();

            count = 0;
        }
    }


    public void OnShieldButtonPressed()
    {
        if (shieldItemCount > 0)
        {
            UseShield();
        }
    }


    void SetDirection(string direction)
    {
        anim.SetBool("Up", false);
        anim.SetBool("Down", false);
        anim.SetBool("Left", false);
        anim.SetBool("Right", false);


        if (!string.IsNullOrEmpty(
            direction))
        {
            anim.SetBool(
                direction,
                true
            );
        }
    }


    // =========================================================
    // 데미지 계산
    // =========================================================

    public (
        int damage,
        bool isCriticalHit
    ) CalculateDamage()
    {
        bool isCriticalHit =
            UnityEngine.Random.value <=
            criticalChance;


        int damage =
            bulletDamage;


        if (isCriticalHit)
        {
            SoundManager.instance.PlaySFX(
                SoundManager.instance
                    .CriticalClip
            );


            damage =
                Mathf.RoundToInt(
                    damage *
                    criticalMultiplier
                );


            Debug.Log(
                "Critical Hit! Damage: " +
                damage
            );
        }
        else
        {
            SoundManager.instance.PlaySFX(
                SoundManager.instance
                    .ShootClip
            );


            Debug.Log(
                "Normal Hit. Damage: " +
                damage
            );
        }


        return (
            damage,
            isCriticalHit
        );
    }


    // =========================================================
    // 공격
    // =========================================================

    void Shoot()
    {
        isShooting = true;

        anim.SetTrigger(
            "Attack1"
        );

        StartCoroutine(
            ShootCoroutine()
        );
    }


    IEnumerator ShootCoroutine()
    {
        int completed = 0;
        int total = firePoints.Count;


        foreach (Transform fp
            in firePoints)
        {
            StartCoroutine(
                draw_bullet(
                    fp,
                    () =>
                    {
                        completed++;
                    }
                )
            );
        }


        while (completed < total)
        {
            yield return null;
        }


        isShooting = false;
    }


    public void AddFirePoint()
    {
        if (firePoints.Count >= 7)
            return;


        int count =
            firePoints.Count;


        float angleOffset =
            (count % 2 == 0)
                ? -15f
                : 15f;


        Transform lastFirePoint =
            firePoints[count - 1];


        Transform newFirePoint =
            Instantiate(
                firePoint,
                firePoint.position,
                lastFirePoint.rotation *
                Quaternion.Euler(
                    0,
                    0,
                    angleOffset
                )
            );


        newFirePoint.SetParent(
            transform
        );


        firePoints.Add(
            newFirePoint
        );
    }


    IEnumerator draw_bullet(
        Transform firePoint,
        System.Action onComplete)
    {
        List<GameObject> bullets =
            new List<GameObject>();


        for (int i = 0;
            i < bulletCount;
            i++)
        {
            GameObject bullet =
                Instantiate(
                    bulletPrefab,
                    firePoint.position,
                    firePoint.rotation
                );


            var (
                damage,
                isCrit
            ) = CalculateDamage();


            TestBullet bulletScript =
                bullet.GetComponent<TestBullet>();


            if (bulletScript != null)
            {
                bulletScript.bulletLifeTime =
                    bulletLifeTime;

                bulletScript.damage =
                    damage;

                bulletScript.isCritical =
                    isCrit;


                Destroy(
                    bullet,
                    bulletScript.bulletLifeTime
                );
            }
            else
            {
                Destroy(
                    bullet,
                    bulletLifeTime
                );
            }


            if (isCrit)
            {
                SoundManager.instance.PlaySFX(
                    SoundManager.instance
                        .CriticalClip
                );
            }
            else
            {
                SoundManager.instance.PlaySFX(
                    SoundManager.instance
                        .ShootClip
                );
            }


            bullet.transform.localScale *=
                bulletScaleMultiplier;


            bullets.Add(
                bullet
            );


            Rigidbody2D rb =
                bullet.GetComponent<Rigidbody2D>();


            if (rb != null)
            {
                rb.linearVelocity =
                    firePoint.right *
                    bulletSpeed;
            }


            bullet.tag =
                "Bullet";


            Collider2D bulletCollider =
                bullet.GetComponent<Collider2D>();


            if (bulletCollider != null)
            {
                Collider2D playerCollider =
                    GetComponent<Collider2D>();


                Physics2D.IgnoreCollision(
                    bulletCollider,
                    playerCollider
                );
            }


            yield return
                new WaitForSeconds(
                    0.1f
                );
        }


        onComplete?.Invoke();
    }


    // =========================================================
    // 피격 이펙트
    // =========================================================

    IEnumerator HurtEffect()
    {
        if (isHurtEffectPlaying)
            yield break;


        isHurtEffectPlaying =
            true;


        spriter.color =
            Color.red;


        yield return
            new WaitForSeconds(
                hurtEffectDuration
            );


        spriter.color =
            originalColor;


        isHurtEffectPlaying =
            false;
    }


    IEnumerator ShieldEffect()
    {
        if (isShieldEffectPlaying)
            yield break;


        isShieldEffectPlaying =
            true;


        spriter.color =
            Color.yellow;


        yield return
            new WaitForSeconds(
                invincibilityDuration
            );


        spriter.color =
            originalColor;


        isShieldEffectPlaying =
            false;
    }


    // =========================================================
    // 접촉 데미지
    // =========================================================

    void OnCollisionStay2D(
        Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;


        string tag =
            collision.gameObject.tag;


        float damagePerSecond = 0f;
        float modeMagnification = 1f;


        // 하드코어 모드
        if (ModeManager.instance.currentMode ==
            ModeManager.GameMode.Hardcore)
        {
            modeMagnification = 10f;
        }


        // 스테이지 배율
        int stageLevel =
            (GameManager.instance.stageCount / 5)
            + 1;


        float stageMagnification =
            (float)stageLevel;


        // 기본 접촉 데미지
        if (tag == "Enemy")
        {
            damagePerSecond = 5f;
        }
        else if (tag == "Boss")
        {
            damagePerSecond = 15f;
        }
        else
        {
            return;
        }


        if (isInvincible)
            return;


        float finalDamage =
            damagePerSecond *
            modeMagnification *
            stageMagnification;


        // 이번 프레임 피해량
        float frameDamage =
            Time.deltaTime *
            finalDamage;


        // 실제 감소한 HP만 기록
        float actualDamage =
            Mathf.Min(
                frameDamage,
                GameManager.instance.health
            );


        // 데미지 적용
        GameManager.instance.health -=
            frameDamage;


        // ============================
        // ✅ 받은 피해량 기록
        // ============================

        if (tag == "Enemy")
        {
            GameDataLogger.Instance?
                .LogEnemyDamageTaken(
                    actualDamage
                );
        }
        else if (tag == "Boss")
        {
            GameDataLogger.Instance?
                .LogBossDamageTaken(
                    actualDamage
                );
        }


        StartCoroutine(
            HurtEffect()
        );


        SoundManager.instance.PlaySFX(
            SoundManager.instance
                .HitClip
        );


        // 사망 판정
        if (GameManager.instance.health <= 0)
        {
            if (tag == "Enemy")
            {
                pendingDeathCause =
                    "EnemyContact";
            }
            else if (tag == "Boss")
            {
                pendingDeathCause =
                    "BossContact";
            }


            HandleDeathDecision();
        }
    }


    // =========================================================
    // 보스 직접 공격 데미지
    // =========================================================

    public void TakeDamage(float damage)
    {
        if (!GameManager.instance.isLive ||
            isInvincible)
        {
            return;
        }


        float actualDamage =
            Mathf.Min(
                damage,
                GameManager.instance.health
            );


        GameManager.instance.health -=
            damage;


        // ✅ 보스 공격 피해 기록
        GameDataLogger.Instance?
            .LogBossDamageTaken(
                actualDamage
            );


        StartCoroutine(
            HurtEffect()
        );


        SoundManager.instance.PlaySFX(
            SoundManager.instance
                .HitClip
        );


        if (GameManager.instance.health <= 0)
        {
            pendingDeathCause =
                "BossAttack";


            HandleDeathDecision();
        }
    }


    // =========================================================
    // 사망 / 부활 결정
    // =========================================================

    private void HandleDeathDecision()
    {
        // 플레이어 사망 상태
        GameManager.instance.isLive =
            false;


        anim.SetTrigger(
            "Death"
        );


        SoundManager.instance.PlaySFX(
            SoundManager.instance
                .DeathClip
        );


        playerCollider.enabled =
            false;


        // 부활 아이템
        if (Resurrection_Count > 0)
        {
            Resurrection_Count--;


            RevivePlayer(
                true
            );
        }

        // 광고 부활 기회
        else if (!_hasUsedAdRevive)
        {
            StartAdRevivePrompt();
        }

        // 최종 사망
        else
        {
            Player_Die();
        }
    }


    // =========================================================
    // 광고 부활
    // =========================================================

    private void StartAdRevivePrompt()
    {
        Debug.Log(
            "광고 시청 후 부활 프롬프트를 시작합니다."
        );


        GameManager.instance.StopGameTime();


        ReviveUIPanel.SetActive(
            true
        );


        AdManager.Instance
            .OnRewardedAdCompleted +=
            HandleAdReviveResult;


        if (ReviveCountdownText != null)
        {
            ReviveCountdownText.text =
                Mathf.CeilToInt(
                    AdPromptDuration
                ).ToString();
        }


        _adPromptCoroutine =
            StartCoroutine(
                AdReviveTimeoutCoroutine()
            );
    }


    IEnumerator AdReviveTimeoutCoroutine()
    {
        float remainingTime =
            AdPromptDuration;


        while (remainingTime > 0)
        {
            remainingTime -=
                Time.unscaledDeltaTime;


            if (ReviveCountdownText != null)
            {
                int seconds =
                    Mathf.CeilToInt(
                        remainingTime
                    );


                ReviveCountdownText.text =
                    seconds.ToString();
            }


            yield return null;
        }


        if (ReviveCountdownText != null)
        {
            ReviveCountdownText.text =
                "0";
        }


        Debug.Log(
            "타임아웃. 게임 종료 처리."
        );


        AdManager.Instance
            .OnRewardedAdCompleted -=
            HandleAdReviveResult;


        HandleAdReviveResult(
            false
        );
    }


    public void OnReviveAdButtonClicked()
    {
        if (_adPromptCoroutine != null)
        {
            StopCoroutine(
                _adPromptCoroutine
            );
        }


        ReviveUIPanel.SetActive(
            false
        );


        AdManager.Instance
            .ShowRewardedAd();
    }


    public void OnNoButtonClicked()
    {
        Debug.Log(
            "'아니오' 버튼 클릭. 게임 종료 처리."
        );


        if (_adPromptCoroutine != null)
        {
            StopCoroutine(
                _adPromptCoroutine
            );
        }


        AdManager.Instance
            .OnRewardedAdCompleted -=
            HandleAdReviveResult;


        HandleAdReviveResult(
            false
        );
    }


    private void HandleAdReviveResult(
        bool success)
    {
        AdManager.Instance
            .OnRewardedAdCompleted -=
            HandleAdReviveResult;


        ReviveUIPanel.SetActive(
            false
        );


        GameManager.instance
            .ResumeGameTime();


        if (success)
        {
            RevivePlayer(
                false
            );
        }
        else
        {
            Player_Die();
        }
    }


    // =========================================================
    // 부활
    // =========================================================

    public void RevivePlayer(
        bool isItemRevive)
    {
        GameManager.instance.health =
            GameManager.instance.maxHealth;


        GameManager.instance.isLive =
            true;


        playerCollider.enabled =
            true;


        if (isItemRevive)
        {
            // ✅ 아이템 부활 사용 기록
            GameDataLogger.Instance?
                .LogItemRevive();


            UpdateResurrectionState();
        }
        else
        {
            // 광고 부활 1회 제한
            _hasUsedAdRevive =
                true;


            // ✅ 광고 부활 성공 기록
            GameDataLogger.Instance?
                .LogAdRevive();


            Debug.Log(
                "광고 부활 사용 완료. " +
                "다음 사망 시 광고 부활 불가."
            );
        }


        StartCoroutine(
            ActivateInvincibility()
        );


        SoundManager.instance.PlaySFX(
            SoundManager.instance
                .ResurrectionClip
        );


        Debug.Log(
            $"플레이어 부활! " +
            $"(Source: " +
            $"{(isItemRevive ? "Item" : "Ad")})"
        );
    }


    // =========================================================
    // 최종 사망
    // =========================================================

    public void Player_Die()
    {
        // 대전모드
        if (ModeManager.instance.currentMode ==
            ModeManager.GameMode.Battle)
        {
            BattleGameManager.Instance?
                .OnPlayerDead();

            StartCoroutine(
                DeathEffect()
            );

            return;
        }

        // 원인을 못 잡은 경우도 None으로 남기지 않음
        string finalDeathCause =
            pendingDeathCause == "None"
                ? "UnknownPlayerDeath"
                : pendingDeathCause;

        AdManager.Instance
            .ShowInterstitialAd();

        StartCoroutine(
            DeathEffect()
        );

        StartCoroutine(
            GameManager.instance
                .WaitAndLoadGameOverScene(
                    5f,
                    finalDeathCause
                )
        );
    }


    // =========================================================
    // 사망 이펙트
    // =========================================================

    public IEnumerator DeathEffect()
    {
        Vector3 targetPosition =
            transform.position;


        Vector3 startPosition =
            targetPosition +
            new Vector3(
                0f,
                3f,
                0f
            );


        GameObject tombstone =
            Instantiate(
                tombstonePrefab,
                startPosition,
                Quaternion.identity
            );


        float fallDuration =
            fadeDuration;


        float elapsed =
            0f;


        Color originalColor =
            spriter.color;


        while (elapsed < fallDuration)
        {
            tombstone.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    elapsed /
                    fallDuration
                );


            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    elapsed /
                    fadeDuration
                );


            spriter.color =
                new Color(
                    originalColor.r,
                    originalColor.g,
                    originalColor.b,
                    alpha
                );


            elapsed +=
                Time.deltaTime;


            yield return null;
        }


        tombstone.transform.position =
            targetPosition;


        spriter.color =
            new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                0f
            );
    }


    // =========================================================
    // 쉴드
    // =========================================================

    public void UpdateShieldCountUI()
    {
        shieldCountText.text =
            shieldItemCount.ToString();
    }


    public void UseShield()
    {
        if (shieldItemCount > 0 &&
            !isInvincible)
        {
            shieldItemCount--;


            // ✅ 실제 쉴드 사용 횟수 기록
            GameDataLogger.Instance?
                .LogShieldUse();


            UpdateShieldCountUI();


            StartCoroutine(
                ActivateInvincibility()
            );


            StartCoroutine(
                ShieldEffect()
            );


            SoundManager.instance.PlaySFX(
                SoundManager.instance
                    .ShieldActivateClip
            );


            if (shieldEffectPrefab != null &&
                activeShieldEffect == null)
            {
                activeShieldEffect =
                    Instantiate(
                        shieldEffectPrefab,
                        transform.position,
                        Quaternion.identity
                    );


                activeShieldEffect.transform
                    .SetParent(
                        transform
                    );


                activeShieldEffect.transform
                    .localPosition =
                    new Vector3(
                        0f,
                        -1f,
                        0f
                    );
            }
        }
    }


    // =========================================================
    // 부활 상태 UI
    // =========================================================

    public void UpdateResurrectionState()
    {
        if (ResurrectionObject != null)
        {
            ResurrectionObject.SetActive(
                Resurrection_Count >= 1
            );
        }
    }


    // =========================================================
    // 무적
    // =========================================================

    private IEnumerator ActivateInvincibility()
    {
        isInvincible =
            true;


        Debug.Log(
            "Player is invincible!"
        );


        if (activeShieldEffect != null)
        {
            activeShieldEffect.SetActive(
                true
            );
        }


        yield return
            new WaitForSeconds(
                invincibilityDuration
            );


        isInvincible =
            false;


        Debug.Log(
            "Player is no longer invincible."
        );


        if (activeShieldEffect != null)
        {
            Destroy(
                activeShieldEffect
            );


            activeShieldEffect =
                null;
        }
    }
}