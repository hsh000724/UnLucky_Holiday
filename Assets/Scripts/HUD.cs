using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum InfoType { Time, Health, EnemyHealth }
    public InfoType type;

    Text myText;
    Slider mySlider;

    // ✅ Enemy 참조 추가
    public Enemy enemyTarget;

    void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }

    void LateUpdate()
    {
        switch (type)
        {
            case InfoType.Time:
                float elapsedTime = Time.time - GameManager.instance.startTime;
                int minutes = Mathf.FloorToInt(elapsedTime / 60);
                int seconds = Mathf.FloorToInt(elapsedTime % 60);
                myText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
                break;

            case InfoType.Health:
                float currentHealth = GameManager.instance.health;
                float maxHealth = GameManager.instance.maxHealth;
                mySlider.value = currentHealth / maxHealth;
                break;

            case InfoType.EnemyHealth:
                if (enemyTarget != null)
                {
                    mySlider.value = enemyTarget.health / enemyTarget.maxHealth;
                }
                else
                {
                    gameObject.SetActive(false); // 타겟이 없으면 비활성화
                }
                break;
        }
    }
    
}
