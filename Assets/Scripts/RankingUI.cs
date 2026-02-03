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

        // 텍스트 초기화
        for (int i = 0; i < 3; i++) topTexts[i].text = $"{i + 1}등: 데이터 로딩 중...";
        if (middleTexts != null)
            for (int i = 0; i < middleTexts.Length; i++) middleTexts[i].text = $"{i + 4}등: -";

        // 서버에서 데이터를 가져옴 (복합 조건 정렬을 위해 클라이언트에서 정렬 수행)
        db.Collection(collectionName)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("랭킹 데이터를 가져오지 못했습니다.");
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                List<DocumentSnapshot> docList = new List<DocumentSnapshot>(snapshot.Documents);

                // --- 커스텀 정렬 로직 시작 ---
                docList.Sort((a, b) =>
                {
                    var dataA = a.ToDictionary();
                    var dataB = b.ToDictionary();

                    if (!dataA.ContainsKey("bestScore") || !dataB.ContainsKey("bestScore")) return 0;

                    var scoreA = dataA["bestScore"] as Dictionary<string, object>;
                    var scoreB = dataB["bestScore"] as Dictionary<string, object>;

                    bool clearedA = scoreA.ContainsKey("isCleared") ? (bool)scoreA["isCleared"] : false;
                    bool clearedB = scoreB.ContainsKey("isCleared") ? (bool)scoreB["isCleared"] : false;

                    int stageA = ConvertToInt(scoreA["stageReached"]);
                    int stageB = ConvertToInt(scoreB["stageReached"]);

                    float timeA = ConvertToFloat(scoreA["survivalTime"]);
                    float timeB = ConvertToFloat(scoreB["survivalTime"]);

                    // 1순위: 클리어 여부 (클리어한 사람이 상위)
                    if (clearedA != clearedB)
                        return clearedB.CompareTo(clearedA);

                    if (clearedA) // 둘 다 클리어한 경우
                    {
                        // 2순위: 도달 스테이지가 낮을수록 상위 (보스까지 빨리 도달)
                        if (stageA != stageB) return stageA.CompareTo(stageB);
                        // 3순위: 플레이 타임이 짧을수록 상위
                        return timeA.CompareTo(timeB);
                    }
                    else // 둘 다 클리어 못한 경우
                    {
                        // 2순위: 도달 스테이지가 높을수록 상위
                        if (stageA != stageB) return stageB.CompareTo(stageA);
                        // 3순위: 플레이 타임이 길수록 상위 (더 오래 생존)
                        return timeB.CompareTo(timeA);
                    }
                });
                // --- 커스텀 정렬 로직 끝 ---

                int rank = 1;
                bool foundMyRecord = false;
                string currentNickname = AuthManager.Instance.UserNickname;

                // UI 텍스트 다시 초기화
                for (int i = 0; i < 3; i++) topTexts[i].text = $"{i + 1}등: -";
                if (middleTexts != null)
                    for (int i = 0; i < middleTexts.Length; i++) middleTexts[i].text = $"{i + 4}등: -";
                if (myRecord != null) myRecord.text = $"내 기록: {currentNickname} - 기록 없음";

                foreach (DocumentSnapshot doc in docList)
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

                    string recordDisplay = $"Stage {stage}, {time:F1}s ({(cleared ? "클리어" : "도전 중")})";

                    // 상위 3등 표시
                    if (rank <= 3)
                    {
                        topTexts[rank - 1].text = $"{rank}등: {nickname}";
                    }
                    // 4등 ~ 10등 표시
                    else if (rank <= 10 && middleTexts != null && middleTexts.Length >= (rank - 3))
                    {
                        middleTexts[rank - 4].text = $"{rank}등: {nickname} - {recordDisplay}";
                    }

                    // 내 기록 확인 (ID 비교)
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