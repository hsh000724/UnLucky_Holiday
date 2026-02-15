using UnityEngine;

public class CameraZoomManager : MonoBehaviour
{
    public static CameraZoomManager Instance;

    private float zoomValue = 1f;
    private int basePPU = 32;

    private const string ZoomPrefKey = "CameraZoomValue";
    private const float DefaultZoom = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSavedZoom();   // 🔥 저장값 불러오기

            Debug.Log("[CameraZoomManager] ✅ 생성 및 유지됨");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🔹 자동 생성용
    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("CameraZoomManager");
            obj.AddComponent<CameraZoomManager>();
            Debug.Log("[CameraZoomManager] ✅ 자동 생성됨 (EnsureExists 호출)");
        }
    }

    // 🔥 줌 설정 (저장 포함)
    public void SetZoom(float value)
    {
        zoomValue = Mathf.Clamp(value, 0.5f, 2f);

        PlayerPrefs.SetFloat(ZoomPrefKey, zoomValue);
        PlayerPrefs.Save();

        Debug.Log($"[CameraZoomManager] 줌 저장됨: {zoomValue}");
    }

    public float GetZoom() => zoomValue;

    public void SetBasePPU(int value)
    {
        basePPU = Mathf.Max(1, value);
    }

    public int GetBasePPU() => basePPU;

    public void ResetZoom()
    {
        zoomValue = DefaultZoom;
        SaveZoom();
        Debug.Log("[CameraZoomManager] 줌값 초기화됨");
    }

    public void ResetZoomToDefault()
    {
        zoomValue = DefaultZoom;
        SaveZoom();
        Debug.Log("[CameraZoomManager] 🔄 줌이 기본값(1.0)으로 초기화됨");
    }

    // 🔥 저장 함수 분리
    private void SaveZoom()
    {
        PlayerPrefs.SetFloat(ZoomPrefKey, zoomValue);
        PlayerPrefs.Save();
    }

    // 🔥 저장값 불러오기
    private void LoadSavedZoom()
    {
        if (PlayerPrefs.HasKey(ZoomPrefKey))
        {
            zoomValue = PlayerPrefs.GetFloat(ZoomPrefKey, DefaultZoom);
            Debug.Log($"[CameraZoomManager] 저장된 줌 불러옴: {zoomValue}");
        }
        else
        {
            zoomValue = DefaultZoom;
            Debug.Log("[CameraZoomManager] 저장된 줌 없음 → 기본값 사용");
        }
    }
}
