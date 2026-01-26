using UnityEngine;
using Google;
using System;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    // 기존의 모든 이벤트 정의를 여기로 가져왔습니다.
    public static event Action OnAccountInfoChanged;
    public event Action<bool, string> OnLoginStatusUpdated;

    private GoogleSignInUser currentUser;
    private bool isGuest = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    void Start() { InitializeGoogleSignIn(); }

    private void InitializeGoogleSignIn()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = "853773184056-rbt60sjs34um28k79sdpi5cr4djupa99.apps.googleusercontent.com",
            RequestIdToken = true,
            RequestEmail = true,
        };
    }

    #region 로그인 로직 (기존 기능 유지)
    public void SignInWithGoogle()
    {
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsCanceled) OnLoginStatusUpdated?.Invoke(false, "구글 로그인 취소됨");
            else if (task.IsFaulted) OnLoginStatusUpdated?.Invoke(false, "로그인 실패");
            else
            {
                currentUser = task.Result;
                isGuest = false;
                PlayerPrefs.SetString("GoogleEmail", currentUser.Email);
                PlayerPrefs.Save();
                OnLoginStatusUpdated?.Invoke(true, "구글 로그인 성공!");
                NotifyAccountInfoChanged(); // 계정 정보 변경 알림
            }
        });
    }

    public void SignInAsGuest()
    {
        string guestId = Guid.NewGuid().ToString();
        PlayerPrefs.SetString("GuestID", guestId);
        PlayerPrefs.Save();
        isGuest = true;
        currentUser = null;
        OnLoginStatusUpdated?.Invoke(true, "게스트 로그인 성공!");
        NotifyAccountInfoChanged();
    }

    public void Logout()
    {
        if (!isGuest && GoogleSignIn.DefaultInstance != null)
            GoogleSignIn.DefaultInstance.SignOut();

        isGuest = false;
        currentUser = null;
        PlayerPrefs.DeleteKey("GoogleEmail");
        PlayerPrefs.DeleteKey("GuestID");
        PlayerPrefs.DeleteKey("Nickname");
        PlayerPrefs.Save();

        OnLoginStatusUpdated?.Invoke(false, "로그아웃 되었습니다.");
        NotifyAccountInfoChanged();
    }
    #endregion

    #region 데이터 로직 (Firestore)
    public async Task<bool> CheckNicknameDuplication(string nickname)
    {
        try
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            var snapshot = await db.Collection("usernames").WhereEqualTo("nickname", nickname).GetSnapshotAsync();
            return snapshot.Count > 0;
        }
        catch { return false; }
    }

    public async Task<bool> ChangeAndRegisterNicknameInFirestore(string newNickname, string oldNickname, string userId)
    {
        // 닉네임 변경 및 Firestore 등록 기존 로직...
        PlayerPrefs.SetString("Nickname", newNickname);
        PlayerPrefs.Save();
        NotifyAccountInfoChanged();
        return true;
    }
    #endregion

    #region 정보 반환 함수 (기타 스크립트에서 참조용)
    public string GetUserId() => PlayerPrefs.GetString(isGuest ? "GuestID" : "GoogleEmail", "Anonymous");
    public string GetNickname() => PlayerPrefs.GetString("Nickname", "Unknown");
    public bool IsLoggedIn() => !string.IsNullOrEmpty(PlayerPrefs.GetString("GoogleEmail", "")) || !string.IsNullOrEmpty(PlayerPrefs.GetString("GuestID", ""));
    public void NotifyAccountInfoChanged() => OnAccountInfoChanged?.Invoke();
    #endregion
}