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

        miniMap =
            FindObjectOfType<MiniMapController>();


        if (miniMap != null)
        {
            miniMap.RegisterBox(
                transform,
                false
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


        for (int i = 0; i <= 5; i++)
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
                    CollectibleType.GreenOrb
                ))
            {
                availableEffects.Add(6);
            }
        }
        else
        {
            availableEffects.Add(6);
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
            case 0:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.AtkBuff(
                        player,
                        AtkbuffAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box2_AttackUp"
                        );
                }
                break;


            case 1:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.Heal(
                        Normal_Heal,
                        Normal_Add_Health
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box2_Heal"
                        );
                }
                break;


            case 2:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.PlayerSpeedUp(
                        player,
                        SpeedbuffAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box2_SpeedUp"
                        );
                }
                break;


            case 3:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.AtkSpeedUp(
                        player,
                        CooldownAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box2_AttackSpeedUp"
                        );
                }
                break;


            case 4:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.CriticalChanceUp(
                        player,
                        CriticalChance_IncreaseAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box2_CriticalChanceUp"
                        );
                }
                break;


            case 5:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.CriticalMultiplier(
                        player,
                        CriticalMultiplier_IncreaseAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box2_CriticalMultiplierUp"
                        );
                }
                break;


            case 6:
                if (isInfinityMode)
                {
                    if (ItemOption.instance != null)
                    {
                        ItemOption.instance.AtkSpeedUp(
                            player,
                            Infinity_CooldownAmount
                        );

                        GameDataLogger.Instance?
                            .LogItemAcquire(
                                "Box2_AttackSpeedUp_InfinityBonus"
                            );


                        GameObject.Find(
                            "MessageManager"
                        )
                        .GetComponent<MessageManager>()
                        .ShowMessage(
                            "추가 공격 속도 증가",
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
                        CollectibleType.GreenOrb
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box2_GreenOrb"
                        );


                    GameObject.Find(
                        "MessageManager"
                    )
                    .GetComponent<MessageManager>()
                    .ShowMessage(
                        "오브2 획득",
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