using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResolutionSetting : MonoBehaviour
{
    public Dropdown resolutionDropdown;

    private List<Vector2Int> customResolutions = new List<Vector2Int>
    {
        new Vector2Int(1280, 720),   // HD
        new Vector2Int(1600, 900),   // HD+
        new Vector2Int(1920, 1080),  // FHD
        new Vector2Int(2560, 1440)   // QHD
    };

    private int currentResolutionIndex = 0;

    void Start()
    {
        InitializeResolutions();
    }

    void InitializeResolutions()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < customResolutions.Count; i++)
        {
            string option = customResolutions[i].x + " x " + customResolutions[i].y;
            options.Add(option);
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(ApplyResolution);

        // 현재 해상도 인덱스 초기화
        Vector2Int current = new Vector2Int(Screen.width, Screen.height);
        for (int i = 0; i < customResolutions.Count; i++)
        {
            if (customResolutions[i].x == current.x && customResolutions[i].y == current.y)
            {
                currentResolutionIndex = i;
                break;
            }
        }

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void ApplyResolution(int index)
    {
        Vector2Int selectedRes = customResolutions[index];

#if UNITY_EDITOR
        // 에디터에서만 실제 해상도 변경
        Screen.SetResolution(selectedRes.x, selectedRes.y, false);
#else
        // 모바일에서는 실제 해상도 변경 대신 CanvasScaler만 조정하는 게 일반적입니다.
        Debug.Log($"모바일에서는 해상도 변경 불가. 설정값: {selectedRes.x}x{selectedRes.y}");
#endif
    }
}
