using UnityEngine;

public class ChildAnimationRelay : MonoBehaviour
{
    public BossAI bossAI; // 부모에 있는 스크립트 연결

    public void DealDamage()
    {
        if (bossAI != null)
        {
            bossAI.DealDamage();
        }
    }
}
