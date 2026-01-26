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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !enemiesInField.Contains(enemy))
            {
                enemiesInField.Add(enemy);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemiesInField.Contains(enemy))
            {
                enemiesInField.Remove(enemy);
            }
        }
    }

    IEnumerator DamageOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초 대기

            // 🔥 핵심: 공격 시점에 GameManager에 있는 최신 데미지 값을 가져옵니다.
            float currentDamage = 0f;
            if (GameManager.instance.weapon1 != null)
            {
                currentDamage = GameManager.instance.weapon1.damage;
            }

            // 자기장 안의 모든 적에게 데미지 입힘
            for (int i = enemiesInField.Count - 1; i >= 0; i--)
            {
                // 적이 존재하고 활성화된 상태인지 체크
                if (enemiesInField[i] != null && enemiesInField[i].gameObject.activeSelf)
                {
                    // 최신 데미지를 정수로 변환하여 전달 (TakeDamage 인자에 따라 조정)
                    enemiesInField[i].TakeDamage((int)currentDamage, transform.position);
                }
                else
                {
                    // 적이 죽었거나 사라졌다면 리스트에서 제거
                    enemiesInField.RemoveAt(i);
                }
            }
        }
    }
}