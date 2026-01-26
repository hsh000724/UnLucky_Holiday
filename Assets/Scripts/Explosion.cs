using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float bulletLifeTime = 0.2f;
    public int Explosion_damage = 20;
    public float explosionRadius = 1.5f; // 폭발 범위 반경

    void Start()
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.ExplosionClip);
        // 폭발 효과를 중심으로 범위 내 Enemy에게 데미지
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeExplosionDamage(Explosion_damage);
            }
        }

        // 일정 시간 후에 폭발 오브젝트 제거
        Destroy(gameObject, bulletLifeTime);
    }

    void OnDrawGizmosSelected()
    {
        // 폭발 범위 시각화 (디버그용)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
