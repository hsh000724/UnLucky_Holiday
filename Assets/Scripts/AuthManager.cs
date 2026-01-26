using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google;
using System;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    // 데이터 변경 시 UI에 알림을 보내기 위한 이벤트
    public event Action OnUserDataChanged;

    public bool IsInitialized { get; private set; } = false;
    private readonly string webClientId = "853773184056-rbt60sjs34um28k79sdpi5cr4djupa99.apps.googleusercontent.com";

    public string UserNickname { get; private set; }
    public bool IsLoggedIn => auth != null && auth.CurrentUser != null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else { Destroy(gameObject); }
    }

    private async void InitializeFirebase()
    {
        Debug.Log("Firebase 의존성 체크 시작...");
        var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == Firebase.DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            IsInitialized = true;
            Debug.Log("<color=green>Firebase 초기화 완료!</color>");
        }
        else
        {
            Debug.LogError($"Firebase 초기화 실패: {dependencyStatus}");
        }
    }

    public async Task<bool> SignInWithGoogle()
    {
        if (!IsInitialized)
        {
            Debug.LogError("Firebase가 아직 초기화되지 않았습니다.");
            return false;
        }

        // 1. 구글 설정 (매번 생성하지 않고 안전하게 체크)
        if (GoogleSignIn.Configuration == null)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestIdToken = true,
                UseGameSignIn = false
            };
        }

        try
        {
            // 2. 강제 종료 방지를 위해 이전 세션을 정리할 때 예외 처리를 더 꼼꼼히 합니다.
            try { GoogleSignIn.DefaultInstance.SignOut(); } catch { }

            Debug.Log("구글 로그인창 띄우는 중...");

            // 3. 실제 로그인 호출
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

            if (googleUser == null)
            {
                Debug.LogWarning("구글 로그인 취소됨 (사용자가 창을 닫음)");
                return false;
            }

            Debug.Log($"구글 인증 성공: {googleUser.DisplayName}");

            // 4. Firebase 인증 연결
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            var authResult = await auth.SignInWithCredentialAsync(credential);

            if (authResult != null)
            {
                await GetSavedNickname();
                OnUserDataChanged?.Invoke();
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            // 여기서 앱이 죽지 않도록 모든 예외를 잡습니다.
            Debug.LogError($"구글 로그인 프로세스 중 치명적 에러: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    public async Task<bool> SignInAnonymously()
    {
        if (!IsInitialized) return false;
        try
        {
            var result = await auth.SignInAnonymouslyAsync();
            if (result != null)
            {
                OnUserDataChanged?.Invoke();
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    public async Task<string> GetSavedNickname()
    {
        if (!IsLoggedIn) return null;

        var snapshot = await db.Collection("users").Document(auth.CurrentUser.UserId).GetSnapshotAsync();
        if (snapshot.Exists && snapshot.ContainsField("nickname"))
        {
            UserNickname = snapshot.GetValue<string>("nickname");
            return UserNickname;
        }
        return null;
    }

    public async Task<string> SetNickname(string nickname)
    {
        DocumentReference nickRef = db.Collection("nicknames").Document(nickname);
        DocumentReference userRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        string result = await db.RunTransactionAsync(async transaction =>
        {
            DocumentSnapshot nickSnap = await transaction.GetSnapshotAsync(nickRef);
            if (nickSnap.Exists) return "Exists";

            transaction.Set(nickRef, new Dictionary<string, object> { { "uid", auth.CurrentUser.UserId } });
            transaction.Set(userRef, new Dictionary<string, object> { { "nickname", nickname } }, SetOptions.MergeAll);

            return "Success";
        });

        if (result == "Success")
        {
            UserNickname = nickname; // 로컬 캐시 갱신
            OnUserDataChanged?.Invoke(); // UI 갱신 유도
        }
        return result;
    }

    public void SignOut()
    {
        // 1. Firebase 로그아웃
        auth.SignOut();

        // 2. [추가] 구글 로그인 자체도 로그아웃 시켜야 다음 로그인 시 계정 선택창이 뜹니다.
        try
        {
            GoogleSignIn.DefaultInstance.SignOut();
        }
        catch { /* 구글 로그인이 아닐 경우를 대비해 예외 처리 */ }

        // 3. 변수 초기화 및 알림
        UserNickname = null;
        OnUserDataChanged?.Invoke();
        Debug.Log("모든 인증 세션 종료 및 로그아웃 완료");
    }
}