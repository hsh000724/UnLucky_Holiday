using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox4 : MonoBehaviour
{
    private Animator animator;

    public float BulletScale_IncreaseAmount = 0.2f;
    public float Range_ScopeSize = 0.2f;
    public int Add_Lucky_Level = 1;
    public Weapon weapon;

    // Infinity 모드에서 case 6 대체 효과로 사용할 투사체 크기 증가량
    public float Infinity_BulletScale_IncreaseAmount = 0.4f;

    void Start()
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Drop_RareItem);
        animator = GetComponent<Animator>();
        GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("축하합니다! 신성 아이템이 등장하였습니다!", 2f, new Color(0.4f, 0.6f, 1f));
        Destroy(gameObject, 60.0f);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetTrigger("OpenBox");
            // 랜덤으로 아이템 효과 결정
            ApplyRandomEffect(collision.GetComponent<Player>());

            // 아이템 박스 제거
            Destroy(gameObject, 1);
        }
    }

    void ApplyRandomEffect(Player player)
    {
        List<int> availableEffects = new List<int>();

        // 0부터 5번 효과는 무조건 선택 가능
        for (int i = 0; i <= 5; i++)
        {
            availableEffects.Add(i);
        }

        // --- 수정된 부분 시작 ---
        bool isInfinityMode = ModeManager.instance != null && ModeManager.instance.currentMode == ModeManager.GameMode.Infinity;

        if (!isInfinityMode)
        {
            // Infinity 모드가 아닐 경우, 미수집 상태일 때만 수집 아이템 (case 6) 추가
            if (!CollectionManager.Instance.IsCollected(CollectibleType.YellowOrb))
            {
                availableEffects.Add(6); // case 6: 수집 아이템 획득
            }
        }
        else
        {
            // Infinity 모드: case 6을 투사체 크기 증가로 사용하기 위해 무조건 추가
            availableEffects.Add(6); // case 6: 투사체 크기 증가 (Infinity 모드 전용)
        }
        // --- 수정된 부분 끝 ---

        // 7번 효과는 무조건 선택 가능
        availableEffects.Add(7);

        if (availableEffects.Count == 0)
        {
            Debug.Log("사용 가능한 아이템 효과가 없습니다.");
            return;
        }

        // 필터링된 리스트에서 랜덤 선택
        int randomIndex = Random.Range(0, availableEffects.Count);
        int randomEffect = availableEffects[randomIndex];

        switch (randomEffect)
        {
            case 0:
                //공격 횟수 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.IncreaseBullet(player);
                }
                break;
            case 1:
                //발사대 추가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.AddBulletFirePoint(player);
                }
                break;
            case 2:
                //투사체 크기 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.IncreaseBulletSize(player, BulletScale_IncreaseAmount);
                }
                break;
            case 3:
                //사거리 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.IncreasedRange(player, Range_ScopeSize);
                }
                break;
            case 4:
                //부활
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.Resurrection(player);
                }
                break;
            case 5:
                //스페셜 무기 추가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.Add_SpecialWeapon(player);
                }
                break;
            case 6:
                // --- 수정된 부분 시작 ---
                if (isInfinityMode)
                {
                    // Infinity 모드: 투사체 크기 증가
                    if (player != null && ItemOption.instance != null)
                    {
                        ItemOption.instance.IncreaseBulletSize(player, Infinity_BulletScale_IncreaseAmount);
                        GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("추가 투사체 크기 증가", 3f, new Color(1f, 0.2f, 0.2f));
                    }
                }
                else
                {
                    // 일반 모드: 수집 아이템 획득
                    GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("오브4 획득", 3f, new Color(1f, 0.2f, 0.2f));
                    CollectionManager.Instance.Collect(CollectibleType.YellowOrb);
                }
                // --- 수정된 부분 끝 ---
                break;
            case 7:
                ItemOption.instance.IncreaseDropProbability(player, Add_Lucky_Level);
                break;
        }
    }


}