using System.Collections;
using System.Collections.Generic;   // <- Dictionary 등을 위해 필요
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class FirestoreManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Awake()
    {
        // Firebase 의존성 확인 후 초기화
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("FirestoreManager: Firebase 초기화 완료");
            }
            else
            {
                Debug.LogError("FirestoreManager: Firebase 의존성 문제 - " + status);
            }
        });
    }

    // 닉네임 사용 가능 여부 확인 (nicknames 컬렉션에 닉네임을 문서 ID로 예약)
    public async Task<bool> IsNicknameAvailable(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname)) return false;

        try
        {
            DocumentReference docRef = db.Collection("nicknames").Document(nickname);
            DocumentSnapshot snap = await docRef.GetSnapshotAsync();
            return !snap.Exists;
        }
        catch (System.Exception e)
        {
            Debug.LogError("IsNicknameAvailable 실패: " + e);
            return false;
        }
    }

    // 닉네임 저장 (중복 체크는 호출자에서 했다고 가정)
    // 닉네임을 nicknames 컬렉션에 등록하고, 가능하면 users 컬렉션(uid)에 nickname 저장
    public async Task<bool> SaveNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname)) return false;

        try
        {
            DocumentReference nickRef = db.Collection("nicknames").Document(nickname);
            DocumentSnapshot existing = await nickRef.GetSnapshotAsync();
            if (existing.Exists)
            {
                Debug.LogWarning("SaveNickname: 이미 존재하는 닉네임");
                return false;
            }

            string uid = auth?.CurrentUser?.UserId;

            var nickData = new Dictionary<string, object>
            {
                { "uid", uid ?? "" },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            await nickRef.SetAsync(nickData);

            if (!string.IsNullOrEmpty(uid))
            {
                // uid 기반 사용자 문서에도 닉네임 저장 (users 컬렉션)
                DocumentReference userRef = db.Collection("users").Document(uid);
                var userData = new Dictionary<string, object>
                {
                    { "nickname", nickname },
                    { "updatedAt", Timestamp.GetCurrentTimestamp() }
                };
                await userRef.SetAsync(userData, SetOptions.MergeAll);
            }

            PlayerPrefs.SetString("nickname", nickname); // 로컬에 저장
            Debug.Log("SaveNickname: 닉네임 저장 완료 - " + nickname);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveNickname 실패: " + e);
            return false;
        }
    }

    // 게임 결과 저장: UID가 있으면 Scores 컬렉션의 문서 ID를 uid로 사용 (최고점수 기준)
    // 또한 users/{uid}/games 에도 기록(옵션)
    public async Task SaveGameResult(float survivalTime, int stageCount, bool isCleared)
    {
        try
        {
            string uid = auth?.CurrentUser?.UserId;
            string nickname = PlayerPrefs.GetString("nickname", "");

            if (!string.IsNullOrEmpty(uid))
            {
                DocumentReference scoreRef = db.Collection("Scores").Document(uid);

                // 기존 최고 생존시간 가져오기 (간단한 방식: snapshot 후 비교)
                DocumentSnapshot snap = await scoreRef.GetSnapshotAsync();
                double currentBest = 0;
                if (snap.Exists && snap.ContainsField("bestScore.survivalTime"))
                {
                    // Firestore numeric 타입은 double로 읽는게 안전
                    currentBest = snap.GetValue<double>("bestScore.survivalTime");
                }

                if (survivalTime > currentBest)
                {
                    var bestScore = new Dictionary<string, object>
                    {
                        { "survivalTime", survivalTime },
                        { "stageReached", stageCount },
                        { "isCleared", isCleared },
                        { "updatedAt", Timestamp.GetCurrentTimestamp() }
                    };

                    var data = new Dictionary<string, object>
                    {
                        { "nickname", nickname },
                        { "bestScore", bestScore }
                    };

                    await scoreRef.SetAsync(data, SetOptions.MergeAll);
                    Debug.Log("SaveGameResult: 최고 점수 업데이트 완료 (uid 기반)");
                }
                else
                {
                    Debug.Log("SaveGameResult: 기존 최고가 더 높음 -> 업데이트 없음");
                }

                // 플레이 히스토리도 남기기 (선택사항)
                DocumentReference historyRef = db.Collection("users").Document(uid).Collection("games").Document();
                var hist = new Dictionary<string, object>
                {
                    { "gameTime", survivalTime },
                    { "stageCount", stageCount },
                    { "isCleared", isCleared },
                    { "timestamp", Timestamp.GetCurrentTimestamp() }
                };
                await historyRef.SetAsync(hist);
            }
            else if (!string.IsNullOrEmpty(nickname))
            {
                // 로그인이 되어있지 않은 경우(게스트로 nickname만 있는 상황) - fallback 저장
                DocumentReference fallbackRef = db.Collection("users_by_nickname").Document(nickname).Collection("games").Document();
                var hist = new Dictionary<string, object>
                {
                    { "gameTime", survivalTime },
                    { "stageCount", stageCount },
                    { "isCleared", isCleared },
                    { "timestamp", Timestamp.GetCurrentTimestamp() }
                };
                await fallbackRef.SetAsync(hist);
                Debug.Log("SaveGameResult: 기록 저장 완료 (nickname 기반 fallback)");
            }
            else
            {
                Debug.LogWarning("SaveGameResult: uid와 nickname 모두 없음 - 저장 불가");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveGameResult 실패: " + e);
        }
    }
}
