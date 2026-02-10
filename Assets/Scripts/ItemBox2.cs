using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox2 : MonoBehaviour
{
    private Animator animator;
    private MiniMapController miniMap;

    [Header("Item Effect Values")]
    public int AtkbuffAmount = 5;
    public int Normal_Heal = 20;
    public int Normal_Add_Health = 10;
    public float SpeedbuffAmount = 1.15f;
    public int CooldownAmount = 2;

    public float CriticalChance_IncreaseAmount = 0.05f;
    public float CriticalMultiplier_IncreaseAmount = 0.1f;

    public int Infinity_CooldownAmount = 3;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 🔹 미니맵 등록 (일반 상자)
        miniMap = FindObjectOfType<MiniMapController>();
        if (miniMap != null)
        {
            miniMap.RegisterBox(transform, false); // false = 일반 등급
        }

        Destroy(gameObject, 60.0f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // 🔹 미니맵 제거
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
        if (miniMap != null)
        {
            miniMap.UnregisterBox(transform);
        }
    }

    void ApplyRandomEffect(Player player)
    {
        List<int> availableEffects = new List<int>();

        for (int i = 0; i <= 5; i++)
            availableEffects.Add(i);

        bool isInfinityMode =
            ModeManager.instance != null &&
            ModeManager.instance.currentMode == ModeManager.GameMode.Infinity;

        if (!isInfinityMode)
        {
            if (!CollectionManager.Instance.IsCollected(CollectibleType.GreenOrb))
                availableEffects.Add(6);
        }
        else
        {
            availableEffects.Add(6);
        }

        int randomEffect = availableEffects[Random.Range(0, availableEffects.Count)];

        switch (randomEffect)
        {
            case 0:
                ItemOption.instance?.AtkBuff(player, AtkbuffAmount);
                break;
            case 1:
                ItemOption.instance?.Heal(Normal_Heal, Normal_Add_Health);
                break;
            case 2:
                ItemOption.instance?.PlayerSpeedUp(player, SpeedbuffAmount);
                break;
            case 3:
                ItemOption.instance?.AtkSpeedUp(player, CooldownAmount);
                break;
            case 4:
                ItemOption.instance?.CriticalChanceUp(player, CriticalChance_IncreaseAmount);
                break;
            case 5:
                ItemOption.instance?.CriticalMultiplier(player, CriticalMultiplier_IncreaseAmount);
                break;
            case 6:
                if (isInfinityMode)
                {
                    ItemOption.instance?.AtkSpeedUp(player, Infinity_CooldownAmount);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("추가 공격 속도 증가", 3f, new Color(1f, 0.2f, 0.2f));
                }
                else
                {
                    CollectionManager.Instance.Collect(CollectibleType.GreenOrb);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("오브2 획득", 3f, new Color(1f, 0.2f, 0.2f));
                }
                break;
        }
    }
}
