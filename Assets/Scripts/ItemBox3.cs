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
                    CollectibleType.BlueOrb
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
                            "Box3_AttackUp"
                        );
                }
                break;


            case 1:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.Heal(
                        Big_Heal,
                        Big_Add_Health
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box3_Heal"
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
                            "Box3_SpeedUp"
                        );
                }
                break;


            case 3:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.IncreaseBullet(
                        player
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box3_BulletCountUp"
                        );
                }
                break;


            case 4:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.AddBulletFirePoint(
                        player
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box3_FirePointUp"
                        );
                }
                break;


            case 5:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.IncreasedRange(
                        player,
                        Range_ScopeSize
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box3_RangeUp"
                        );
                }
                break;


            case 6:
                if (isInfinityMode)
                {
                    if (ItemOption.instance != null)
                    {
                        ItemOption.instance.IncreaseBullet(
                            player
                        );

                        GameDataLogger.Instance?
                            .LogItemAcquire(
                                "Box3_BulletCountUp_InfinityBonus"
                            );


                        GameObject.Find(
                            "MessageManager"
                        )
                        .GetComponent<MessageManager>()
                        .ShowMessage(
                            "추가 공격 횟수 증가",
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
                        CollectibleType.BlueOrb
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box3_BlueOrb"
                        );


                    GameObject.Find(
                        "MessageManager"
                    )
                    .GetComponent<MessageManager>()
                    .ShowMessage(
                        "오브3 획득",
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