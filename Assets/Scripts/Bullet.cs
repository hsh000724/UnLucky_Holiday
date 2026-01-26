using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 1f; // 탄환의 생명 시간
    public int damage = 20;

    void Start()
    {
        // 5초 뒤에 탄환을 파괴
        Destroy(gameObject, lifeTime);
    }
}
