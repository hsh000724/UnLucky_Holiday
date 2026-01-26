using UnityEngine;
using UnityEngine.UI;

public class CameraZoomOption : MonoBehaviour
{
    [Header("UI")]
    public Slider zoomSlider;
    public Text zoomPreviewText;
    public Button resetButton;

    [Header("Zoom 설정")]
    public float minZoom = 0.5f;
    public float maxZoom = 2.0f;

    void Start()
    {
        // 매니저 자동 생성 보장
        if (CameraZoomManager.Instance == null)
        {
            new GameObject("CameraZoomManager").AddComponent<CameraZoomManager>();
        }

        zoomSlider.minValue = minZoom;
        zoomSlider.maxValue = maxZoom;

        // 저장된 값 불러오기 (없으면 기본 1)
        float currentZoom = CameraZoomManager.Instance.GetZoom();
        zoomSlider.value = currentZoom;

        UpdatePreviewText(currentZoom);

        zoomSlider.onValueChanged.AddListener(OnZoomChanged);
        resetButton.onClick.AddListener(OnResetClicked);
    }

    void OnZoomChanged(float value)
    {
        CameraZoomManager.Instance.SetZoom(value);
        UpdatePreviewText(value);
    }

    void OnResetClicked()
    {
        CameraZoomManager.Instance.ResetZoom();
        zoomSlider.value = CameraZoomManager.Instance.GetZoom();
        UpdatePreviewText(1f);
    }

    void UpdatePreviewText(float value)
    {
        if (zoomPreviewText != null)
            zoomPreviewText.text = $"x{value:F2}";
    }

    private void OnDestroy()
    {
        zoomSlider.onValueChanged.RemoveListener(OnZoomChanged);
        resetButton.onClick.RemoveListener(OnResetClicked);
    }
}
