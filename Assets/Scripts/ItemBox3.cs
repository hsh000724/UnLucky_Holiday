using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox3 : MonoBehaviour
{
    private Animator animator;

    public int AtkbuffAmount = 20;
    public int Big_Heal = 100;
    public int Big_Add_Health = 30;
    public float SpeedbuffAmount = 1.3f;
    public float Range_ScopeSize = 1.1f;

    // Infinity 모드에서 case 6 대체 효과로 사용할 공격 횟수 증가 여부 (ItemOption의 IncreaseBullet은 레벨 증가로 보임)
    // 여기서는 ItemBox3의 case 3과 동일하게 '공격 횟수 증가'를 사용합니다.

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
            Player player = collision.GetComponent<Player>();

            ApplyRandomEffect(player);

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
            if (!CollectionManager.Instance.IsCollected(CollectibleType.BlueOrb))
            {
                availableEffects.Add(6); // case 6: 수집 아이템 획득
            }
        }
        else
        {
            // Infinity 모드: case 6을 공격 횟수 증가로 사용하기 위해 무조건 추가
            availableEffects.Add(6); // case 6: 공격 횟수 증가 (Infinity 모드 전용)
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
                // 공격력 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.AtkBuff(player, AtkbuffAmount);
                }
                break;
            case 1:
                // 체력 회복
                if (ItemOption.instance != null)
                {
                    ItemOption.instance.Heal(Big_Heal, Big_Add_Health);
                }
                else
                {
                    Debug.LogWarning("ItemOption이 연결되지 않았습니다.");
                }
                break;
            case 2:
                //플레이어 이동속도 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.PlayerSpeedUp(player, SpeedbuffAmount);
                }
                break;
            case 3:
                //공격 횟수 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.IncreaseBullet(player);
                }
                break;
            case 4:
                //발사대 추가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.AddBulletFirePoint(player);
                }
                break;
            case 5:
                //사거리 증가
                if (player != null && ItemOption.instance != null)
                {
                    ItemOption.instance.IncreasedRange(player, Range_ScopeSize);
                }
                break;
            case 6:
                // --- 수정된 부분 시작 ---
                if (isInfinityMode)
                {
                    // Infinity 모드: 공격 횟수 증가 (case 3과 동일)
                    if (player != null && ItemOption.instance != null)
                    {
                        ItemOption.instance.IncreaseBullet(player);
                        GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("추가 공격 횟수 증가", 3f, new Color(1f, 0.2f, 0.2f));
                    }
                }
                else
                {
                    // 일반 모드: 수집 아이템 획득
                    GameObject.Find("MessageManager").GetComponent<MessageManager>().ShowMessage("오브3 획득", 3f, new Color(1f, 0.2f, 0.2f));
                    CollectionManager.Instance.Collect(CollectibleType.BlueOrb);
                }
                // --- 수정된 부분 끝 ---
                break;
        }
    }


}