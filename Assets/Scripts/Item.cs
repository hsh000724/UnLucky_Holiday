using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource; 

    public AudioClip HealSound;
    public AudioClip AtkBuffSound;     
    public AudioClip PlayerSpeedUpSound;
    public AudioClip AtkSpeedUpSound;
    public AudioClip CriticalChanceUpSound;

    void Start()
    {
        // **audioSource를 현재 GameObject에서 가져오기**
        audioSource = GetComponent<AudioSource>();

        // **AudioSource가 없으면 추가**
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Heal()
    {
        if (audioSource != null && HealSound != null)
        {
            audioSource.PlayOneShot(HealSound);
        }

        if (GameManager.instance.health != GameManager.instance.maxHealth)
        {
            GameManager.instance.health += 20;
            if (GameManager.instance.health > GameManager.instance.maxHealth)
            {
                GameManager.instance.health = GameManager.instance.maxHealth;
            }
            Debug.Log("플레이어의 체력이 '20'만큼 회복 되었습니다. 현재 체력 : " + GameManager.instance.health);
        }
        else
        {
            GameManager.instance.maxHealth += 10;
            Debug.Log("플레이어의 최대 체력이 '10'만큼 증가 되었습니다. 현재 체력 : " + GameManager.instance.health);
        }
    }

    public void PlayerSpeedUp(Player player)
    {
        if (audioSource != null && PlayerSpeedUpSound != null)
        {
            audioSource.PlayOneShot(PlayerSpeedUpSound);
        }
        player.speed = player.speed * 1.15f;
        Debug.Log("플레이어의 이동속도가 '15%' 증가하였습니다. 현재 속도 : " + player.speed);
    }

    public void AtkBuff(Player player)
    {
        if (audioSource != null && AtkBuffSound != null)
        {
            audioSource.PlayOneShot(AtkBuffSound);
        }
        player.bulletDamage += 5;
        Debug.Log("플레이어의 공격력이 '5' 증가 하였습니다. 현재 공격력 :" + player.bulletDamage);
    }
}
