using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D; // PixelPerfectCamera

public class CameraPixelPerfectZoom : MonoBehaviour
{
    [Header("UI")]
    public Slider zoomSlider;         // 옵션 패널의 슬라이더 (0.5 ~ 2.0 등)
    public Text previewText;          // "x1.0" 같은 미리보기 (선택)
    public Button resetButton;        // 초기화 버튼 (선택)

    [Header("PixelPerfect")]
    public PixelPerfectCamera pixelPerfectCamera; // 연결 안하면 Camera.main에서 자동 탐색

    [Header("설정(조정 가능)")]
    public float minZoom = 0.5f;
    public float maxZoom = 2.0f;
    public float defaultZoom = 1.0f;

    int basePPU;

    void Start()
    {
        // 1) CameraZoomManager 존재 보장
        CameraZoomManager.EnsureExists();

        // 2) PixelPerfectCamera 자동 탐색 (없으면 에러 로그)
        if (pixelPerfectCamera == null && Camera.main != null)
            pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();

        if (pixelPerfectCamera == null)
        {
            Debug.LogError("[CameraPixelPerfectZoom] ❌ PixelPerfectCamera를 찾을 수 없습니다. Main Camera에 컴포넌트가 있는지 확인하세요.");
            return;
        }

        // 3) basePPU 초기 저장 (타이틀씬의 원본 값)
        basePPU = pixelPerfectCamera.assetsPPU;
        CameraZoomManager.Instance.SetBasePPU(basePPU);

        // 4) 슬라이더 초기화: 이전에 저장된 값이 있다면 불러와서 적용
        if (zoomSlider != null)
        {
            zoomSlider.minValue = minZoom;
            zoomSlider.maxValue = maxZoom;
            float saved = CameraZoomManager.Instance.GetZoom();
            // saved가 0일 가능성(초기화) -> defaultZoom 사용
            if (saved <= 0f) saved = defaultZoom;

            // 슬라이더값 세팅(이로 인해 OnValueChanged가 호출될 수 있으니 주의)
            zoomSlider.value = saved;
            zoomSlider.onValueChanged.AddListener(OnZoomChanged);
        }

        // 5) reset 버튼 연결
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);

        // 6) 미리보기 텍스트 업데이트 (현재값 반영)
        UpdatePreviewText(CameraZoomManager.Instance.GetZoom());
    }

    void OnZoomChanged(float value)
    {
        if (pixelPerfectCamera == null) return;

        // 저장 & 적용
        CameraZoomManager.Instance.SetZoom(value);

        int newPPU = Mathf.RoundToInt(basePPU * value);
        pixelPerfectCamera.assetsPPU = Mathf.Max(1, newPPU);

        UpdatePreviewText(value);
        Debug.Log($"[CameraPixelPerfectZoom] 슬라이더 변경 -> Zoom:{value:F2}, PPU:{pixelPerfectCamera.assetsPPU}");
    }

    public void OnResetClicked()
    {
        // 매니저 리셋, 슬라이더 및 카메라 갱신
        CameraZoomManager.Instance.ResetZoomToDefault();
        float z = CameraZoomManager.Instance.GetZoom();

        if (zoomSlider != null)
            zoomSlider.value = z; // 이것이 OnZoomChanged를 유발 -> 자동으로 PPU 적용

        // 만약 슬라이더가 null이면 직접 적용
        if (zoomSlider == null && pixelPerfectCamera != null)
        {
            pixelPerfectCamera.assetsPPU = basePPU;
            UpdatePreviewText(1f);
        }

        Debug.Log("[CameraPixelPerfectZoom] 초기화 버튼 클릭 - 줌을 기본으로 되돌림");
    }

    void UpdatePreviewText(float zoom)
    {
        if (previewText != null)
        {
            // 🔹표시용 배율 계산 (기존값보다 0.5 작게)
            float displayZoom = zoom - 0.5f + 1.0f;  // = zoom + 0.5f

            previewText.text = $"x{displayZoom:F2}";
        }
    }

    private void OnDestroy()
    {
        if (zoomSlider != null)
            zoomSlider.onValueChanged.RemoveListener(OnZoomChanged);
        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetClicked);
    }
}
