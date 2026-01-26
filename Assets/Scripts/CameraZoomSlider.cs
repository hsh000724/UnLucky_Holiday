using UnityEngine;
using UnityEngine.UI;

public class CameraZoomSlider : MonoBehaviour
{
    public Slider zoomSlider;
    public Text previewText;

    private void Start()
    {
        if (zoomSlider == null)
        {
            Debug.LogError("[CameraZoomSlider] ❌ 슬라이더가 연결되지 않았습니다!");
            return;
        }

        // 기존 저장된 값으로 초기화
        if (CameraZoomManager.Instance != null)
        {
            float savedZoom = CameraZoomManager.Instance.GetZoom();
            zoomSlider.value = savedZoom;
        }

        zoomSlider.onValueChanged.AddListener(OnZoomChanged);
        OnZoomChanged(zoomSlider.value);
        Debug.Log("[CameraZoomSlider] ✅ 초기화 완료. 현재 슬라이더 값: " + zoomSlider.value);
    }

    private void OnZoomChanged(float value)
    {
        if (CameraZoomManager.Instance != null)
        {
            CameraZoomManager.Instance.SetZoom(value);
            Debug.Log($"[CameraZoomSlider] ✅ CameraZoomManager에 배율 전달: {value}");
        }

        if (previewText != null)
        {
            previewText.text = $"x{value:F1}";
        }
    }
}
