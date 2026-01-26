using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forever_RotateX : MonoBehaviour
{
    public float angle = 90;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        this.transform.Rotate(angle/50, 0, 0);
    }
}
