using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossSpawnManager : MonoBehaviour
{
    public GameObject bossPrefab;              // 보스 본체 (비활성화 상태)
    public GameObject bossSpawnEffectPrefab;   // 보스 등장 이펙트 (선택)
    public GameObject dialogueManagerObject;
    private BossDialogueManager dialogueManager;
    public Transform spawnPoint;               // 등장 위치
    public GameObject bossHealthBarUI;         // 보스 체력바 UI
    public Text dialogueText;                  // (선택) 등장 대사 텍스트
    public Button bossSummonButton;

    // 일반 몬스터 스포너 오브젝트
    public GameObject monsterSpawner;

    void Start()
    {
        dialogueManager = dialogueManagerObject.GetComponent<BossDialogueManager>();
    }

    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    public void SpawnBoss()
    {
        // 등장 이펙트 재생
        if (bossSpawnEffectPrefab != null && spawnPoint != null)
        {
            Instantiate(bossSpawnEffectPrefab, spawnPoint.position, Quaternion.identity);
        }

        // 보스 활성화
        if (bossPrefab != null)
        {
            bossPrefab.transform.position = spawnPoint.position;
            bossPrefab.SetActive(true);
        }

        // 체력바 활성화
        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(true);
        }

        // 대사 출력
        StartCoroutine(dialogueManager.ShowDialogueById("Summon"));

        // 버튼 비활성화
        bossSummonButton.gameObject.SetActive(false);

        // 일반 몬스터 스포너 비활성화
        if (monsterSpawner != null)
        {
            monsterSpawner.SetActive(false);
        }
    }
}
