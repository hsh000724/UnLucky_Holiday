using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraResolution : MonoBehaviour
{
    public float targetAspect = 16f / 9f; // 목표 비율 (예: 16:9)
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // 화면이 더 세로로 긴 경우 → 위아래에 검은 여백
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            // 화면이 더 가로로 긴 경우 → 좌우에 검은 여백
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}
