using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseDown_Roulette : MonoBehaviour
{
    public float maxSpeed = 50;

    float Speed = 0;

    void OnMouseDown()
    {
        Speed = maxSpeed;
    }
    void FixedUpdate()
    {
        Speed = Speed * (float)0.97;
        this.transform.Rotate(0,0,Speed);
    }
}
