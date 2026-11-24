using System;
using System.Collections.Generic;
using UnityEngine;

using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;


public class GameDataLogger : MonoBehaviour
{
    public static GameDataLogger Instance { get; private set; }

    private FirebaseFirestore db;

    // ─────────────────────────────────────
    // 세션 기본 정보
    // ─────────────────────────────────────

    private string sessionId;
    private string userId;
    private string nickname;
    private string gameMode;

    private bool sessionStarted = false;
    private bool sessionEnded = false;


    // ─────────────────────────────────────
    // 게임 결과
    // ─────────────────────────────────────

    private float playTime;
    private int stageReached;
    private bool isCleared;

    private string deathCause = "None";


    // ─────────────────────────────────────
    // 전투 데이터
    // ─────────────────────────────────────

    private int enemyKillCount = 0;

    private float bulletDamageDealt = 0f;
    private float specialWeaponDamageDealt = 0f;
    private float explosionDamageDealt = 0f;
    private float magneticFieldDamageDealt = 0f;

    private float bossDamageDealt = 0f;

    private int criticalHitCount = 0;


    // ─────────────────────────────────────
    // 플레이어 피해 데이터
    // ─────────────────────────────────────

    private float totalDamageTaken = 0f;

    private float enemyDamageTaken = 0f;
    private float bossDamageTaken = 0f;


    // ─────────────────────────────────────
    // 생존 / 행동 데이터
    // ─────────────────────────────────────

    private int shieldUseCount = 0;

    private int itemReviveCount = 0;
    private int adReviveCount = 0;


    // ─────────────────────────────────────
    // 아이템 상자 드롭
    // ─────────────────────────────────────

    private Dictionary<string, int> boxDropCounts =
        new Dictionary<string, int>();


    // ─────────────────────────────────────
    // 아이템 획득 횟수
    // ─────────────────────────────────────

    private Dictionary<string, int> itemAcquireCounts =
        new Dictionary<string, int>();


    // 아이템 획득 상세 로그
    //
    // 예:
    // AttackUp / 83초
    // AttackUp / 152초
    // Shield / 201초
    //
    private List<Dictionary<string, object>> itemAcquireLogs =
        new List<Dictionary<string, object>>();


    // ─────────────────────────────────────
    // 오브 데이터
    // ─────────────────────────────────────

    private Dictionary<string, float> orbAcquireTimes =
        new Dictionary<string, float>();

    private float allOrbsCollectedTime = -1f;


    // ─────────────────────────────────────
    // 보스 데이터
    // ─────────────────────────────────────

    private bool bossSpawned = false;
    private bool bossKilled = false;

