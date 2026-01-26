using UnityEngine;
using UnityEngine.UI;

public class CooldownUI : MonoBehaviour
{
    public Image cooldownImage;
    private bool isCooldown = false;
    private float currentCount = 0;
    private float maxCount = 1;

    // 초기 설정
    public void Initialize(int max)
    {
        maxCount = max;
        currentCount = max;
        cooldownImage.fillAmount = 1f;
        isCooldown = false;
    }

    // 쿨타임 시작 시 호출
    public void StartCooldown()
    {
        currentCount = 0;
        isCooldown = true;
    }

    // 매 프레임 혹은 쿨다운 로직에 따라 업데이트
    public void UpdateCooldown(int count)
    {
        if (!isCooldown) return;

        currentCount = count;
        cooldownImage.fillAmount = (float)currentCount / maxCount;

        if (currentCount >= maxCount)
        {
            isCooldown = false;
        }
    }

    // ✅ maxCount가 변경되었을 때 호출
    public void UpdateMaxCount(int newMax)
    {
        // 최대값을 갱신하고, 현재값도 재계산해서 fillAmount가 정확히 반영되도록 함
        maxCount = newMax;

        // 혹시 currentCount가 존재한다면 fillAmount 업데이트
        cooldownImage.fillAmount = (float)currentCount / maxCount;
    }
}
