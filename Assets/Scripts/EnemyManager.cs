using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    public int currentEnemyCount;
    public int maxEnemyCount = 100;
    bool isGameOverTriggered = false;

    private void Awake()
    {
        Instance = this;
    }

    public void AddEnemy()
    {
        currentEnemyCount++;

        // ✅ 추가
        BattleGameManager.Instance?.OnEnemyCountChanged(currentEnemyCount);

        CheckEnemyLimit();
    }

    public void RemoveEnemy()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0)
            currentEnemyCount = 0;

        // ✅ 추가
        BattleGameManager.Instance?.OnEnemyCountChanged(currentEnemyCount);
    }

    void CheckEnemyLimit()
    {
        if (currentEnemyCount >= maxEnemyCount && !isGameOverTriggered)
        {
            isGameOverTriggered = true;
            StartCoroutine(GameManager.instance.WaitAndLoadGameOverScene(1f));
        }
    }
}