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
        animator =
            GetComponent<Animator>();


        SoundManager.instance.PlaySFX(
            SoundManager.instance.Drop_LegendItem
        );


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
            "축하합니다! 레전더리 아이템이 등장하였습니다!",
            3f,
            new Color(
                1f,
                0.2f,
                0.2f
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
            new List<int>
            {
                0,
                1,
                3
            };


        bool isInfinityMode =
            ModeManager.instance != null &&
            ModeManager.instance.currentMode ==
            ModeManager.GameMode.Infinity;


        if (!isInfinityMode)
        {
            if (!CollectionManager.Instance
                .IsCollected(
                    CollectibleType.PurpleOrb
                ))
            {
                availableEffects.Add(2);
            }
        }
        else
        {
            availableEffects.Add(2);
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
            // 자기장
            case 0:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.MagneticField(
                        player
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box5_MagneticField"
                        );
                }
                break;


            // 폭발탄
            case 1:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.EnableExplosionBullet();

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box5_ExplosionBullet"
                        );
                }
                break;


            case 2:
                if (isInfinityMode)
                {
                    if (ItemOption.instance != null)
                    {
                        ItemOption.instance.IncreaseDropProbability(
                            player,
                            Infinity_Add_Lucky_Level
                        );

                        GameDataLogger.Instance?
                            .LogItemAcquire(
                                "Box5_LuckyLevelUp_InfinityBonus"
                            );


                        GameObject.Find(
                            "MessageManager"
                        )
                        .GetComponent<MessageManager>()
                        .ShowMessage(
                            "추가 럭키 레벨 증가",
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
                        CollectibleType.PurpleOrb
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box5_PurpleOrb"
                        );


                    GameObject.Find(
                        "MessageManager"
                    )
                    .GetComponent<MessageManager>()
                    .ShowMessage(
                        "오브5 획득",
                        3f,
                        new Color(
                            1f,
                            0.2f,
                            0.2f
                        )
                    );
                }

                break;


            // 럭키 레벨
            case 3:
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.IncreaseDropProbability(
                        player,
                        Add_Lucky_Level
                    );

                    GameDataLogger.Instance?
                        .LogItemAcquire(
                            "Box5_LuckyLevelUp"
                        );
                }
                break;
        }
    }
}