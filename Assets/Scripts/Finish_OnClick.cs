using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish_OnClick : MonoBehaviour
{
    public string showObjectName;
    GameObject showObject;

    void OnMouseDown()
    {
        this.gameObject.SetActive(false);
        showObject.SetActive(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        showObject = GameObject.Find(showObjectName);
        showObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
