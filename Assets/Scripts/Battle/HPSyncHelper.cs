using UnityEngine;

public class HPSyncHelper
{
    private int _lastSyncedPercent = 100;

    // HP 변경 시 호출 → true 반환 시 Firestore 업데이트 필요
    public bool ShouldSync(int currentHp, int maxHp)
    {
        if (maxHp <= 0) return false;

        float hpRatio = (float)currentHp / maxHp * 100f;
        int currentPercent = Mathf.FloorToInt(hpRatio);

        // 5% 미만 → 1%마다 업데이트
        if (currentPercent < 5)
        {
            if (currentPercent != _lastSyncedPercent)
            {
                _lastSyncedPercent = currentPercent;
                return true;
            }
            return false;
        }

        // 5% 이상 → 10% 단위 업데이트
        int currentBracket = (currentPercent / 10) * 10;
        int lastBracket = (_lastSyncedPercent / 10) * 10;

        if (currentBracket != lastBracket)
        {
            _lastSyncedPercent = currentPercent;
            return true;
        }

        return false;
    }

    public void Reset() => _lastSyncedPercent = 100;
}