using UnityEngine;

public class Camera_Move : MonoBehaviour
{
    public Transform target; // 따라다닐 대상인 플레이어의 Transform

    public Vector3 offset; // 카메라와 플레이어 간의 오프셋

    public float smoothSpeed = 0.125f; // 카메라의 부드러운 이동을 위한 속도

    void LateUpdate()
    {
        // 플레이어의 위치에 오프셋을 더한 위치로 카메라를 이동시킴
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z; // 카메라의 z 위치를 고정

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 카메라가 플레이어를 바라보도록 함
        transform.LookAt(target);
    }
}
