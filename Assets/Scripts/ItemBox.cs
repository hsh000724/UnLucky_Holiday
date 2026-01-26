using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    private Animator animator;

    public int AtkbuffAmount = 1;
    public int Small_Heal = 5;
    public int Small_Add_Health = 1;
    public float SpeedbuffAmount = 1.05f;
    public int CooldownAmount = 1;

    // 공격력 증가를 위한 추가 버프량 (Infinity 모드 case 5용)
    public int Infinity_AtkBuffAmount = 2; // 예를 들어 일반 버프(case 0)보다 조금 더 높게 설정할 수 있습니다.


    void Start()
    {
        animator = GetComponent<Animator>();
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

        // 0부터 4번 효과는 무조건 선택 가능
        for (int i = 0; i <= 4; i++)
        {
            availableEffects.Add(i);
        }

        // --- 수정된 부분 시작 ---

        // 현재 모드가 Infinity 모드인지 확인
        bool isInfinityMode = ModeManager.instance != null && ModeManager.instance.currentMode == ModeManager.GameMode.Infinity;

        if (!isInfinityMode)
        {
            // Infinity 모드가 아닐 경우, 미수집 상태일 때만 수집 아이템 (case 5) 추가
            if (!CollectionManager.Instance.IsCollected(CollectibleType.RedOrb))
            {
                availableEffects.Add(5); // case 5: 수집 아이템 획득
            }
        }
        else // Infinity 모드인 경우
        {
            // Infinity 모드에서는 case 5를 공격력 증가로 사용하기 위해 무조건 추가
            // (이미 case 0~4가 추가되었으므로, case 5가 추가되도록 함)
            availableEffects.Add(5); // case 5: 공격력 증가 (Infinity 모드 전용)
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
                // 공격력 증가 (기본)
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.AtkBuff(player, AtkbuffAmount);
                }
                break;
            case 1:
                // 체력 회복
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.Heal(Small_Heal, Small_Add_Health);
                }
                else
                {
                    Debug.LogWarning("ItemOption이 연결되지 않았습니다.");
                }
                break;
            case 2:
                // 플레이어 이동 속도 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.PlayerSpeedUp(player, SpeedbuffAmount);
                }
                break;
            case 3:
                // 공격 속도 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.AtkSpeedUp(player, CooldownAmount);
                }
                break;
            case 4:
                // 방패 획득
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.GetShield(player);
                }
                break;
            case 5:
                // --- 수정된 부분 시작 ---
                if (isInfinityMode)
                {
                    // Infinity 모드: 공격력 증가
                    if (player != null && ItemOption.instance != null)
                    {
                        ItemOption.instance.AtkBuff(player, Infinity_AtkBuffAmount);
                        GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("추가 공격력 증가", 3f, new Color(1f, 0.2f, 0.2f));
                    }
                }
                else
                {
                    // 일반 모드: 수집 아이템 획득
                    CollectionManager.Instance.Collect(CollectibleType.RedOrb);
                    GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("오브1 획득", 3f, new Color(1f, 0.2f, 0.2f));
                }
                // --- 수정된 부분 끝 ---
                break;
        }
    }
}