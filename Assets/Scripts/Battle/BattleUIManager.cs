using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    [Header("상대방 HP")]
    public Slider opponentHPSlider;
    public TMP_Text txtOpponentHP;

    [Header("상대방 처치 수")]
    public TMP_Text txtOpponentKillCount;

    [Header("상대방 컬렉션 슬롯 (5개)")]
    public CollectionSlotUI[] opponentCollectionSlots;

    [Header("내 처치 수")]
    public TMP_Text txtMyKillCount;

    [Header("방해몬스터 알림")]
    public GameObject hinderAlertUI;
    public float alertDuration = 2f;

    [Header("필드 몬스터 수")]
    public TMP_Text txtMyEnemyCount;
    public TMP_Text txtOpponentEnemyCount;

    // ─────────────────────────────────────
    // 상대방 데이터 업데이트
    // ─────────────────────────────────────

    public void UpdateOpponentHP(int hpPercent)
    {
        if (opponentHPSlider != null)
            opponentHPSlider.value = hpPercent / 100f;

        if (txtOpponentHP != null)
            txtOpponentHP.text = $"{hpPercent}%";
    }

    public void UpdateOpponentKillCount(int count)
    {
        if (txtOpponentKillCount != null)
            txtOpponentKillCount.text = $"처치: {count}";
    }

    // 상대방 컬렉션 업데이트
    // collections: Firestore에서 받은 수집된 아이템 id 목록
    // ex) ["Fire", "Water", "Wind"]
    public void UpdateOpponentCollections(List<string> collections)
    {
        if (opponentCollectionSlots == null) return;

        foreach (var slot in opponentCollectionSlots)
        {
            bool acquired = collections.Contains(slot.collectionId);
            slot.SetAcquired(acquired);
        }
    }

    public void UpdateMyKillCount(int count)
    {
        if (txtMyKillCount != null)
            txtMyKillCount.text = $"처치: {count}";
    }

    // ─────────────────────────────────────
    // 방해몬스터 알림
    // ─────────────────────────────────────

    public void ShowHinderAlert()
    {
        if (hinderAlertUI != null)
            StartCoroutine(ShowAlertCoroutine());
    }

    private IEnumerator ShowAlertCoroutine()
    {
        hinderAlertUI.SetActive(true);
        yield return new WaitForSeconds(alertDuration);
        hinderAlertUI.SetActive(false);
    }
    // ─────────────────────────────────────
    // 필드 몬스터 수 업데이트
    // ─────────────────────────────────────
    public void UpdateMyEnemyCount(int count)
    {
        if (txtMyEnemyCount != null)
            txtMyEnemyCount.text = $"필드: {count}";
    }

    public void UpdateOpponentEnemyCount(int count)
    {
        if (txtOpponentEnemyCount != null)
            txtOpponentEnemyCount.text = $"필드: {count}";
    }
}