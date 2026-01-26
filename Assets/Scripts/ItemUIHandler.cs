using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemUIHandler : MonoBehaviour
{
    [Header("텍스트 UI 연결")]
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("아이콘 UI 연결")]
    [SerializeField] private Image[] itemIcons;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite silhouetteSprite;

    private void OnEnable()
    {
        // 매니저의 이벤트에 UI 갱신 함수 등록
        if (ItemRechargeManager.instance != null)
        {
            ItemRechargeManager.instance.OnDataChanged += UpdateUI;
            UpdateUI(); // 활성화될 때 즉시 갱신
        }
    }

    private void OnDisable()
    {
        // 이벤트 해제
        if (ItemRechargeManager.instance != null)
            ItemRechargeManager.instance.OnDataChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        var manager = ItemRechargeManager.instance;
        if (manager == null) return;

        // 아이템 개수 텍스트
        if (itemCountText != null)
            itemCountText.text = $"{manager.currentItemCount} / {manager.maxItemCount}";

        // 타이머 텍스트
        if (timerText != null)
        {
            if (manager.currentItemCount >= manager.maxItemCount)
                timerText.text = "MAX";
            else
            {
                TimeSpan t = TimeSpan.FromSeconds(manager.timeRemaining);
                timerText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            }
        }

        // 아이콘 업데이트
        for (int i = 0; i < itemIcons.Length; i++)
        {
            if (itemIcons[i] == null) continue;
            itemIcons[i].sprite = (i < manager.currentItemCount) ? activeSprite : silhouetteSprite;
        }
    }
}