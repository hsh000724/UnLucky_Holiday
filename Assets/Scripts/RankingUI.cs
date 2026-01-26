using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using Firebase.Auth;
using System.Threading.Tasks;

public class RankingUI : MonoBehaviour
{
    [Header("Mode Panels")]
    public GameObject classicPanel;
    public GameObject infinityPanel;
    public GameObject hardcorePanel;

    [Header("Classic UI References")]
    public Text classic_rank1Text;
    public Text classic_rank2Text;
    public Text classic_rank3Text;
    public Text[] classic_middleRankTexts;
    public Text classic_myRecordText;

    [Header("Infinity UI References")]
    public Text infinity_rank1Text;
    public Text infinity_rank2Text;
    public Text infinity_rank3Text;
    public Text[] infinity_middleRankTexts;
    public Text infinity_myRecordText;

    [Header("Hardcore UI References")]
    public Text hardcore_rank1Text;
    public Text hardcore_rank2Text;
    public Text hardcore_rank3Text;
    public Text[] hardcore_middleRankTexts;
    public Text hardcore_myRecordText;

    private FirebaseFirestore db;
    private ModeManager.GameMode currentDisplayMode = ModeManager.GameMode.Classic;

    void Start()
    {
        CloseAllPanels();
    }

    private void CloseAllPanels()
    {
        if (classicPanel != null) classicPanel.SetActive(false);
        if (infinityPanel != null) infinityPanel.SetActive(false);
        if (hardcorePanel != null) hardcorePanel.SetActive(false);
    }

    public void OnClassicButtonClicked() => ShowRanking(ModeManager.GameMode.Classic);
    public void OnInfinityButtonClicked() => ShowRanking(ModeManager.GameMode.Infinity);
    public void OnHardcoreButtonClicked() => ShowRanking(ModeManager.GameMode.Hardcore);

    public async void ShowRanking(ModeManager.GameMode mode)
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
        {
            Debug.Log("[RankingUI] 초기화 대기 중...");
            while (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
            {
                await Task.Delay(100);
            }
        }

        if (db == null) db = FirebaseFirestore.DefaultInstance;

        currentDisplayMode = mode;
        CloseAllPanels();

        if (mode == ModeManager.GameMode.Classic && classicPanel != null) classicPanel.SetActive(true);
        else if (mode == ModeManager.GameMode.Infinity && infinityPanel != null) infinityPanel.SetActive(true);
        else if (mode == ModeManager.GameMode.Hardcore && hardcorePanel != null) hardcorePanel.SetActive(true);

        string myUserId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(myUserId))
        {
            Debug.LogError("로그인된 유저 정보를 찾을 수 없습니다.");
            return;
        }

        LoadRankingData(myUserId, mode);
    }

    private void LoadRankingData(string myUserId, ModeManager.GameMode mode)
    {
        string collectionName = $"Scores_{mode}";

        Text[] topTexts = new Text[3];
        Text[] middleTexts = null;
        Text myRecord = null;

        if (mode == ModeManager.GameMode.Classic)
        {
            topTexts[0] = classic_rank1Text; topTexts[1] = classic_rank2Text; topTexts[2] = classic_rank3Text;
            middleTexts = classic_middleRankTexts; myRecord = classic_myRecordText;
        }
        else if (mode == ModeManager.GameMode.Infinity)
        {
            topTexts[0] = infinity_rank1Text; topTexts[1] = infinity_rank2Text; topTexts[2] = infinity_rank3Text;
            middleTexts = infinity_middleRankTexts; myRecord = infinity_myRecordText;
        }
        else if (mode == ModeManager.GameMode.Hardcore)
        {
            topTexts[0] = hardcore_rank1Text; topTexts[1] = hardcore_rank2Text; topTexts[2] = hardcore_rank3Text;
            middleTexts = hardcore_middleRankTexts; myRecord = hardcore_myRecordText;
        }

        if (topTexts[0] == null) return;

        db.Collection(collectionName)
            .OrderByDescending("bestScore.stageReached")
            .OrderByDescending("bestScore.survivalTime")
            .Limit(10)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("랭킹 데이터를 가져오지 못했습니다.");
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                int rank = 1;
                bool foundMyRecord = false;
                string currentNickname = AuthManager.Instance.UserNickname;

                for (int i = 0; i < 3; i++) topTexts[i].text = $"{i + 1}등: -";
                if (middleTexts != null)
                    for (int i = 0; i < middleTexts.Length; i++) middleTexts[i].text = $"{i + 4}등: -";
                if (myRecord != null) myRecord.text = $"내 기록: {currentNickname} - 기록 없음";

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    Dictionary<string, object> data = doc.ToDictionary();
                    string nickname = data.ContainsKey("nickname") ? data["nickname"].ToString() : "Unknown";

                    int stage = 0;
                    float time = 0f;
                    bool cleared = false;

                    if (data.ContainsKey("bestScore") && data["bestScore"] is Dictionary<string, object> bestScore)
                    {
                        stage = ConvertToInt(bestScore["stageReached"]);
                        time = ConvertToFloat(bestScore["survivalTime"]);
                        cleared = (bool)bestScore["isCleared"];
                    }

                    // 상세 정보 문자열
                    string recordDisplay = $"Stage {stage}, {time:F1}s ({(cleared ? "클리어" : "도전 중")})";

                    // 순위에 따른 텍스트 설정
                    if (rank <= 3)
                    {
                        // 1,2,3등은 닉네임만 표시
                        topTexts[rank - 1].text = $"{rank}등: {nickname}";
                    }
                    else if (rank <= 10 && middleTexts != null && middleTexts.Length >= (rank - 3))
                    {
                        // 4등부터는 기존처럼 상세 정보 포함
                        middleTexts[rank - 4].text = $"{rank}등: {nickname} - {recordDisplay}";
                    }

                    // 내 기록은 순위와 상관없이 항상 상세 정보 표시
                    if (doc.Id == myUserId && myRecord != null)
                    {
                        myRecord.text = $"내 기록: {nickname} - {recordDisplay} (순위: {rank}등)";
                        foundMyRecord = true;
                    }
                    rank++;
                }

                if (!foundMyRecord && myRecord != null)
                {
                    myRecord.text = $"내 기록: {currentNickname} - 순위 없음 (Top 10 외)";
                }
            });
    }

    public void OnCloseButtonClicked()
    {
        CloseAllPanels();
        Debug.Log("랭킹 패널을 닫았습니다.");
    }

    private int ConvertToInt(object value) => value is long l ? (int)l : (value is int i ? i : 0);
    private float ConvertToFloat(object value) => value is double d ? (float)d : (value is float f ? f : (value is long l ? (float)l : 0f));
}