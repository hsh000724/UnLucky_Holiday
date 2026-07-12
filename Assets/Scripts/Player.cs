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
    public float maxHealth = 100f;      // 플레이어의 최대 체력
    public float currentHealth;         // 현재 체력
    public int Resurrection_Count = 1;      // 부활 아이템 개수 (기본값 설정)
    public float bulletSpeed = 20f;     // 탄환 속도
    public float bulletScaleMultiplier = 1f; // 기본 탄환 크기 배율
    public float bulletLifeTime = 0.2f; // 기본 탄환 지속시간
    public int bulletDamage = 10;       // 탄환의 데미지
    public float criticalChance = 0.2f; // 치명타 확률 (예: 20%)
    public float criticalMultiplier = 1.5f; // 치명타 배율
    public int bulletCount = 1; // 발사 탄환 개수
    public bool Explosion_Bullet = false;
    public int shieldItemCount = 1;     // 쉴드 아이템 개수
    public float invincibilityDuration = 3f; // 무적 상태 지속 시간
    public bool Have_MagneticField = false; // 자기장 존재 여부
    public int maxCount = 20;       // 공격 쿨타임 변수
    public int Lucky_Level = 0;

    // ⭐ [추가] 광고 부활 사용 여부 플래그
    private bool _hasUsedAdRevive = false;

    // [GameObject 및 이펙트 변수]
    public GameObject ResurrectionObject;
    public GameObject bulletPrefab;     // 탄환 프리팹
    public Transform firePoint;         // 탄환이 발사될 위치
    public Transform firePointPrefab;
    public List<Transform> firePoints = new List<Transform>();
    private Vector2 moveDirection;
    private Collider2D playerCollider;
    public GameObject shieldEffectPrefab;   // 쉴드 파티클 프리팹
    private GameObject activeShieldEffect; // 활성화된 쉴드 이펙트 객체
    private bool isInvincible = false;      // 무적 상태 여부
    private Color originalColor;        // 원래 색 저장용
    public float hurtEffectDuration = 0.2f;     // 빨간색 유지 시간
    private bool isHurtEffectPlaying = false; // 중복 방지
    private bool isShieldEffectPlaying = false; // 쉴드 이펙트 중복 방지
    public GameObject tombstonePrefab; // 묘비 프리팹
    public float fadeDuration = 1f;     // 서서히 사라지는 시간

    // ✅ 광고 부활 로직 관련 변수 추가
    [Header("Ad Revive Settings")]
    public GameObject ReviveUIPanel; // **인스펙터에서 UI 패널 연결 필수**
    public Text ReviveCountdownText; // ✨추가: 카운트다운을 표시할 Text UI 컴포넌트
    public float AdPromptDuration = 10f; // 부활 선택 대기 시간 (10초)
    private Coroutine _adPromptCoroutine; // 타임아웃 코루틴 관리를 위한 변수

    // [내부 상태 변수]
    int count = 0;
    private bool isShooting = false;
    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    public Text shieldCountText;  // 쉴드 아이템 개수를 표시할 Text UI

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

        originalColor = spriter.color; // 원래 색 저장
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        // 🔸[조작 입력 처리]
#if UNITY_EDITOR
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            inputVec.x = Input.GetAxisRaw("Horizontal");
            inputVec.y = Input.GetAxisRaw("Vertical");
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

        moveDirection = inputVec.normalized;

        if (moveDirection != Vector2.zero)
        {
            float baseAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            int firePointCount = firePoints.Count;

            for (int i = 0; i < firePointCount; i++)
            {
                float angleOffset = (i == 0) ? 0 :
                                        (i == 1 || i == 2) ? ((i % 2 == 0) ? -15f : 15f) :
                                        (i == 3 || i == 4) ? ((i % 2 == 0) ? -30f : 30f) :
                                        (i == 5 || i == 6) ? ((i % 2 == 0) ? -45f : 45f) : 0;

                firePoints[i].rotation = Quaternion.Euler(0, 0, baseAngle + angleOffset);
            }
        }

        currentHealth = GameManager.instance.health;
        maxHealth = GameManager.instance.maxHealth;

        // 스페이스바로 쉴드 아이템 사용 (PC 테스트용)
        if (Input.GetKeyDown(KeyCode.Space) && shieldItemCount > 0)
        {
            UseShield();
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);

        if (count < maxCount)
        {
            count++;
            cooldownUI.UpdateCooldown(count); // 쿨다운 UI 갱신
        }
    }


    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }

        // 방향 애니메이션 처리
        if (Mathf.Abs(inputVec.y) > Mathf.Abs(inputVec.x))
        {
            if (inputVec.y > 0)
                SetDirection("Up");
            else if (inputVec.y < 0)
                SetDirection("Down");
        }
        else if (Mathf.Abs(inputVec.x) > 0)
        {
            if (inputVec.x > 0)
                SetDirection("Right");
            else if (inputVec.x < 0)
                SetDirection("Left");
        }
        else
        {
            SetDirection(null);
        }
    }

    // [버튼 이벤트 함수]
    public void OnAttackButtonPressed()
    {
        if (count >= maxCount && !isShooting)
        {
            cooldownUI.StartCooldown();  // 쿨다운 시작
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

        if (!string.IsNullOrEmpty(direction))
        {
            anim.SetBool(direction, true);
        }
    }

    // [데미지 계산 로직]
    public (int damage, bool isCriticalHit) CalculateDamage()
    {
        bool isCriticalHit = UnityEngine.Random.value <= criticalChance;
        int damage = bulletDamage;

        if (isCriticalHit)
        {
            SoundManager.instance.PlaySFX(SoundManager.instance.CriticalClip);
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
            Debug.Log("Critical Hit! Damage: " + damage);
        }
        else
        {
            SoundManager.instance.PlaySFX(SoundManager.instance.ShootClip);
            Debug.Log("Normal Hit. Damage: " + damage);
        }

        return (damage, isCriticalHit);
    }

    // [공격 로직]
    void Shoot()
    {
        isShooting = true;
        anim.SetTrigger("Attack1");
        StartCoroutine(ShootCoroutine());
    }

    IEnumerator ShootCoroutine()
    {
        int completed = 0;
        int total = firePoints.Count;

        foreach (Transform fp in firePoints)
        {
            StartCoroutine(draw_bullet(fp, () =>
            {
                completed++;
            }));
        }

        while (completed < total)
            yield return null;

        isShooting = false;
    }

    public void AddFirePoint()
    {
        if (firePoints.Count >= 7)
            return;

        int count = firePoints.Count; // 현재 FirePoint 개수
        // FirePoint 생성 로직 (기존 로직 유지)
        float angleOffset = (count % 2 == 0) ? -15f : 15f;
        Transform lastFirePoint = firePoints[count - 1];
        Transform newFirePoint = Instantiate(firePoint, firePoint.position, lastFirePoint.rotation * Quaternion.Euler(0, 0, angleOffset));
        newFirePoint.SetParent(transform);
        firePoints.Add(newFirePoint);
    }

    IEnumerator draw_bullet(Transform firePoint, System.Action onComplete)
    {
        List<GameObject> bullets = new List<GameObject>();

        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            var (damage, isCrit) = CalculateDamage();

            TestBullet bulletScript = bullet.GetComponent<TestBullet>(); // 이 부분은 주석 처리 또는 적절히 수정 필요
            if (bulletScript != null)
            {
                bulletScript.bulletLifeTime = bulletLifeTime;
                bulletScript.damage = damage;
                bulletScript.isCritical = isCrit;
                Destroy(bullet, bulletScript.bulletLifeTime);
            }
            else
            {
            // TestBullet이 없으면 기본 지속시간으로 파괴
            Destroy(bullet, bulletLifeTime);
            }

            // 사운드 및 스케일 설정
            if (isCrit)
                SoundManager.instance.PlaySFX(SoundManager.instance.CriticalClip);
            else
                SoundManager.instance.PlaySFX(SoundManager.instance.ShootClip);

            bullet.transform.localScale *= bulletScaleMultiplier;
            bullets.Add(bullet);

            // 물리적 이동
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.right * bulletSpeed;
            }

            bullet.tag = "Bullet";

            // 플레이어 충돌 무시
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            if (bulletCollider != null)
            {
                Collider2D playerCollider = GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(bulletCollider, playerCollider);
            }

            yield return new WaitForSeconds(0.1f);
        }

        onComplete?.Invoke(); // 콜백 호출
    }

    // [이펙트 코루틴]
    IEnumerator HurtEffect()
    {
        if (isHurtEffectPlaying) yield break;
        isHurtEffectPlaying = true;
        spriter.color = Color.red;
        yield return new WaitForSeconds(hurtEffectDuration);
        spriter.color = originalColor;
        isHurtEffectPlaying = false;
    }

    IEnumerator ShieldEffect()
    {
        if (isShieldEffectPlaying) yield break;
        isShieldEffectPlaying = true; // isHurtEffectPlaying 대신 isShieldEffectPlaying 사용
        spriter.color = Color.yellow; // 쉴드 사용 시 노란색 (임시)
        yield return new WaitForSeconds(invincibilityDuration);
        spriter.color = originalColor;
        isShieldEffectPlaying = false;
    }

    // [접촉 데미지 처리]
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;

        string tag = collision.gameObject.tag;
        float damagePerSecond = 0f;
        float modeMagnification = 1f;

        // 1. 모드에 따른 배율 결정 (하드코어)
        if (ModeManager.instance.currentMode == ModeManager.GameMode.Hardcore)
        {
            modeMagnification = 10f;
        }

        // 2. 스테이지에 따른 배율 결정 (5스테이지마다 1배씩 추가 증가)
        // 예: 1~4스테이지(1배), 5~9스테이지(2배), 10~14스테이지(3배)...
        int stageLevel = (GameManager.instance.stageCount / 5) + 1;
        float stageMagnification = (float)stageLevel;

        // 3. 기본 데미지 설정
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

        // 4. 최종 데미지 계산 (기본 데미지 * 모드 배율 * 스테이지 배율)
        float finalDamage = damagePerSecond * modeMagnification * stageMagnification;

        // 데미지 적용
        GameManager.instance.health -= Time.deltaTime * finalDamage;

        // 이펙트 및 사운드 (매 프레임 호출 방지를 위해 필요 시 조건부 실행 권장)
        StartCoroutine(HurtEffect());
        SoundManager.instance.PlaySFX(SoundManager.instance.HitClip);

        // 사망 판정
        if (GameManager.instance.health <= 0)
        {
            HandleDeathDecision();
        }
    }

    // [투사체 데미지 처리]
    public void TakeDamage(float damage)
    {
        if (!GameManager.instance.isLive || isInvincible)
            return;

        GameManager.instance.health -= damage;
        StartCoroutine(HurtEffect());
        SoundManager.instance.PlaySFX(SoundManager.instance.HitClip);

        // ✅ 사망 판정 로직 수정
        if (GameManager.instance.health <= 0)
        {
            HandleDeathDecision();
        }
    }

    // ⭐ 사망 시 부활 여부를 결정하는 핵심 로직 (수정됨)
    private void HandleDeathDecision()
    {
        // 1. 플레이어 사망 상태로 전환 (애니메이션, 충돌 비활성화)
        GameManager.instance.isLive = false;
        anim.SetTrigger("Death");
        SoundManager.instance.PlaySFX(SoundManager.instance.DeathClip);
        playerCollider.enabled = false;

        // 2. 부활 아이템 보유 여부 확인
        if (Resurrection_Count > 0)
        {
            Resurrection_Count--;
            RevivePlayer(true); // 아이템으로 즉시 부활
        }
        // ⭐ [수정] 광고 부활 사용 여부를 확인
        else if (!_hasUsedAdRevive)
        {
            StartAdRevivePrompt(); // 광고 부활 프롬프트 시작
        }
        // ⭐ [추가] 아이템도 없고 광고 부활 기회도 없는 경우
        else
        {
            Player_Die(); // 최종 게임 오버 처리
        }
    }

    // ⭐ 광고 부활 프롬프트 시작 (수정됨: 이벤트 구독 추가)
    private void StartAdRevivePrompt()
    {
        Debug.Log("광고 시청 후 부활 프롬프트를 시작합니다.");

        // 게임 중지
        GameManager.instance.StopGameTime();

        // UI 표시 및 이벤트 구독
        ReviveUIPanel.SetActive(true);
        // ⭐ [수정] AdManager의 이벤트를 구독하여 광고 완료 신호를 받습니다.
        AdManager.Instance.OnRewardedAdCompleted += HandleAdReviveResult;

        // ✨추가: 카운트다운 텍스트 초기화
        if (ReviveCountdownText != null)
        {
            ReviveCountdownText.text = Mathf.CeilToInt(AdPromptDuration).ToString();
        }

        // 10초 타임아웃 코루틴 시작
        _adPromptCoroutine = StartCoroutine(AdReviveTimeoutCoroutine());
    }

    // ⭐ 부활 선택 타임아웃 코루틴 (수정됨: 카운트다운 로직 추가)
    IEnumerator AdReviveTimeoutCoroutine()
    {
        float remainingTime = AdPromptDuration;

        // 카운트다운 시작
        while (remainingTime > 0)
        {
            // Time.timeScale이 0이므로 Time.unscaledDeltaTime 사용
            remainingTime -= Time.unscaledDeltaTime;

            // 남은 시간을 정수 형태로 포맷하여 UI에 표시
            if (ReviveCountdownText != null)
            {
                int seconds = Mathf.CeilToInt(remainingTime);
                ReviveCountdownText.text = seconds.ToString();
            }

            yield return null; // 매 프레임 대기 (실시간으로 업데이트됨)
        }

        // 카운트다운 완료 시 최종 사망 처리
        if (ReviveCountdownText != null)
        {
            ReviveCountdownText.text = "0"; // 최종적으로 0으로 설정
        }

        Debug.Log("타임아웃. 게임 종료 처리.");
        // ⭐ 타임아웃 시에도 구독 해제 후 처리
        AdManager.Instance.OnRewardedAdCompleted -= HandleAdReviveResult;
        HandleAdReviveResult(false);
    }

    // **[UI 버튼 연결 필수]** - '광고 보고 부활' 버튼 클릭 시
    public void OnReviveAdButtonClicked()
    {
        if (_adPromptCoroutine != null) StopCoroutine(_adPromptCoroutine);
        ReviveUIPanel.SetActive(false);
        AdManager.Instance.ShowRewardedAd(); // 보상형 광고 표시 요청
    }

    // **[UI 버튼 연결 필수]** - '아니오/게임 종료' 버튼 클릭 시 (수정됨: 구독 해제 추가)
    public void OnNoButtonClicked()
    {
        Debug.Log("'아니오' 버튼 클릭. 게임 종료 처리.");
        if (_adPromptCoroutine != null) StopCoroutine(_adPromptCoroutine);

        // ⭐ [수정] 구독 해제
        AdManager.Instance.OnRewardedAdCompleted -= HandleAdReviveResult;

        HandleAdReviveResult(false); // 부활 실패로 간주
    }

    // ⭐ AdManager 이벤트 콜백 함수 (광고 시청 결과)
    private void HandleAdReviveResult(bool success)
    {
        // ⭐ [수정] 이벤트 구독 해제 (AdManager가 호출한 경우 포함)
        AdManager.Instance.OnRewardedAdCompleted -= HandleAdReviveResult;

        ReviveUIPanel.SetActive(false);
        GameManager.instance.ResumeGameTime(); // 게임 시간 재개

        if (success)
        {
            RevivePlayer(false); // 광고 부활
        }
        else
        {
            Player_Die(); // 최종 게임 오버
        }
    }


    // ⭐ 플레이어를 부활시키는 최종 로직 (수정됨: 광고 부활 플래그 설정 추가)
    public void RevivePlayer(bool isItemRevive)
    {
        // 1. 상태 및 능력치 복구
        GameManager.instance.health = GameManager.instance.maxHealth;
        GameManager.instance.isLive = true;
        playerCollider.enabled = true;

        // ⭐ [추가] 광고 부활이었을 경우 플래그를 true로 설정 (1회 제한)
        if (!isItemRevive)
        {
            _hasUsedAdRevive = true;
            Debug.Log("광고 부활 사용 완료. 다음 사망 시 광고 부활 불가.");
        }

        // 2. 무적 시간 부여 
        StartCoroutine(ActivateInvincibility());

        // 3. 이펙트 및 사운드
        if (isItemRevive)
        {
            UpdateResurrectionState();
        }
        SoundManager.instance.PlaySFX(SoundManager.instance.ResurrectionClip);

        Debug.Log($"플레이어 부활! (Source: {(isItemRevive ? "Item" : "Ad")})");
    }

    // 최종 사망 처리 로직
    public void Player_Die()
    {
        // 대전모드인지 확인
        if (ModeManager.instance.currentMode == ModeManager.GameMode.Battle)
        {
            // 대전모드 → BattleGameManager에 위임
            // 씬 전환은 BattleGameManager가 처리
            BattleGameManager.Instance?.OnPlayerDead();
            StartCoroutine(DeathEffect());
            return; // ← GameOver 씬 이동 차단
        }

        // 싱글모드 → 기존 로직 유지
        AdManager.Instance.ShowInterstitialAd();
        StartCoroutine(DeathEffect());
        StartCoroutine(GameManager.instance.WaitAndLoadGameOverScene(5f));
    }


    // [기타 기능 함수]
    public IEnumerator DeathEffect()
    {
        // 묘비 생성 및 플레이어 페이드 아웃 로직 (기존 로직 유지)
        Vector3 targetPosition = transform.position;
        Vector3 startPosition = targetPosition + new Vector3(0f, 3f, 0f);
        GameObject tombstone = Instantiate(tombstonePrefab, startPosition, Quaternion.identity);
        float fallDuration = fadeDuration;
        float elapsed = 0f;
        Color originalColor = spriter.color;

        while (elapsed < fallDuration)
        {
            tombstone.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / fallDuration);
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriter.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        tombstone.transform.position = targetPosition;
        spriter.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }


    public void UpdateShieldCountUI()
    {
        shieldCountText.text = shieldItemCount.ToString();
    }

    public void UseShield()
    {
        if (shieldItemCount > 0 && !isInvincible)
        {
            shieldItemCount--;
            UpdateShieldCountUI();
            StartCoroutine(ActivateInvincibility());
            StartCoroutine(ShieldEffect());
            SoundManager.instance.PlaySFX(SoundManager.instance.ShieldActivateClip);

            if (shieldEffectPrefab != null && activeShieldEffect == null)
            {
                activeShieldEffect = Instantiate(shieldEffectPrefab, transform.position, Quaternion.identity);
                activeShieldEffect.transform.SetParent(transform);
                activeShieldEffect.transform.localPosition = new Vector3(0f, -1f, 0f);
            }
        }
    }

    public void UpdateResurrectionState()
    {
        if (ResurrectionObject != null)
        {
            ResurrectionObject.SetActive(Resurrection_Count >= 1);
        }
    }

    private IEnumerator ActivateInvincibility()
    {
        isInvincible = true;
        Debug.Log("Player is invincible!");

        if (activeShieldEffect != null)
        {
            activeShieldEffect.SetActive(true);
        }

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        Debug.Log("Player is no longer invincible.");

        if (activeShieldEffect != null)
        {
            Destroy(activeShieldEffect);
            activeShieldEffect = null;
        }
    }
}