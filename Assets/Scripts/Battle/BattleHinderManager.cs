using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHinderManager : MonoBehaviour
{
    [Header("방해몬스터 설정")]
    public GameObject[] hinderPrefabs;      // 방해몬스터 프리팹 배열
    public Transform playerTransform;    // 플레이어 위치 참조
    public float spawnRadius = 5f;   // 플레이어 주변 소환 반경
    public int hinderHP = 50;   // 방해몬스터 체력

    // 방해몬스터로 소환된 Enemy들을 추적
    private List<GameObject> _activeHinders = new List<GameObject>();

    // BattleGameManager에서 호출
    public void SpawnHinderMonster()
    {
        if (hinderPrefabs == null || hinderPrefabs.Length == 0) return;

        // 플레이어 주변 랜덤 위치 계산
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 spawnPos = playerTransform.position +
                               new Vector3(randomDir.x, randomDir.y, 0) * spawnRadius;

        // 랜덤 방해몬스터 선택
        int index = UnityEngine.Random.Range(0, hinderPrefabs.Length);
        GameObject hinder = Instantiate(hinderPrefabs[index], spawnPos, Quaternion.identity);

        // 방해몬스터 태그 설정 → Enemy.Dead()에서 구분용
        hinder.tag = "HinderEnemy";

        // 방해몬스터 체력 설정
        Enemy enemyScript = hinder.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.health = hinderHP;
            enemyScript.maxHealth = hinderHP;
        }

        _activeHinders.Add(hinder);
        StartCoroutine(RemoveFromList(hinder));
    }

    // 방해몬스터 사망 시 리스트에서 제거
    private IEnumerator RemoveFromList(GameObject hinder)
    {
        while (hinder != null && hinder.activeSelf)
            yield return new WaitForSeconds(0.5f);

        _activeHinders.Remove(hinder);
    }

    // 방해몬스터 여부 확인 (Enemy.Dead()에서 호출)
    public static bool IsHinderEnemy(GameObject enemy)
    {
        return enemy.CompareTag("HinderEnemy");
    }
}