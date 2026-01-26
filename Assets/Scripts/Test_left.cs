using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_left : MonoBehaviour
{
    public float speed = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(speed/60, 0, 0);
    }
}
