using UnityEngine;
using UnityEngine.UI;

public class StageCounter : MonoBehaviour
{
    public Text stageText;        // UI에 표시할 Text 컴포넌트
    public int stageCount;

    void Start()
    {
        stageCount = 1;
        // 매 60초마다 IncreaseStage 메서드를 반복 호출
        InvokeRepeating("IncreaseStage", 60f, 60f);
        UpdateStageUI();  // 초기 스테이지 UI 업데이트
    }
    

    void IncreaseStage()
    {
        stageCount++;     // 스테이지 카운트를 1 증가
        UpdateStageUI();  // UI 업데이트
    }

    void UpdateStageUI()
    {
        // 스테이지 카운트를 UI에 업데이트
        stageText.text = "STAGE " + stageCount;
    }
}
