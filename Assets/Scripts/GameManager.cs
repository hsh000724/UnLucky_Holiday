using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth; // 추가됨

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
        // 1분마다 스테이지 증가
        InvokeRepeating("IncreaseStage", 60f, 60f);
        // 기존 UI 초기화 기능 복원
        UIManager.Instance?.ResetCollectionUI();
        // 기존 총알 속성 초기화 기능 복원
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
        // 기존 게임 속도 관리 기능 복원
        GameSpeedManager.Instance?.ApplyCurrentSpeed();
    }

    void Update()
    {
        if (isLive)
        {
            gameTime = Time.time - startTime;

            BattleGameManager.Instance?.OnHpChanged(health, maxHealth);
        }
    }

    public void HandleGameClear()
    {
        if (!isLive) return;

        isLive = false;
        isCleared = true;
        CancelInvoke("IncreaseStage");

        StartCoroutine(WaitAndLoadEndingScene(2.5f));
    }

    public IEnumerator WaitAndLoadEndingScene(float delay)
    {
        yield return new WaitForSeconds(delay);

        // [변경점] LoginUIManager 대신 AuthManager 및 FirebaseAuth 활용
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        string nickname = AuthManager.Instance.UserNickname;

        ModeManager.GameMode currentMode = ModeManager.instance.currentMode;

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null && !string.IsNullOrEmpty(userId))
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

        FadeManager.instance.FadeToScene("EndingCredit");
        Time.timeScale = 1f;
    }


    public void StopGameTime()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGameTime()
    {
        Time.timeScale = 1f;
        GameSpeedManager.Instance?.ApplyCurrentSpeed();
    }

    public IEnumerator WaitAndLoadGameOverScene(float delay)
    {
        yield return new WaitForSeconds(delay);

        // [변경점] LoginUIManager 대신 AuthManager 및 FirebaseAuth 활용
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        string nickname = AuthManager.Instance.UserNickname;

        ModeManager.GameMode currentMode = ModeManager.instance.currentMode;

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null && !string.IsNullOrEmpty(userId))
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

        FadeManager.instance.FadeToScene("GameOver");
        Time.timeScale = 1f;
    }
}