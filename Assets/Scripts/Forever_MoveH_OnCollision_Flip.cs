using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forever_MoveH_OnCollision_Flip : MonoBehaviour
{

    public float speed = 1;

    Rigidbody2D rbody;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        rbody.gravityScale = 0;
        rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        rbody.linearVelocity = new Vector2(speed, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        speed = -speed;
        this.GetComponent<SpriteRenderer>().flipX = (speed < 0);
    }
}
