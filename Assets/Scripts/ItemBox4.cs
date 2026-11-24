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
        SoundManager.instance.PlaySFX(
            SoundManager.instance.Drop_RareItem
        );


        animator =
            GetComponent<Animator>();


        miniMap =
            FindObjectOfType<MiniMapController>();


        if (miniMap != null)
        {
            miniMap.RegisterBox(
                transform,
                true
            );
        }


        GameObject.Find(
            "MessageManager"
        )
        .GetComponent<MessageManager>()
        .ShowMessage(
            "축하합니다! 신성 아이템이 등장하였습니다!",
            2f,
            new Color(
                0.4f,
                0.6f,
                1f
            )
        );


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
                    CollectibleType.YellowOrb
                ))
            {
                availableEffects.Add(6);
            }
        }
        else
        {
            availableEffects.Add(6);
        }


        availableEffects.Add(7);


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
                    ItemOption.instance.IncreaseBullet(
                        player
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box4_BulletCountUp"
                        );
                }
                break;


            case 1:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.AddBulletFirePoint(
                        player
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box4_FirePointUp"
                        );
                }
                break;


            case 2:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.IncreaseBulletSize(
                        player,
                        BulletScale_IncreaseAmount
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box4_BulletSizeUp"
                        );
                }
                break;


            case 3:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.IncreasedRange(
                        player,
                        Range_ScopeSize
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box4_RangeUp"
                        );
                }
                break;


            case 4:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.Resurrection(
                        player
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box4_Resurrection"
                        );
                }
                break;


            case 5:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.Add_SpecialWeapon(
                        player
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box4_SpecialWeapon"
                        );
                }
                break;


            case 6:
                if (isInfinityMode)
                {
                    if (ItemOption.instance != null)
                    {
                        ItemOption.instance.IncreaseBulletSize(
                            player,
                            Infinity_BulletScale_IncreaseAmount
                        );

                        GameDataLogger.Instance?
                            .LogItemAcquire(
                                "Box4_BulletSizeUp_InfinityBonus"
                            );


                        GameObject.Find(
                            "MessageManager"
                        )
                        .GetComponent<MessageManager>()
                        .ShowMessage(
                            "추가 투사체 크기 증가",
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
                        CollectibleType.YellowOrb
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box4_YellowOrb"
                        );


                    GameObject.Find(
                        "MessageManager"
                    )
                    .GetComponent<MessageManager>()
                    .ShowMessage(
                        "오브4 획득",
                        3f,
                        new Color(
                            1f,
                            0.2f,
                            0.2f
                        )
                    );
                }

                break;


            case 7:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.IncreaseDropProbability(
                        player,
                        Add_Lucky_Level
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box4_LuckyLevelUp"
                        );
                }
                break;
        }
    }
}