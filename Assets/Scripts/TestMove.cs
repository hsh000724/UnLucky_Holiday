using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    public float moveSpeed = 5f;  // 이동 속도 조정 변수

    // Update는 프레임마다 호출됩니다
    void Update()
    {
        // 입력값 받아오기
        float moveX = Input.GetAxis("Horizontal");  // 좌우 입력 (A, D 또는 왼쪽, 오른쪽 화살표)
        float moveY = Input.GetAxis("Vertical");    // 상하 입력 (W, S 또는 위, 아래 화살표)

        // 이동 벡터 계산
        Vector3 movement = new Vector3(moveX, moveY, 0f);

        // 캐릭터 이동
        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}
