using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox5 : MonoBehaviour
{
    private Animator animator;
    private MiniMapController miniMap;

    public int Add_Lucky_Level = 2;
    public int Infinity_Add_Lucky_Level = 3;

    void Start()
    {
        animator = GetComponent<Animator>();
        SoundManager.instance.PlaySFX(SoundManager.instance.Drop_LegendItem);

        // 🔹 미니맵 등록 (전설급)
        miniMap = FindObjectOfType<MiniMapController>();
        if (miniMap != null)
        {
            miniMap.RegisterBox(transform, true); // true = 희귀 / 전설
        }

        GameObject.Find("MessageManager")
            .GetComponent<MessageManager>()
            .ShowMessage("축하합니다! 레전더리 아이템이 등장하였습니다!", 3f, new Color(1f, 0.2f, 0.2f));

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
        List<int> availableEffects = new List<int> { 0, 1, 3 };

        bool isInfinityMode =
            ModeManager.instance != null &&
            ModeManager.instance.currentMode == ModeManager.GameMode.Infinity;

        if (!isInfinityMode)
        {
            if (!CollectionManager.Instance.IsCollected(CollectibleType.PurpleOrb))
                availableEffects.Add(2);
        }
        else
        {
            availableEffects.Add(2);
        }

        int randomEffect = availableEffects[Random.Range(0, availableEffects.Count)];

        switch (randomEffect)
        {
            case 0: ItemOption.instance?.MagneticField(player); break;
            case 1: ItemOption.instance?.EnableExplosionBullet(); break;
            case 2:
                if (isInfinityMode)
                {
                    ItemOption.instance?.IncreaseDropProbability(player, Infinity_Add_Lucky_Level);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("추가 럭키 레벨 증가", 3f, new Color(1f, 0.2f, 0.2f));
                }
                else
                {
                    CollectionManager.Instance.Collect(CollectibleType.PurpleOrb);
                    GameObject.Find("MessageManager")
                        .GetComponent<MessageManager>()
                        .ShowMessage("오브5 획득", 3f, new Color(1f, 0.2f, 0.2f));
                }
                break;
            case 3:
                ItemOption.instance?.IncreaseDropProbability(player, Add_Lucky_Level);
                break;
        }
    }
}
