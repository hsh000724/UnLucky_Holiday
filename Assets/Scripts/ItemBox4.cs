using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox4 : MonoBehaviour
{
    private Animator animator;
    private MiniMapController miniMap;

    public float BulletScale_IncreaseAmount = 0.2f;
    public float Range_ScopeSize = 0.2f;
    public int Add_Lucky_Level = 1;
    public Weapon weapon;

    public float Infinity_BulletScale_IncreaseAmount = 0.4f;

    void Start()
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Drop_RareItem);
        animator = GetComponent<Animator>();

        // 🔹 미니맵 등록 (신성급)
        miniMap = FindObjectOfType<MiniMapController>();
        if (miniMap != null)
        {
            miniMap.RegisterBox(transform, true); // true = 희귀 / 신성
        }

        GameObject.Find("MessageManager")
            .GetComponent<MessageManager>()
            .ShowMessage("축하합니다! 신성 아이템이 등장하였습니다!", 2f, new Color(0.4f, 0.6f, 1f));

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
            if (!CollectionManager.Instance.IsCollected(CollectibleType.YellowOrb))
                availableEffects.Add(6);
        }
        else
        {
            availableEffects.Add(6);
        }

        availableEffects.Add(7);

        int randomEffect = availableEffects[Random.Range(0, availableEffects.Count)];

        switch (randomEffect)
        {
            case 0: ItemOption.instance?.IncreaseBullet(player); break;
            case 1: ItemOption.instance?.AddBulletFirePoint(player); break;
            case 2: ItemOption.instance?.IncreaseBulletSize(player, BulletScale_IncreaseAmount); break;
            case 3: ItemOption.instance?.IncreasedRange(player, Range_ScopeSize); break;
            case 4: ItemOption.instance?.Resurrection(player); break;
            case 5: ItemOption.instance?.Add_SpecialWeapon(player); break;
            case 6:
                if (isInfinityMode)
                {
                    ItemOption.instance?.IncreaseBulletSize(player, Infinity_BulletScale_IncreaseAmount);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("추가 투사체 크기 증가", 3f, new Color(1f, 0.2f, 0.2f));
                }
                else
                {
                    CollectionManager.Instance.Collect(CollectibleType.YellowOrb);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("오브4 획득", 3f, new Color(1f, 0.2f, 0.2f));
                }
                break;
            case 7:
                ItemOption.instance?.IncreaseDropProbability(player, Add_Lucky_Level);
                break;
        }
    }
}
