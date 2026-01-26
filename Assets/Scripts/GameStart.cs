using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public string showObjectName;

    GameObject showObject;
    float orgY = 0;
    float ofsetY = 10000;

    // Start is called before the first frame update
    void Start()
    {
        showObject = GameObject.Find(showObjectName);
        orgY = showObject.transform.position.y;
        if (orgY > ofsetY)
        {
            orgY -= ofsetY;
        }
        Vector3 pos = showObject.transform.position;
        pos.y = orgY + ofsetY;
        showObject.transform.position = pos;
    }
    void OnMouseDown()
    {
        Vector3 pos = showObject.transform.position;
        pos.y = orgY;
        showObject.transform.position = pos;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
