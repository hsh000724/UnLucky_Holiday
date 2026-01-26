using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnKeyPress_TestMoveGravity : MonoBehaviour
{
    public float speed = 3;
    public float jumppower = 8;

    float vx = 0;
    bool leftFlag = false;
    bool pushFlag = false;
    bool jumpFlag = false;
    bool groundFlag = false;

    Rigidbody2D rbody;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    void Update()
    {
        vx = 0;
        if(Input.GetKey("right"))
        {
            vx = speed;
            leftFlag = false;
        }
        if(Input.GetKey("left"))
        {
            vx = -speed;
            leftFlag = true;
        }
        if(Input.GetKey("space") && groundFlag)
        {
            if(pushFlag ==  false)
            {
            jumpFlag = true;
            pushFlag = true;
            }
        }
        else
        {
            pushFlag = false;
        }
    }
    private void FixedUpdate()
    {
        rbody.linearVelocity = new Vector2(vx, rbody.linearVelocity.y);
        this.GetComponent<SpriteRenderer>().flipX = leftFlag;
        if(jumpFlag)
        {
            jumpFlag = false;
            rbody.AddForce(new Vector2(0, jumppower), ForceMode2D.Impulse);
        }
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        groundFlag = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        groundFlag = false;
    }
}
