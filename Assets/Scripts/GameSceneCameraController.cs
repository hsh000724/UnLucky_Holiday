using UnityEngine;
using UnityEngine.U2D; // PixelPerfectCamera

public class GameSceneCameraController : MonoBehaviour
{
    private PixelPerfectCamera pixelCam;

    void Start()
    {
        pixelCam = Camera.main.GetComponent<PixelPerfectCamera>();

        if (pixelCam == null)
        {
            Debug.LogError("[GameSceneCameraController] PixelPerfectCamera 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // CameraZoomManager가 없다면 새로 생성
        if (CameraZoomManager.Instance == null)
        {
            new GameObject("CameraZoomManager").AddComponent<CameraZoomManager>();
        }

        // basePPU 설정 (첫 진입 시)
        if (CameraZoomManager.Instance.GetBasePPU() <= 1)
            CameraZoomManager.Instance.SetBasePPU(pixelCam.assetsPPU);

        // 적용
        ApplyZoom();
    }

    void ApplyZoom()
    {
        float zoom = CameraZoomManager.Instance.GetZoom();
        int basePPU = CameraZoomManager.Instance.GetBasePPU();

        pixelCam.assetsPPU = Mathf.RoundToInt(basePPU * zoom);

        Debug.Log($"[GameSceneCameraController] PixelPerfectCamera 적용됨 - Zoom:{zoom:F2}, PPU:{pixelCam.assetsPPU}");
    }
}
