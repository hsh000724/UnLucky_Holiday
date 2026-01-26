using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playershoot : MonoBehaviour
{
    public GameObject bulletPrefab; // 발사할 탄환의 프리팹
    public Transform firePoint; // 탄환이 발사될 위치
    public float bulletSpeed = 10f; // 탄환 속도

    void Update()
    {
        // 마우스 왼쪽 버튼 클릭 시 발사
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // 탄환 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 탄환의 Rigidbody2D 가져오기
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // 탄환의 방향을 플레이어가 바라보는 방향으로 설정 (firePoint의 로컬 업 방향으로 발사)
        rb.linearVelocity = firePoint.up * bulletSpeed;
    }
}
