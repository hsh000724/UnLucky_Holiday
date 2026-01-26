using UnityEngine;

public class GameSpeedManager : MonoBehaviour
{
    public static GameSpeedManager Instance { get; private set; }

    private readonly float[] allowedSpeeds = { 0.5f, 1f, 1.5f };
    private int currentIndex = 1; // 기본값: 1배속
    private const string SpeedPrefKey = "GameSpeedIndex";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSavedSpeed();
    }

    private void Start()
    {
        ApplyCurrentSpeed();
    }

    /// <summary>
    /// 저장된 배속 인덱스를 불러옵니다.
    /// </summary>
    private void LoadSavedSpeed()
    {
        if (PlayerPrefs.HasKey(SpeedPrefKey))
        {
            currentIndex = PlayerPrefs.GetInt(SpeedPrefKey, 1);
            currentIndex = Mathf.Clamp(currentIndex, 0, allowedSpeeds.Length - 1);
            Debug.Log($"[GameSpeedManager] Loaded saved speed index: {currentIndex} (x{allowedSpeeds[currentIndex]})");
        }
        else
        {
            currentIndex = 1; // 기본값
            Debug.Log("[GameSpeedManager] No saved speed found, defaulting to x1.0");
        }
    }

    /// <summary>
    /// 현재 배속을 저장합니다.
    /// </summary>
    private void SaveSpeed()
    {
        PlayerPrefs.SetInt(SpeedPrefKey, currentIndex);
        PlayerPrefs.Save();
        Debug.Log($"[GameSpeedManager] Saved speed index: {currentIndex}");
    }

    /// <summary>
    /// 외부(UI)에서 배속 인덱스 설정
    /// </summary>
    public void SetSpeedByIndex(int index)
    {
        if (index < 0 || index >= allowedSpeeds.Length)
            return;

        currentIndex = index;
        SaveSpeed();
        ApplyCurrentSpeed();
    }

    public float GetCurrentSpeed() => allowedSpeeds[currentIndex];
    public int GetCurrentIndex() => currentIndex;

    public void ResetToDefaultSpeed()
    {
        currentIndex = 1;
        SaveSpeed();
        ApplyCurrentSpeed();
    }

    public void ApplyCurrentSpeed()
    {
        Time.timeScale = allowedSpeeds[currentIndex];
        Debug.Log($"[GameSpeedManager] Time scale applied: x{allowedSpeeds[currentIndex]}");
    }
}
