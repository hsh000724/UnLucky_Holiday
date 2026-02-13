using UnityEngine;
using System;

public class ItemRechargeManager : MonoBehaviour
{
    public static ItemRechargeManager instance;

    [Header("충전 설정")]
    public int maxItemCount = 5;
    public int rechargeTimeSeconds = 3600; //60분

    public int currentItemCount { get; private set; }
    public float timeRemaining { get; private set; }

    // 데이터가 변경될 때 UI에 알려주기 위한 이벤트 (Action)
    public Action OnDataChanged;

    private const string ITEM_KEY = "StoredItemCount";
    private const string TIME_KEY = "LastSaveTime";
    private const string TIME_REMAIN_KEY = "StoredTimeRemaining";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 모든 씬에서 유지
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (currentItemCount < maxItemCount)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                RechargeItem();
            }
            // 시간이 흐를 때마다 UI 갱신 신호를 보냄
            OnDataChanged?.Invoke();
        }
    }

    private void RechargeItem()
    {
        currentItemCount++;
        timeRemaining = (currentItemCount < maxItemCount) ? rechargeTimeSeconds : 0;
        SaveData();
        OnDataChanged?.Invoke();
    }

    public bool TryUseItem()
    {
        if (currentItemCount > 0)
        {
            currentItemCount--;
            if (currentItemCount == maxItemCount - 1) timeRemaining = rechargeTimeSeconds;
            SaveData();
            OnDataChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void AddRewardItem()
    {
        if (currentItemCount < maxItemCount)
        {
            currentItemCount++;
            SaveData();
            OnDataChanged?.Invoke();
        }
    }

    // --- 데이터 저장 및 로드 ---
    private void SaveData()
    {
        PlayerPrefs.SetInt(ITEM_KEY, currentItemCount);
        PlayerPrefs.SetString(TIME_KEY, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.SetFloat(TIME_REMAIN_KEY, timeRemaining);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        currentItemCount = PlayerPrefs.GetInt(ITEM_KEY, maxItemCount);
        string lastTimeStr = PlayerPrefs.GetString(TIME_KEY, string.Empty);
        timeRemaining = PlayerPrefs.GetFloat(TIME_REMAIN_KEY, rechargeTimeSeconds);

        if (!string.IsNullOrEmpty(lastTimeStr) && currentItemCount < maxItemCount)
        {
            DateTime lastTime = DateTime.FromBinary(Convert.ToInt64(lastTimeStr));
            TimeSpan elapsed = DateTime.UtcNow - lastTime;

            float totalElapsedSeconds = (float)elapsed.TotalSeconds;

            // 남은 시간에서 먼저 차감
            timeRemaining -= totalElapsedSeconds;

            while (timeRemaining <= 0 && currentItemCount < maxItemCount)
            {
                currentItemCount++;
                timeRemaining += rechargeTimeSeconds;
            }

            if (currentItemCount >= maxItemCount)
            {
                currentItemCount = maxItemCount;
                timeRemaining = 0;
            }
        }
        else
        {
            timeRemaining = (currentItemCount < maxItemCount) ? rechargeTimeSeconds : 0;
        }
    }


    private void OnApplicationPause(bool pause) { if (pause) SaveData(); }
    private void OnApplicationQuit() { SaveData(); }
}