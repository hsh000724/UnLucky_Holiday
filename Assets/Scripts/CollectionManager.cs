using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;

    public GameObject SpawnPortal;
    public GameObject player;
    public BossSpawnManager bossSpawnManager;

    private HashSet<CollectibleType> collectedItems =
        new HashSet<CollectibleType>();


    [Header("UI")]
    public Button bossSummonButton;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    // =========================================================
    // 오브 획득
    // =========================================================

    public void Collect(CollectibleType type)
    {
        // 이미 획득한 오브라면 중복 처리하지 않음
        if (collectedItems.Contains(type))
        {
            return;
        }


        // 실제 오브 획득 처리
        collectedItems.Add(type);


        // UI 갱신
        UIManager.Instance?
            .UpdateCollectionUI(type);


        // 배틀모드 관련 기존 처리
        BattleGameManager.Instance?
            .OnCollectionAcquired(
                type.ToString()
            );


        // =====================================================
        // ✅ 오브 획득 시간 기록
        // =====================================================

        GameDataLogger.Instance?
            .LogOrbAcquire(
                type.ToString()
            );


        Debug.Log(
            $"[GameDataLogger] 오브 획득 : " +
            $"{type} / " +
            $"{collectedItems.Count}/5"
        );


        // =====================================================
        // 모든 오브 수집 완료
        // =====================================================

        if (collectedItems.Count == 5)
        {
            // ✅ 모든 오브를 모은 시간 기록
            GameDataLogger.Instance?
                .LogAllOrbsCollected();


            Debug.Log(
                "[GameDataLogger] 모든 오브 수집 완료"
            );


            EnableBossSummonButton();
        }
    }


    // =========================================================
    // 특정 오브 획득 여부
    // =========================================================

    public bool IsCollected(
        CollectibleType type)
    {
        return collectedItems.Contains(
            type
        );
    }


    // =========================================================
    // 보스 소환 활성화
    // =========================================================

    private void EnableBossSummonButton()
    {
        if (bossSummonButton != null)
        {
            bossSummonButton
                .gameObject
                .SetActive(true);


            bossSummonButton.interactable =
                true;


            Debug.Log(
                "🎉 모든 아이템 수집 완료! " +
                "보스 소환 가능!"
            );


            // 포탈을 플레이어 위치에 생성
            Vector3 spawnPosition =
                player.transform.position;


            GameObject portal =
                Instantiate(
                    SpawnPortal,
                    spawnPosition,
                    Quaternion.identity
                );


            // BossSpawnManager에
            // 포탈 위치 전달
            if (bossSpawnManager != null)
            {
                bossSpawnManager
                    .SetSpawnPoint(
                        portal.transform
                    );
            }
        }
    }
}