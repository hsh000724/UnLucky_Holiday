using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoxRarity
{
    Normal,
    Legendary,
    Divine
}

public class ItemBox : MonoBehaviour
{
    private Animator animator;
    private MiniMapController miniMap;

    [Header("MiniMap")]
    public BoxRarity rarity = BoxRarity.Normal;

    [Header("Item Effect Values")]
    public int AtkbuffAmount = 1;
    public int Small_Heal = 5;
    public int Small_Add_Health = 1;
    public float SpeedbuffAmount = 1.05f;
    public int CooldownAmount = 1;

    // 공격력 증가를 위한 추가 버프량 (Infinity 모드 case 5용)
    public int Infinity_AtkBuffAmount = 2;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 🔹 미니맵 등록
        miniMap = FindObjectOfType<MiniMapController>();
        if (miniMap != null)
        {
            bool isRare = rarity != BoxRarity.Normal;
            miniMap.RegisterBox(transform, isRare);
        }

        // 🔹 일정 시간 후 자동 제거
        Destroy(gameObject, 60.0f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // 🔹 미니맵에서 제거
        if (miniMap != null)
        {
            miniMap.UnregisterBox(transform);
        }

        animator.SetTrigger("OpenBox");
        ApplyRandomEffect(collision.GetComponent<Player>());

        Destroy(gameObject, 1.0f);
    }

    void OnDestroy()
    {
        // 🔹 예외 상황 대비 (중복 호출 방지)
        if (miniMap != null)
        {
            miniMap.UnregisterBox(transform);
        }
    }

    void ApplyRandomEffect(Player player)
    {
        List<int> availableEffects = new List<int>();

        // 0~4 기본 효과
        for (int i = 0; i <= 4; i++)
            availableEffects.Add(i);

        bool isInfinityMode =
            ModeManager.instance != null &&
            ModeManager.instance.currentMode == ModeManager.GameMode.Infinity;

        if (!isInfinityMode)
        {
            if (!CollectionManager.Instance.IsCollected(CollectibleType.RedOrb))
            {
                availableEffects.Add(5);
            }
        }
        else
        {
            availableEffects.Add(5);
        }

        if (availableEffects.Count == 0)
        {
            Debug.Log("사용 가능한 아이템 효과가 없습니다.");
            return;
        }

        int randomEffect = availableEffects[Random.Range(0, availableEffects.Count)];

        switch (randomEffect)
        {
            case 0: // 공격력 증가
                if (player != null && ItemOption.instance != null)
                    ItemOption.instance.AtkBuff(player, AtkbuffAmount);
                break;

            case 1: // 체력 회복
                if (ItemOption.instance != null)
                    ItemOption.instance.Heal(Small_Heal, Small_Add_Health);
                break;

            case 2: // 이동 속도 증가
                if (player != null && ItemOption.instance != null)
                    ItemOption.instance.PlayerSpeedUp(player, SpeedbuffAmount);
                break;

            case 3: // 공격 속도 증가
                if (player != null && ItemOption.instance != null)
                    ItemOption.instance.AtkSpeedUp(player, CooldownAmount);
                break;

            case 4: // 방패 획득
                if (player != null && ItemOption.instance != null)
                    ItemOption.instance.GetShield(player);
                break;

            case 5:
                if (isInfinityMode)
                {
                    if (player != null && ItemOption.instance != null)
                    {
                        ItemOption.instance.AtkBuff(player, Infinity_AtkBuffAmount);
                        GameObject.Find("MessageManager")
                            .GetComponent<MessageManager>()
                            .ShowMessage("추가 공격력 증가", 3f, new Color(1f, 0.2f, 0.2f));
                    }
                }
                else
                {
                    CollectionManager.Instance.Collect(CollectibleType.RedOrb);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("오브1 획득", 3f, new Color(1f, 0.2f, 0.2f));
                }
                break;
        }
    }
}
