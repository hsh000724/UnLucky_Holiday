using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        Vector3 pos = player.position;
        pos.z = -10f;
        transform.position = pos;
    }
}

