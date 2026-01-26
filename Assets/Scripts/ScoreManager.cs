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
    /// <param name="mode">현재 플레이한 게임 모드 (Classic, Infinity, Hardcore)</param>
    public void SaveBestScore(string userId, string nickname, float survivalTime, int stageReached, bool isCleared, ModeManager.GameMode mode)
    {
        // 🚩 1. 모드에 따라 컬렉션 이름 결정
        // 예: Classic -> Scores_Classic, Infinity -> Scores_Infinity
        string collectionName = $"Scores_{mode}";

        Debug.Log($"[SaveBestScore 호출됨] Mode={mode}, Collection={collectionName}, userId={userId}, stage={stageReached}, time={survivalTime}, cleared={isCleared}");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("⚠️ 유저 ID가 없습니다. 점수 저장 실패.");
            return;
        }

        DocumentReference docRef = db.Collection(collectionName).Document(userId); // 👈 컬렉션 이름 동적 적용

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;

                // 기존 기록 불러오기 변수 (모드별로 분리됨)
                bool currentIsCleared = false;
                int currentStage = 0;
                float currentSurvivalTime = 0;

                // 기존 기록이 있는 경우 값 가져오기
                if (snapshot.Exists && snapshot.ContainsField("bestScore"))
                {
                    // ⚠️ GetValue<Dictionary<string, object>>는 Firebase SDK 버전에 따라 오류를 유발할 수 있습니다.
                    // ToDictionary()를 사용하거나, GetValue<object>() 후 Dictionary로 캐스팅하는 것이 더 안정적일 수 있습니다.
                    // 현재 코드 스타일을 유지하되, 오류 발생 시 캐스팅 부분을 확인해야 합니다.
                    if (snapshot.TryGetValue("bestScore", out object bestScoreObj) && bestScoreObj is Dictionary<string, object> bestScore)
                    {
                        if (bestScore.ContainsKey("isCleared"))
                            currentIsCleared = (bool)bestScore["isCleared"];
                        if (bestScore.ContainsKey("stageReached"))
                            currentStage = ConvertToInt(bestScore["stageReached"]);
                        if (bestScore.ContainsKey("survivalTime"))
                            currentSurvivalTime = ConvertToFloat(bestScore["survivalTime"]);
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ 기존 문서에 'bestScore' 필드가 딕셔너리가 아닙니다. 무시하고 새 기록 저장을 시도합니다.");
                    }
                }

                bool isNewBest = false;

                // ✅ 최고 점수 비교 로직
                // 1. 신규 기록이 아예 없는 경우: 무조건 저장
                if (!snapshot.Exists)
                {
                    isNewBest = true;
                    Debug.Log("🆕 신규 기록이 없어서 첫 저장을 진행합니다.");
                }
                else
                {
                    // 2. 기존 기록과 비교 (클리어 여부 > 스테이지 > 시간 순)
                    if (isCleared && !currentIsCleared)
                    {
                        // 새 기록이 클리어이고 기존 기록이 클리어가 아닐 경우: 최고 기록 갱신
                        isNewBest = true;
                    }
                    else if (isCleared == currentIsCleared)
                    {
                        // 클리어 여부가 같을 경우 (둘 다 클리어했거나, 둘 다 클리어 못 했을 경우)
                        if (stageReached > currentStage)
                            isNewBest = true; // 도달 스테이지가 더 높음
                        else if (stageReached == currentStage && survivalTime > currentSurvivalTime)
                            isNewBest = true; // 스테이지는 같지만 생존 시간이 더 길음
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

                    // 🚩 docRef는 이미 모드별 컬렉션을 가리키고 있습니다.
                    docRef.SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(setTask => // SetOptions.Merge를 사용하여 닉네임만 업데이트될 수도 있도록 설정
                    {
                        if (setTask.IsCompletedSuccessfully)
                        {
                            Debug.Log($"✅ Firestore 저장 성공! [{mode}] 유저ID: {userId}, Stage: {stageReached}, 생존시간: {survivalTime}, 클리어 여부: {isCleared}");
                        }
                        else if (setTask.IsFaulted)
                        {
                            Debug.LogError($"❌ Firestore 저장 실패: {setTask.Exception}");
                        }
                        else if (setTask.IsCanceled)
                        {
                            Debug.LogWarning("⚠️ Firestore 저장이 취소됨.");
                        }
                    });
                }
                else
                {
                    Debug.Log($"ℹ️ [{mode}] 기존 최고 점수가 더 높아서 업데이트 안 함. (기존: Stage {currentStage}, Time {currentSurvivalTime:F1}s, 신규: Stage {stageReached}, Time {survivalTime:F1}s)");
                }
            }
            else
            {
                Debug.LogError($"❌ Firestore 스냅샷 불러오기 실패: {task.Exception}");
            }
        });
    }

    private int ConvertToInt(object value)
    {
        // Firestore Long 타입을 int로 변환
        if (value is long) return (int)(long)value;
        if (value is int) return (int)value;
        return 0;
    }

    private float ConvertToFloat(object value)
    {
        // Firestore Double 타입을 float로 변환
        if (value is double) return (float)(double)value;
        if (value is float) return (float)value;
        if (value is long) return (float)(long)value;
        return 0f;
    }
}