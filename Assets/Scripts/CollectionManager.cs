using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;
    public GameObject SpawnPortal;
    public GameObject player;
    public BossSpawnManager bossSpawnManager;

    private HashSet<CollectibleType> collectedItems = new HashSet<CollectibleType>();

    [Header("UI")]
    public Button bossSummonButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Collect(CollectibleType type)
    {
        if (!collectedItems.Contains(type))
        {
            collectedItems.Add(type);
            UIManager.Instance?.UpdateCollectionUI(type);
        }

        if (collectedItems.Count == 5)
        {
            EnableBossSummonButton();
        }
    }

    // 🔽 추가된 함수
    public bool IsCollected(CollectibleType type)
    {
        return collectedItems.Contains(type);
    }

    private void EnableBossSummonButton()
    {
        if (bossSummonButton != null)
        {
            bossSummonButton.gameObject.SetActive(true);
            bossSummonButton.interactable = true;
            Debug.Log("🎉 모든 아이템 수집 완료! 보스 소환 가능!");

            // 🔽 포탈을 플레이어 위치에 생성하고, 해당 위치를 BossSpawnManager에 전달
            Vector3 spawnPosition = player.transform.position;
            GameObject portal = Instantiate(SpawnPortal, spawnPosition, Quaternion.identity);

            // BossSpawnManager에 포탈 위치 전달
            if (bossSpawnManager != null)
            {
                bossSpawnManager.SetSpawnPoint(portal.transform);
            }
        }
    }
}
