using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossSpawnManager : MonoBehaviour
{
    public GameObject bossPrefab;              // 보스 본체 (비활성화 상태)
    public GameObject bossSpawnEffectPrefab;   // 보스 등장 이펙트
    public GameObject dialogueManagerObject;

    private BossDialogueManager dialogueManager;

    public Transform spawnPoint;               // 등장 위치
    public GameObject bossHealthBarUI;         // 보스 체력바 UI
    public Text dialogueText;
    public Button bossSummonButton;

    // 일반 몬스터 스포너
    public GameObject monsterSpawner;


    void Start()
    {
        dialogueManager =
            dialogueManagerObject
                .GetComponent<BossDialogueManager>();
    }


    public void SetSpawnPoint(
        Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }


    public void SpawnBoss()
    {
        // 등장 위치가 없으면 소환 불가
        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "BossSpawnManager : spawnPoint가 없습니다."
            );

            return;
        }


        // 등장 이펙트
        if (bossSpawnEffectPrefab != null)
        {
            Instantiate(
                bossSpawnEffectPrefab,
                spawnPoint.position,
                Quaternion.identity
            );
        }


        // =====================================================
        // 보스 활성화
        // =====================================================

        if (bossPrefab != null)
        {
            bossPrefab.transform.position =
                spawnPoint.position;

            bossPrefab.SetActive(true);

            // 실제 소환된 순간부터 BossAI 작동
            BossAI bossAI =
                bossPrefab.GetComponent<BossAI>();

            if (bossAI != null)
            {
                bossAI.StartBossBattle();
            }

            // 보스 소환 로그
            GameDataLogger.Instance?
                .LogBossSpawn();

            Debug.Log(
                "[BossSpawnManager] 보스 실제 소환"
            );
        }


        // 체력바 활성화
        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(
                true
            );
        }


        // 등장 대사
        if (dialogueManager != null)
        {
            StartCoroutine(
                dialogueManager
                    .ShowDialogueById(
                        "Summon"
                    )
            );
        }


        // 소환 버튼 비활성화
        if (bossSummonButton != null)
        {
            bossSummonButton
                .gameObject
                .SetActive(false);
        }


        // 일반 몬스터 생성 중지
        if (monsterSpawner != null)
        {
            monsterSpawner.SetActive(
                false
            );
        }
    }
}