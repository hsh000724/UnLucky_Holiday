using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class onCollision_SwitchScene : MonoBehaviour
{
    public string targetObjectName;
    public string sceneName;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.name == targetObjectName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
