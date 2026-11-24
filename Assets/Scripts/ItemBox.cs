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

    public int Infinity_AtkBuffAmount = 2;

    void Start()
    {
        animator = GetComponent<Animator>();

        miniMap = FindObjectOfType<MiniMapController>();

        if (miniMap != null)
        {
            bool isRare =
                rarity != BoxRarity.Normal;

            miniMap.RegisterBox(
                transform,
                isRare
            );
        }

        Destroy(
            gameObject,
            60.0f
        );
    }


    void OnTriggerEnter2D(
        Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;


        if (miniMap != null)
        {
            miniMap.UnregisterBox(
                transform
            );
        }


        animator.SetTrigger(
            "OpenBox"
        );


        ApplyRandomEffect(
            collision.GetComponent<Player>()
        );


        Destroy(
            gameObject,
            1.0f
        );
    }


    void OnDestroy()
    {
        if (miniMap != null)
        {
            miniMap.UnregisterBox(
                transform
            );
        }
    }


    void ApplyRandomEffect(
        Player player)
    {
        List<int> availableEffects =
            new List<int>();


        for (int i = 0; i <= 4; i++)
        {
            availableEffects.Add(i);
        }


        bool isInfinityMode =
            ModeManager.instance != null &&
            ModeManager.instance.currentMode ==
            ModeManager.GameMode.Infinity;


        if (!isInfinityMode)
        {
            if (!CollectionManager.Instance
                .IsCollected(
                    CollectibleType.RedOrb
                ))
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
            Debug.Log(
                "사용 가능한 아이템 효과가 없습니다."
            );

            return;
        }


        int randomEffect =
            availableEffects[
                Random.Range(
                    0,
                    availableEffects.Count
                )
            ];


        switch (randomEffect)
        {
            // 공격력 증가
            case 0:
                if (player != null &&
                    ItemOption.instance != null)
                {
                    ItemOption.instance.AtkBuff(
                        player,
                        AtkbuffAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box1_AttackUp"
                        );
                }
                break;


            // 체력 회복
            case 1:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.Heal(
                        Small_Heal,
                        Small_Add_Health
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box1_Heal"
                        );
                }
                break;


            // 이동속도
            case 2:
                if (player != null &&
                    ItemOption.instance != null)
                {
                    ItemOption.instance.PlayerSpeedUp(
                        player,
                        SpeedbuffAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box1_SpeedUp"
                        );
                }
                break;


            // 공격속도
            case 3:
                if (player != null &&
                    ItemOption.instance != null)
                {
                    ItemOption.instance.AtkSpeedUp(
                        player,
                        CooldownAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box1_AttackSpeedUp"
                        );
                }
                break;


            // 쉴드 획득
            case 4:
                if (player != null &&
                    ItemOption.instance != null)
                {
                    ItemOption.instance.GetShield(
                        player
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box1_Shield"
                        );
                }
                break;


            case 5:
                if (isInfinityMode)
                {
                    if (player != null &&
                        ItemOption.instance != null)
                    {
                        ItemOption.instance.AtkBuff(
                            player,
                            Infinity_AtkBuffAmount
                        );

                        GameDataLogger.Instance?
                            .LogItemAcquire(
                                "Box1_AttackUp_InfinityBonus"
                            );


                        GameObject.Find(
                            "MessageManager"
                        )
                        .GetComponent<MessageManager>()
                        .ShowMessage(
                            "추가 공격력 증가",
                            3f,
                            new Color(
                                1f,
                                0.2f,
                                0.2f
                            )
                        );
                    }
                }
                else
                {
                    CollectionManager.Instance.Collect(
                        CollectibleType.RedOrb
                    );

                    // ✅ 상자에서 RedOrb를 획득했다는 기록
                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box1_RedOrb"
                        );


                    GameObject.Find(
                        "MessageManager"
                    )
                    .GetComponent<MessageManager>()
                    .ShowMessage(
                        "오브1 획득",
                        3f,
                        new Color(
                            1f,
                            0.2f,
                            0.2f
                        )
                    );
                }

                break;
        }
    }
}