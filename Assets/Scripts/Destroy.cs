using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision) // 충돌했을 때
    {
        // 충돌한 오브젝트 삭제
        Destroy(collision.gameObject);
        Destroy(this.gameObject);
    }
}