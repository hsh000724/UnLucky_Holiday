using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBullet : MonoBehaviour
{
    public static bool Explosion_Bullet = false;
    public GameObject explosionPrefab; // 폭발 이펙트 프리팹
    
    public float bulletLifeTime; // 탄환의 생명 시간
    public int damage;
    public bool isCritical;

    public void Start()
    {
        // 5초 뒤에 탄환을 파괴
        Destroy(gameObject, bulletLifeTime);
    }

    public void Explosion(Vector3 position)
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, position, Quaternion.identity);
            Debug.Log("폭발!!");   
        }
    }
}
