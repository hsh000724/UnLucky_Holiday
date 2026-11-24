using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public bool isLive;
    public float startTime;
    public float gameTime;
    public float health;
    public float maxHealth = 100;
    public PoolManager pool;
    public Player player;
    public Weapon weapon;
    public Weapon1 weapon1;
    public int stageCount = 1;
    public bool isCleared = false;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        GameStart();
        startTime = Time.time;

        // ✅ 플레이 데이터 세션 시작
        GameDataLogger.Instance?.StartSession();

        // 1분마다 스테이지 증가
        InvokeRepeating("IncreaseStage", 60f, 60f);

        // 기존 UI 초기화
        UIManager.Instance?.ResetCollectionUI();

        // 폭발탄 상태 초기화
        TestBullet.Explosion_Bullet = false;
    }


    private void IncreaseStage()
    {
        stageCount++;
    }


    public void GameStart()
    {
        health = maxHealth;
        isLive = true;
        isCleared = false;

        // 기존 게임 속도 관리
        GameSpeedManager.Instance?.ApplyCurrentSpeed();
    }


    void Update()
    {
        if (isLive)
        {
            gameTime = Time.time - startTime;

            BattleGameManager.Instance?.OnHpChanged(
                health,
                maxHealth
            );
        }
    }


    // =========================================================
    // 게임 클리어
    // =========================================================

    public void HandleGameClear()
    {
        if (!isLive)
            return;

        isLive = false;
        isCleared = true;

        CancelInvoke("IncreaseStage");


        // ✅ 클리어 플레이 로그 저장
        GameDataLogger.Instance?.EndSession(
            gameTime,
            stageCount,
            true
        );


        StartCoroutine(
            WaitAndLoadEndingScene(2.5f)
        );
    }


    // =========================================================
    // 엔딩 씬 이동
    // =========================================================

    public IEnumerator WaitAndLoadEndingScene(
        float delay)
    {
        yield return new WaitForSeconds(delay);


        // 로그인 정보
        string userId =
            FirebaseAuth.DefaultInstance
                .CurrentUser?.UserId;

        string nickname =
            AuthManager.Instance.UserNickname;


        ModeManager.GameMode currentMode =
            ModeManager.instance.currentMode;


        // 최고 기록 저장
        ScoreManager scoreManager =
            FindFirstObjectByType<ScoreManager>();


        if (scoreManager != null &&
            !string.IsNullOrEmpty(userId))
        {
            scoreManager.SaveBestScore(
                userId,
                nickname,
                gameTime,
                stageCount,
                isCleared,
                currentMode
            );
        }


        // 엔딩 크레딧 이동
        FadeManager.instance.FadeToScene(
            "EndingCredit"
        );

        Time.timeScale = 1f;
    }


    // =========================================================
    // 게임 일시정지
    // =========================================================

    public void StopGameTime()
    {
        Time.timeScale = 0f;
    }


    public void ResumeGameTime()
    {
        Time.timeScale = 1f;

        GameSpeedManager.Instance?
            .ApplyCurrentSpeed();
    }


    // =========================================================
    // 게임오버
    // =========================================================

    public IEnumerator WaitAndLoadGameOverScene(
        float delay,
        string reason)
    {
        Debug.LogWarning(
            "==============================\n" +
            "[GAME OVER]\n" +
            $"Reason : {reason}\n" +
            $"HP : {health}\n" +
            $"Enemy Count : " +
            $"{(EnemyManager.Instance != null ? EnemyManager.Instance.currentEnemyCount : -1)}\n" +
            $"GameTime : {gameTime}\n" +
            $"Stage : {stageCount}\n" +
            "=============================="
        );


        // 게임 진행 정지
        isLive = false;

        CancelInvoke("IncreaseStage");


        // =====================================================
        // ✅ 게임오버 원인 기록
        // =====================================================

        GameDataLogger.Instance?
            .SetDeathCause(
                reason
            );


        // =====================================================
        // ✅ 현재 세션 Firestore 저장
        // =====================================================

        GameDataLogger.Instance?
            .EndSession(
                gameTime,
                stageCount,
                false
            );


        yield return new WaitForSeconds(delay);


        string userId =
            FirebaseAuth.DefaultInstance
                .CurrentUser?.UserId;


        string nickname =
            AuthManager.Instance != null
                ? AuthManager.Instance.UserNickname
                : "Unknown";


        ModeManager.GameMode currentMode =
            ModeManager.instance.currentMode;


        ScoreManager scoreManager =
            FindFirstObjectByType<ScoreManager>();


        if (scoreManager != null &&
            !string.IsNullOrEmpty(userId))
        {
            scoreManager.SaveBestScore(
                userId,
                nickname,
                gameTime,
                stageCount,
                isCleared,
                currentMode
            );
        }


        FadeManager.instance.FadeToScene(
            "GameOver"
        );


        Time.timeScale = 1f;
    }
}