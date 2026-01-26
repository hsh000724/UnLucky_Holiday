using UnityEngine;
using UnityEngine.UI;

public class GameSpeedUIController : MonoBehaviour
{
    public Slider speedSlider;
    public Text speedText;

    private readonly float[] allowedSpeeds = { 0.5f, 1f, 1.5f };

    private void Start()
    {
        if (speedSlider == null)
        {
            Debug.LogError("[GameSpeedUIController] SpeedSlider not assigned!");
            return;
        }

        // 슬라이더 기본 설정
        speedSlider.minValue = 0;
        speedSlider.maxValue = allowedSpeeds.Length - 1;
        speedSlider.wholeNumbers = true;

        // 저장된 값 불러오기
        if (GameSpeedManager.Instance != null)
        {
            int currentIndex = GameSpeedManager.Instance.GetCurrentIndex();
            speedSlider.value = currentIndex;
            UpdateSpeedDisplay(currentIndex);
        }

        // 슬라이더 이벤트 연결
        speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
    }

    private void OnSpeedSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);

        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.Instance.SetSpeedByIndex(index);
            UpdateSpeedDisplay(index);
        }
    }

    private void UpdateSpeedDisplay(int index)
    {
        if (speedText != null)
            speedText.text = $"x{allowedSpeeds[index]:0.0}";
    }

    /// <summary>
    /// 리셋 버튼 클릭 시 호출됩니다.
    /// 게임 속도와 UI를 기본값(1배속, 인덱스 1)으로 리셋합니다.
    /// </summary>
    public void ResetSpeedUIAndGame()
    {
        const int defaultIndex = 1; // 기본값 인덱스는 1 (1.0배속)

        // 1. GameSpeedManager를 통해 실제 게임 속도를 리셋합니다.
        if (GameSpeedManager.Instance != null)
        {
            // ResetToDefaultSpeed()는 Time.timeScale을 1.0으로 설정하고 PlayerPrefs에 저장합니다.
            GameSpeedManager.Instance.ResetToDefaultSpeed();
        }

        // 2. UI (슬라이더 및 텍스트)를 기본값으로 업데이트합니다.
        if (speedSlider != null)
        {
            // 이로 인해 OnSpeedSliderChanged가 호출되면서 GameSpeedManager가 중복 호출되는 것을 방지하기 위해 
            // AddListener를 제거하고 다시 추가할 필요 없이, 이미 GameSpeedManager에서 Reset을 했으므로
            // 여기서는 단순히 UI 값만 설정해줍니다.
            speedSlider.value = defaultIndex;
        }

        // 3. 텍스트 디스플레이 업데이트 (슬라이더 값 변경이 텍스트를 업데이트하지만, 명시적으로 호출)
        UpdateSpeedDisplay(defaultIndex);

        Debug.Log("[GameSpeedUIController] Game speed and UI reset to default (x1.0).");
    }
}