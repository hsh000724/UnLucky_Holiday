using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HeartPointUI : MonoBehaviour
{
    public Slider healthBar;      // 체력바 UI
    public Player player;         // Player 스크립트

    void Start()
    {
        // 체력바의 최대값을 플레이어의 최대 체력으로 설정
        healthBar.maxValue = player.maxHealth;
        healthBar.value = player.maxHealth;  // 체력바 초기화
    }

    void Update()
    {
        // 체력바를 현재 플레이어의 체력에 맞게 업데이트
        healthBar.value = player.currentHealth;
    }
}

