using UnityEngine;
using UnityEngine.UI;   // 레거시 UI
using Firebase;
using Firebase.Auth;

public class GuestLoginManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;

    public Text statusText;   // 레거시 Text 컴포넌트

    void Start()
    {
        // Firebase 초기화
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
        });
    }

    public void OnGuestLogin()
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("게스트 로그인 실패: " + task.Exception);
                if (statusText != null) statusText.text = "로그인 실패!";
                return;
            }

            user = task.Result.User;
            Debug.Log("게스트 로그인 성공! UserID: " + user.UserId);
            if (statusText != null) statusText.text = "게스트 로그인 성공!";
        });
    }
}