    private float bossSpawnTime = -1f;
    private float bossKillTime = -1f;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        db = FirebaseFirestore.DefaultInstance;
    }


    // =========================================================
    // 세션 시작
    // =========================================================

    public void StartSession()
    {
        // 혹시 이전 판 데이터가 남아있다면 초기화
        ResetSessionData();

        sessionId = Guid.NewGuid().ToString();

        userId =
            FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "Guest";

        if (AuthManager.Instance != null)
        {
            nickname = AuthManager.Instance.UserNickname;
        }
        else
        {
            nickname = "Unknown";
        }

        if (ModeManager.instance != null)
        {
            gameMode =
                ModeManager.instance.currentMode.ToString();
        }
        else
        {
            gameMode = "Unknown";
        }

        sessionStarted = true;
        sessionEnded = false;

        Debug.Log(
            $"[GameDataLogger] 세션 시작\n" +
            $"Session : {sessionId}\n" +
            $"User : {userId}\n" +
            $"Mode : {gameMode}"
        );
    }


    // =========================================================
    // 적 처치
    // =========================================================

    public void LogEnemyKill()
    {
        if (!CanLog())
            return;

        enemyKillCount++;
    }


    // =========================================================
    // 플레이어가 가한 피해
    // =========================================================

    public void LogBulletDamage(
        float damage,
        bool isCritical)
    {
        if (!CanLog())
            return;

        bulletDamageDealt += damage;

        if (isCritical)
        {
            criticalHitCount++;
        }
    }


    public void LogSpecialWeaponDamage(float damage)
    {
        if (!CanLog())
            return;

        specialWeaponDamageDealt += damage;
    }


    public void LogExplosionDamage(float damage)
    {
        if (!CanLog())
            return;

        explosionDamageDealt += damage;
    }


    public void LogMagneticFieldDamage(float damage)
    {
        if (!CanLog())
            return;

        magneticFieldDamageDealt += damage;
    }


    public void LogBossDamage(
        float damage,
        bool isCritical = false)
    {
        if (!CanLog())
            return;

        bossDamageDealt += damage;

        if (isCritical)
        {
            criticalHitCount++;
        }
    }


    // =========================================================
    // 플레이어가 받은 피해
    // =========================================================

    public void LogEnemyDamageTaken(float damage)
    {
        if (!CanLog())
            return;

        enemyDamageTaken += damage;
        totalDamageTaken += damage;
    }


    public void LogBossDamageTaken(float damage)
    {
        if (!CanLog())
            return;

        bossDamageTaken += damage;
        totalDamageTaken += damage;
    }


    // =========================================================
    // 아이템 상자 드롭
    // =========================================================

    public void LogBoxDrop(string boxType)
    {
        if (!CanLog())
            return;

        if (!boxDropCounts.ContainsKey(boxType))
        {
            boxDropCounts.Add(boxType, 0);
        }

        boxDropCounts[boxType]++;
    }


    // =========================================================
    // 아이템 획득
    // =========================================================

    public void LogItemAcquire(string itemName)
    {
        if (!CanLog())
            return;

        // 획득 횟수
        if (!itemAcquireCounts.ContainsKey(itemName))
        {
            itemAcquireCounts.Add(itemName, 0);
        }

        itemAcquireCounts[itemName]++;


        // 획득 시점
        float acquireTime = GetCurrentGameTime();

        Dictionary<string, object> itemLog =
            new Dictionary<string, object>
            {
                { "itemName", itemName },
                { "time", acquireTime }
            };

        itemAcquireLogs.Add(itemLog);
    }


    // =========================================================
    // 오브 획득
    // =========================================================

    public void LogOrbAcquire(string orbName)
    {
        if (!CanLog())
            return;

        // 중복 기록 방지
        if (orbAcquireTimes.ContainsKey(orbName))
            return;

        float acquireTime = GetCurrentGameTime();

        orbAcquireTimes.Add(
            orbName,
            acquireTime
        );

        Debug.Log(
            $"[GameDataLogger] Orb 획득 : " +
            $"{orbName} / {acquireTime:F1}초"
        );
    }


    public void LogAllOrbsCollected()
    {
        if (!CanLog())
            return;

        if (allOrbsCollectedTime >= 0)
            return;

        allOrbsCollectedTime =
            GetCurrentGameTime();

        Debug.Log(
            $"[GameDataLogger] 모든 Orb 수집 : " +
            $"{allOrbsCollectedTime:F1}초"
        );
    }


    // =========================================================
    // 보스
    // =========================================================

    public void LogBossSpawn()
    {
        if (!CanLog())
            return;

        if (bossSpawned)
            return;

        bossSpawned = true;
        bossSpawnTime = GetCurrentGameTime();

        Debug.Log(
            $"[GameDataLogger] 보스 소환 : " +
            $"{bossSpawnTime:F1}초"
        );
    }


    public void LogBossKill()
    {
        if (!CanLog())
            return;

        if (bossKilled)
            return;

        bossKilled = true;
        bossKillTime = GetCurrentGameTime();

        Debug.Log(
            $"[GameDataLogger] 보스 처치 : " +
            $"{bossKillTime:F1}초"
        );
    }


    // =========================================================
    // 쉴드 / 부활
    // =========================================================

    public void LogShieldUse()
    {
        if (!CanLog())
            return;

        shieldUseCount++;
    }


    public void LogItemRevive()
    {
        if (!CanLog())
            return;

        itemReviveCount++;
    }


    public void LogAdRevive()
    {
        if (!CanLog())
            return;

        adReviveCount++;
    }


    // =========================================================
    // 사망 원인
    // =========================================================

    public void SetDeathCause(string cause)
    {
        if (!CanLog())
            return;

        deathCause = cause;
    }


    // =========================================================
    // 게임 종료
    // =========================================================

    public void EndSession(
        float finalPlayTime,
        int finalStage,
        bool cleared)
    {
        if (!sessionStarted)
        {
            Debug.LogWarning(
                "[GameDataLogger] " +
                "세션이 시작되지 않았습니다."
            );

            return;
        }

        // 중복 저장 방지
        if (sessionEnded)
            return;

        sessionEnded = true;

        playTime = finalPlayTime;
        stageReached = finalStage;
        isCleared = cleared;

        SaveSession();
    }


    // =========================================================
    // Firebase 저장
    // =========================================================

    private void SaveSession()
    {
        float bossFightDuration = -1f;

        if (bossSpawnTime >= 0)
        {
            if (bossKilled &&
                bossKillTime >= bossSpawnTime)
            {
                bossFightDuration =
                    bossKillTime - bossSpawnTime;
            }
            else
            {
                // 보스를 못 잡았으면
                // 게임 종료 시점까지 보스전 시간
                bossFightDuration =
                    playTime - bossSpawnTime;
            }
        }


        float totalDamageDealt =
            bulletDamageDealt +
            specialWeaponDamageDealt +
            explosionDamageDealt +
            magneticFieldDamageDealt;


        Dictionary<string, object> combatData =
            new Dictionary<string, object>
            {
                {
                    "enemyKillCount",
                    enemyKillCount
                },

                {
                    "totalDamageDealt",
                    totalDamageDealt
                },

                {
                    "bulletDamageDealt",
                    bulletDamageDealt
                },

                {
                    "specialWeaponDamageDealt",
                    specialWeaponDamageDealt
                },

                {
                    "explosionDamageDealt",
                    explosionDamageDealt
                },

                {
                    "magneticFieldDamageDealt",
                    magneticFieldDamageDealt
                },

                {
                    "bossDamageDealt",
                    bossDamageDealt
                },

                {
                    "criticalHitCount",
                    criticalHitCount
                }
            };


        Dictionary<string, object> survivalData =
            new Dictionary<string, object>
            {
                {
                    "totalDamageTaken",
                    totalDamageTaken
                },

                {
                    "enemyDamageTaken",
                    enemyDamageTaken
                },

                {
                    "bossDamageTaken",
                    bossDamageTaken
                },

                {
                    "shieldUseCount",
                    shieldUseCount
                },

                {
                    "itemReviveCount",
                    itemReviveCount
                },

                {
                    "adReviveCount",
                    adReviveCount
                },

                {
                    "deathCause",
                    deathCause
                }
            };


        Dictionary<string, object> itemData =
            new Dictionary<string, object>
            {
                {
                    "boxDropCounts",
                    boxDropCounts
                },

                {
                    "itemAcquireCounts",
                    itemAcquireCounts
                },

                {
                    "itemAcquireLogs",
                    itemAcquireLogs
                }
            };


        Dictionary<string, object> progressData =
            new Dictionary<string, object>
            {
                {
                    "orbAcquireTimes",
                    orbAcquireTimes
                },

                {
                    "allOrbsCollectedTime",
                    allOrbsCollectedTime
                },

                {
                    "bossSpawned",
                    bossSpawned
                },

                {
                    "bossSpawnTime",
                    bossSpawnTime
                },

                {
                    "bossKilled",
                    bossKilled
                },

                {
                    "bossKillTime",
                    bossKillTime
                },

                {
                    "bossFightDuration",
                    bossFightDuration
                }
            };


        Dictionary<string, object> data =
            new Dictionary<string, object>
            {
                {
                    "sessionId",
                    sessionId
                },

                {
                    "userId",
                    userId
                },

                {
                    "nickname",
                    nickname
                },

                {
                    "gameMode",
                    gameMode
                },

                {
                    "playTime",
                    playTime
                },

                {
                    "stageReached",
                    stageReached
                },

                {
                    "isCleared",
                    isCleared
                },

                {
                    "combat",
                    combatData
                },

                {
                    "survival",
                    survivalData
                },

                {
                    "items",
                    itemData
                },

                {
                    "progress",
                    progressData
                },

                {
                    "createdAt",
                    Timestamp.GetCurrentTimestamp()
                }
            };


        db.Collection("PlayLogs")
            .Document(sessionId)
            .SetAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log(
                        "[GameDataLogger] " +
                        $"플레이 로그 저장 완료 : {sessionId}"
                    );
                }
                else
                {
                    Debug.LogError(
                        "[GameDataLogger] " +
                        $"플레이 로그 저장 실패 : " +
                        $"{task.Exception}"
                    );
                }
            });
    }


    // =========================================================
    // 현재 플레이 시간
    // =========================================================

    private float GetCurrentGameTime()
    {
        if (GameManager.instance != null)
        {
            return GameManager.instance.gameTime;
        }

        return 0f;
    }


    // =========================================================
    // 로그 가능 여부
    // =========================================================

    private bool CanLog()
    {
        return sessionStarted && !sessionEnded;
    }


    // =========================================================
    // 초기화
    // =========================================================

    private void ResetSessionData()
    {
        sessionId = "";
        userId = "";
        nickname = "";
        gameMode = "";

        playTime = 0f;
        stageReached = 0;
        isCleared = false;

        deathCause = "None";


        enemyKillCount = 0;

        bulletDamageDealt = 0f;
        specialWeaponDamageDealt = 0f;
        explosionDamageDealt = 0f;
        magneticFieldDamageDealt = 0f;
        bossDamageDealt = 0f;

        criticalHitCount = 0;


        totalDamageTaken = 0f;
        enemyDamageTaken = 0f;
        bossDamageTaken = 0f;


        shieldUseCount = 0;

        itemReviveCount = 0;
        adReviveCount = 0;


        boxDropCounts.Clear();

        itemAcquireCounts.Clear();
        itemAcquireLogs.Clear();

        orbAcquireTimes.Clear();

        allOrbsCollectedTime = -1f;


        bossSpawned = false;
        bossKilled = false;

        bossSpawnTime = -1f;
        bossKillTime = -1f;


        sessionStarted = false;
        sessionEnded = false;
    }
}