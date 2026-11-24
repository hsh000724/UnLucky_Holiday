using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticField : MonoBehaviour
{
    private List<Enemy> enemiesInField = new List<Enemy>();


    void Start()
    {
        // 데미지 주기를 관리하는 코루틴 시작
        StartCoroutine(DamageOverTime());
    }


    // =========================================================
    // 자기장 범위 진입
    // =========================================================

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy =
                collision.GetComponent<Enemy>();


            if (enemy != null &&
                !enemiesInField.Contains(enemy))
            {
                enemiesInField.Add(enemy);
            }
        }
    }


    // =========================================================
    // 자기장 범위 이탈
    // =========================================================

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy =
                collision.GetComponent<Enemy>();


            if (enemy != null &&
                enemiesInField.Contains(enemy))
            {
                enemiesInField.Remove(enemy);
            }
        }
    }


    // =========================================================
    // 자기장 지속 데미지
    // =========================================================

    IEnumerator DamageOverTime()
    {
        while (true)
        {
            // 1초마다 데미지
            yield return new WaitForSeconds(1f);


            // =================================================
            // 현재 자기장 데미지 가져오기
            // =================================================

            float currentDamage = 0f;


            if (GameManager.instance != null &&
                GameManager.instance.weapon1 != null)
            {
                currentDamage =
                    GameManager.instance.weapon1.damage;
            }


            // 기존 게임 로직과 동일하게
            // 정수 데미지로 변환
            int damageToApply =
                (int)currentDamage;


            if (damageToApply <= 0)
            {
                continue;
            }


            // =================================================
            // 자기장 안의 모든 적 공격
            // =================================================

            for (int i = enemiesInField.Count - 1;
                i >= 0;
                i--)
            {
                Enemy enemy =
                    enemiesInField[i];


                // 적이 없거나 비활성화 상태라면 제거
                if (enemy == null ||
                    !enemy.gameObject.activeSelf)
                {
                    enemiesInField.RemoveAt(i);

                    continue;
                }


                // =============================================
                // ✅ 실제 피해량 계산
                // =============================================

                // 예:
                // 자기장 공격력 10
                // 적 남은 HP 3
                // → 로그에는 3만 기록
                float actualDamage =
                    Mathf.Min(
                        damageToApply,
                        Mathf.Max(
                            enemy.health,
                            0f
                        )
                    );


                // 실제 게임 데미지 적용
                enemy.TakeDamage(
                    damageToApply,
                    transform.position
                );


                // =============================================
                // ✅ 자기장 피해 로그 기록
                // =============================================

                if (actualDamage > 0f)
                {
                    GameDataLogger.Instance?
                        .LogMagneticFieldDamage(
                            actualDamage
                        );
                }
            }
        }
    }
}