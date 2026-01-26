using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [System.Serializable]
    public class CollectibleIconData
    {
        public CollectibleType type;
        public Image iconImage; // UI Image 오브젝트
    }

    public List<CollectibleIconData> iconDataList;

    private Dictionary<CollectibleType, Image> iconDict;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        // Dictionary 초기화
        iconDict = new Dictionary<CollectibleType, Image>();
        foreach (var data in iconDataList)
        {
            iconDict[data.type] = data.iconImage;
        }
    }

    // ✅ 게임 시작 시 모든 아이콘을 검정색 실루엣으로 처리
    public void ResetCollectionUI()
    {
        if (ModeManager.instance.currentMode != ModeManager.GameMode.Infinity)
        {
            foreach (var icon in iconDict.Values)
            {
                icon.color = Color.black; // 완전 검정색 실루엣
            }
        }

    }

    // ✅ 아이템 수집 시 해당 아이콘 원래 색으로 복원
    public void UpdateCollectionUI(CollectibleType type)
    {
        if (iconDict.TryGetValue(type, out Image icon))
        {
            icon.color = Color.white; // 원래 색상으로 보이게
        }
    }
}
