using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // 원본 유지: Inspector에서 수동으로 10개의 데이터를 채워 넣습니다.
    public Transform[] spawnPoint;
    public SpawnData[] spawnData; // *주의: Awake에서 크기가 확장됩니다.

    [Header("자동 생성 설정")]
    [Tooltip("수동으로 입력한 초기 레벨의 개수 (10개)")]
    public int initialManualLevels = 10;
    [Tooltip("추가로 생성할 레벨의 개수")]
    public int levelsToGenerate = 90;

    int level;
    float timer;

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();

        // --- 레벨 자동 생성 및 배열 확장 로직 추가 ---
        GenerateAdditionalSpawnData();
        // ----------------------------------------
    }

    // 배열을 확장하고 데이터를 채우는 메서드
    void GenerateAdditionalSpawnData()
    {
        // 1. 현재 배열의 데이터를 임시 List로 복사하여 동적 작업을 준비합니다.
        List<SpawnData> tempList = new List<SpawnData>(spawnData);

        // 2. 수동 입력 개수 확인 및 안전 장치
        if (tempList.Count < initialManualLevels)
        {
            Debug.LogError("수동으로 입력된 SpawnData의 개수가 " + initialManualLevels + "개 미만입니다. 자동 생성을 건너뜁니다.");
            return;
        }

        // 3. 마지막 수동 레벨 데이터 가져오기
        SpawnData lastData = tempList[initialManualLevels - 1];

        // 4. 지정된 개수만큼 레벨 자동 생성 및 List에 추가
        for (int i = 0; i < levelsToGenerate; i++)
        {
            SpawnData newSpawnData = new SpawnData();

            // 4-1. spawnTime: 이전 데이터 값 유지
            newSpawnData.spawnTime = lastData.spawnTime;

            // 4-2. spriteType: 이전 데이터 + 1, 0부터 4까지 무한 반복 (나머지 연산 % 5)
            newSpawnData.spriteType = (lastData.spriteType + 1) % 5;

            // 4-3. health: 이전 데이터 * 1.77배 (반올림하여 정수로 변환)
            newSpawnData.health = Mathf.RoundToInt(lastData.health * 1.77f);

            // 4-4. speed: 이전 데이터 값 유지
            newSpawnData.speed = lastData.speed;

            // List에 추가
            tempList.Add(newSpawnData);

            // 다음 루프를 위해 lastData 업데이트
            lastData = newSpawnData;
        }

        // 5. 최종적으로 확장된 List를 원본 배열 (spawnData)에 다시 할당합니다.
        spawnData = tempList.ToArray();

        Debug.Log(levelsToGenerate + "개의 레벨이 자동 생성되었습니다. 최종 총 레벨: " + spawnData.Length);
    }

    void Update()
    {
        timer += Time.deltaTime;
        // 원본 유지: Time.deltaTime으로 타이머 누적

        // 원본 유지: level 계산
        // level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 60f), spawnData.Length - 1);
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 60f), spawnData.Length - 1);


        // 원본 유지: 스폰 조건 확인 및 스폰
        if (timer > spawnData[level].spawnTime)
        {
            timer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        // 원본 유지: 오브젝트 풀에서 몬스터 가져오기 및 초기화
        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        enemy.GetComponent<Enemy>().Init(spawnData[level]);

        EnemyManager.Instance.AddEnemy();
    }
}

[System.Serializable] 
public class SpawnData
{
    public float spawnTime;
    public int spriteType;
    public int health;
    public float speed;
}