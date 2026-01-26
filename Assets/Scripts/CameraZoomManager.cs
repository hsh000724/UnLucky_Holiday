using UnityEngine;

public class CameraZoomManager : MonoBehaviour
{
    public static CameraZoomManager Instance;

    private float zoomValue = 1f;
    private int basePPU = 32;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    public void SetZoom(float value)
    {
        zoomValue = Mathf.Clamp(value, 0.5f, 2f);
    }

    public float GetZoom() => zoomValue;

    public void SetBasePPU(int value)
    {
        basePPU = Mathf.Max(1, value);
    }

    public int GetBasePPU() => basePPU;

    public void ResetZoom()
    {
        zoomValue = 1f;
        Debug.Log("[CameraZoomManager] 줌값 초기화됨");
    }

    // 🔹 오류 해결용 추가 메서드
    public void ResetZoomToDefault()
    {
        zoomValue = 1f;
        Debug.Log("[CameraZoomManager] 🔄 줌이 기본값(1.0)으로 초기화됨");
    }
}
