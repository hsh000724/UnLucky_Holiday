using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox5 : MonoBehaviour
{
    private Animator animator;
    public int Add_Lucky_Level = 2;

    // Infinity 모드에서 case 2 대체 효과로 사용할 럭키 레벨 증가량
    public int Infinity_Add_Lucky_Level = 3;

    void Start()
    {
        animator = GetComponent<Animator>();
        SoundManager.instance.PlaySFX(SoundManager.instance.Drop_LegendItem);
        GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("축하합니다! 레전더리 아이템이 등장하였습니다!", 3f, new Color(1f, 0.2f, 0.2f));
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

        // 0,1,3번 효과는 무조건 선택 가능
        availableEffects.Add(0);
        availableEffects.Add(1);
        availableEffects.Add(3);

        // --- 수정된 부분 시작 ---
        bool isInfinityMode = ModeManager.instance != null && ModeManager.instance.currentMode == ModeManager.GameMode.Infinity;

        if (!isInfinityMode)
        {
            // Infinity 모드가 아닐 경우, 미수집 상태일 때만 수집 아이템 (case 2) 추가
            if (!CollectionManager.Instance.IsCollected(CollectibleType.PurpleOrb))
            {
                availableEffects.Add(2); // case 2: 수집 아이템 획득
            }
        }
        else
        {
            // Infinity 모드: case 2를 럭키 레벨 증가로 사용하기 위해 무조건 추가
            availableEffects.Add(2); // case 2: 럭키 레벨 증가 (Infinity 모드 전용)
        }
        // --- 수정된 부분 끝 ---

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
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.MagneticField(player);
                }
                break;
            case 1:
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.EnableExplosionBullet();
                }
                break;
            case 2:
                // --- 수정된 부분 시작 ---
                if (isInfinityMode)
                {
                    // Infinity 모드: 럭키 레벨 증가
                    if (player != null && ItemOption.instance != null)
                    {
                        ItemOption.instance.IncreaseDropProbability(player, Infinity_Add_Lucky_Level);
                        GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("추가 럭키 레벨 증가", 3f, new Color(1f, 0.2f, 0.2f));
                    }
                }
                else
                {
                    // 일반 모드: 수집 아이템 획득
                    GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("오브5 획득", 3f, new Color(1f, 0.2f, 0.2f));
                    CollectionManager.Instance.Collect(CollectibleType.PurpleOrb);
                }
                // --- 수정된 부분 끝 ---
                break;
            case 3:
                ItemOption.instance.IncreaseDropProbability(player, Add_Lucky_Level);
                break;
        }
    }


}