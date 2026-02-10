using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox3 : MonoBehaviour
{
    private Animator animator;
    private MiniMapController miniMap;

    [Header("Item Effect Values")]
    public int AtkbuffAmount = 20;
    public int Big_Heal = 100;
    public int Big_Add_Health = 30;
    public float SpeedbuffAmount = 1.3f;
    public float Range_ScopeSize = 1.1f;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 🔹 미니맵 등록 (일반 상자)
        miniMap = FindObjectOfType<MiniMapController>();
        if (miniMap != null)
        {
            miniMap.RegisterBox(transform, false); // false = 일반
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
            if (!CollectionManager.Instance.IsCollected(CollectibleType.BlueOrb))
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
                ItemOption.instance?.Heal(Big_Heal, Big_Add_Health);
                break;
            case 2:
                ItemOption.instance?.PlayerSpeedUp(player, SpeedbuffAmount);
                break;
            case 3:
                ItemOption.instance?.IncreaseBullet(player);
                break;
            case 4:
                ItemOption.instance?.AddBulletFirePoint(player);
                break;
            case 5:
                ItemOption.instance?.IncreasedRange(player, Range_ScopeSize);
                break;
            case 6:
                if (isInfinityMode)
                {
                    ItemOption.instance?.IncreaseBullet(player);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("추가 공격 횟수 증가", 3f, new Color(1f, 0.2f, 0.2f));
                }
                else
                {
                    CollectionManager.Instance.Collect(CollectibleType.BlueOrb);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("오브3 획득", 3f, new Color(1f, 0.2f, 0.2f));
                }
                break;
        }
    }
}
