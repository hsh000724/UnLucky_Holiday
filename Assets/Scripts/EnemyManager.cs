using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public int currentEnemyCount;
    public int maxEnemyCount = 100;

    private bool isGameOverTriggered = false;


    private void Awake()
    {
        Instance = this;
    }


    // =========================================================
    // 적 추가
    // =========================================================

    public void AddEnemy()
    {
        currentEnemyCount++;

        // Battle 모드 관련 적 수 갱신
        BattleGameManager.Instance?
            .OnEnemyCountChanged(
                currentEnemyCount
            );

        CheckEnemyLimit();
    }


    // =========================================================
    // 적 제거
    // =========================================================

    public void RemoveEnemy()
    {
        currentEnemyCount--;

        if (currentEnemyCount < 0)
        {
            currentEnemyCount = 0;
        }

        BattleGameManager.Instance?
            .OnEnemyCountChanged(
                currentEnemyCount
            );
    }


    // =========================================================
    // 몬스터 수 제한
    // =========================================================

    private void CheckEnemyLimit()
    {
        if (ModeManager.instance == null)
        {
            return;
        }


        // =====================================================
        // Classic은 몬스터 수 제한 없음
        // =====================================================

        if (ModeManager.instance.currentMode ==
            ModeManager.GameMode.Classic)
        {
            return;
        }


        // =====================================================
        // Infinity / Hardcore / Battle
        // 몬스터 수 제한 적용
        // =====================================================

        if (currentEnemyCount >= maxEnemyCount &&
            !isGameOverTriggered)
        {
            isGameOverTriggered = true;


            Debug.LogWarning(
                "[EnemyManager] 몬스터 수 제한 도달\n" +
                $"Mode : {ModeManager.instance.currentMode}\n" +
                $"Enemy : {currentEnemyCount}/{maxEnemyCount}"
            );


            // ✅ GameManager에 게임오버 원인 전달
            StartCoroutine(
                GameManager.instance
                    .WaitAndLoadGameOverScene(
                        1f,
                        "EnemyLimitReached"
                    )
            );
        }
    }
}