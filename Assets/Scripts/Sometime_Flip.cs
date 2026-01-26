using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sometime_Flip : MonoBehaviour
{
    public int maxCount = 50;

    int count = 0;
    bool flipFlag = false;

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
        count = count + 1;
        if(count >= maxCount)
        {
            this.transform.Rotate(0, 0, 180);
            count = 0;
            flipFlag = !flipFlag;
            this.GetComponent<SpriteRenderer>().flipY = flipFlag;
        }
    }
}
