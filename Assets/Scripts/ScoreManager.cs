using System;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    /// <summary>
    /// 현재 플레이한 게임 모드에 따라 최고 점수를 Firestore에 저장합니다.
    /// </summary>
    public void SaveBestScore(string userId, string nickname, float survivalTime, int stageReached, bool isCleared, ModeManager.GameMode mode)
    {
        string collectionName = $"Scores_{mode}";

        Debug.Log($"[SaveBestScore] Mode={mode}, userId={userId}, stage={stageReached}, time={survivalTime}, cleared={isCleared}");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("⚠️ 유저 ID가 없습니다. 점수 저장 실패.");
            return;
        }

        DocumentReference docRef = db.Collection(collectionName).Document(userId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError($"❌ Firestore 스냅샷 불러오기 실패: {task.Exception}");
                return;
            }

            DocumentSnapshot snapshot = task.Result;

            bool currentIsCleared = false;
            int currentStage = 0;
            float currentSurvivalTime = 0f;

            // 기존 기록 불러오기
            if (snapshot.Exists && snapshot.TryGetValue("bestScore", out object bestScoreObj)
                && bestScoreObj is Dictionary<string, object> bestScore)
            {
                if (bestScore.ContainsKey("isCleared"))
                    currentIsCleared = (bool)bestScore["isCleared"];

                if (bestScore.ContainsKey("stageReached"))
                    currentStage = ConvertToInt(bestScore["stageReached"]);

                if (bestScore.ContainsKey("survivalTime"))
                    currentSurvivalTime = ConvertToFloat(bestScore["survivalTime"]);
            }

            bool isNewBest = false;

            // 🆕 기록이 없는 경우 무조건 저장
            if (!snapshot.Exists)
            {
                isNewBest = true;
                Debug.Log("🆕 신규 유저 기록 첫 저장");
            }
            else
            {
                // ✅ 1단계: 클리어 여부 비교
                if (isCleared && !currentIsCleared)
                {
                    isNewBest = true;
                }
                else if (!isCleared && currentIsCleared)
                {
                    isNewBest = false;
                }
                else
                {
                    // ✅ 2단계: 둘 다 클리어한 경우
                    if (isCleared)
                    {
                        // 스테이지 낮을수록 우선
                        if (stageReached < currentStage)
                            isNewBest = true;

                        // 스테이지 같으면 플레이타임 짧을수록 우선
                        else if (stageReached == currentStage &&
                                 survivalTime < currentSurvivalTime)
                            isNewBest = true;
                    }
                    // ✅ 3단계: 둘 다 클리어 못한 경우
                    else
                    {
                        // 스테이지 높을수록 우선
                        if (stageReached > currentStage)
                            isNewBest = true;

                        // 스테이지 같으면 플레이타임 짧을수록 우선
                        else if (stageReached == currentStage &&
                                 survivalTime < currentSurvivalTime)
                            isNewBest = true;
                    }
                }
            }

            if (isNewBest)
            {
                Dictionary<string, object> bestScoreData = new Dictionary<string, object>
                {
                    { "survivalTime", survivalTime },
                    { "stageReached", stageReached },
                    { "isCleared", isCleared },
                    { "updatedAt", Timestamp.GetCurrentTimestamp() }
                };

                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    { "nickname", nickname },
                    { "bestScore", bestScoreData }
                };

                docRef.SetAsync(data, SetOptions.MergeAll)
                    .ContinueWithOnMainThread(setTask =>
                    {
                        if (setTask.IsCompletedSuccessfully)
                        {
                            Debug.Log($"✅ 최고 기록 갱신 완료! [{mode}] Stage:{stageReached}, Time:{survivalTime:F1}s, Cleared:{isCleared}");
                        }
                        else if (setTask.IsFaulted)
                        {
                            Debug.LogError($"❌ Firestore 저장 실패: {setTask.Exception}");
                        }
                    });
            }
            else
            {
                Debug.Log($"ℹ️ 기존 기록이 더 우수하여 갱신하지 않음. (기존 Stage:{currentStage}, Time:{currentSurvivalTime:F1}s)");
            }
        });
    }

    private int ConvertToInt(object value)
    {
        if (value is long l) return (int)l;
        if (value is int i) return i;
        return 0;
    }

    private float ConvertToFloat(object value)
    {
        if (value is double d) return (float)d;
        if (value is float f) return f;
        if (value is long l) return (float)l;
        return 0f;
    }
}
