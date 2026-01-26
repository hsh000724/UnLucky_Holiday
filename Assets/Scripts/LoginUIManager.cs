using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class LoginUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject nicknamePanel;
    public GameObject mainLobbyUI; // 로비 씬 대신 보여줄 메인 UI 그룹

    [Header("UI Elements")]
    public InputField nicknameInput;
    public Text errorText;

    private async void Start()
    {
        // 1. 초기 상태 설정
        loginPanel.SetActive(false);
        nicknamePanel.SetActive(false);
        if (mainLobbyUI != null) mainLobbyUI.SetActive(false);

        errorText.text = "서버 연결 중...";

        // 2. AuthManager 초기화 대기
        while (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
        {
            await Task.Delay(100);
        }

        // 3. 자동 로그인 확인
        CheckAutoLogin();
    }

    private async void CheckAutoLogin()
    {
        if (AuthManager.Instance.IsLoggedIn)
        {
            errorText.text = "자동 로그인 중...";
            await ProcessLoginSuccess();
        }
        else
        {
            errorText.text = "";
            ShowLoginPanel();
        }
    }

    private async Task ProcessLoginSuccess()
    {
        // 로그인 성공 후 닉네임 확인
        string nickname = await AuthManager.Instance.GetSavedNickname();

        if (!string.IsNullOrEmpty(nickname))
        {
            // 닉네임이 있으면 모든 로그인 관련 패널을 닫고 메인 UI 활성화
            CloseAllPanels();
            if (mainLobbyUI != null) mainLobbyUI.SetActive(true);
            ShowMessage($"{nickname}님 환영합니다!");
        }
        else
        {
            // 닉네임 없으면 설정 패널 표시
            ShowNicknamePanel();
        }
    }

    // --- UI 조작 메서드 ---

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        nicknamePanel.SetActive(false);
    }

    public void ShowNicknamePanel()
    {
        loginPanel.SetActive(false);
        nicknamePanel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        loginPanel.SetActive(false);
        nicknamePanel.SetActive(false);
    }

    private async void ShowMessage(string message)
    {
        if (errorText == null) return;
        errorText.text = message;
        await Task.Delay(3000);
        if (this != null && errorText != null && errorText.text == message)
            errorText.text = "";
    }

    // --- 버튼 이벤트 ---

    public async void OnGoogleLoginClick()
    {
        ShowMessage("구글 로그인 시도 중...");
        bool success = await AuthManager.Instance.SignInWithGoogle();
        if (success) await ProcessLoginSuccess();
        else ShowMessage("로그인 실패");
    }

    public async void OnGuestLoginClick()
    {
        ShowMessage("게스트 로그인 시도 중...");
        if (await AuthManager.Instance.SignInAnonymously()) await ProcessLoginSuccess();
        else ShowMessage("로그인 실패");
    }

    public async void OnConfirmNicknameClick()
    {
        string input = nicknameInput.text;
        if (string.IsNullOrEmpty(input)) { ShowMessage("닉네임을 입력하세요."); return; }

        string result = await AuthManager.Instance.SetNickname(input);

        if (result == "Success")
        {
            // 닉네임 설정 성공 시 패널 닫고 메인 UI 켜기
            CloseAllPanels();
            if (mainLobbyUI != null) mainLobbyUI.SetActive(true);
            ShowMessage("닉네임 설정 완료!");
        }
        else
        {
            ShowMessage("이미 사용 중인 닉네임입니다.");
        }
    }

    // 로그아웃 버튼에 연결할 함수
    public void OnLogoutButtonClick()
    {
        // 1. Firebase 로그아웃 수행
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.SignOut();
            // AuthManager 내부에 FirebaseAuth.DefaultInstance.SignOut() 로직이 있어야 합니다.
        }

        // 2. UI 초기화
        if (mainLobbyUI != null) mainLobbyUI.SetActive(false); // 메인 화면 끄기

        // 3. 다시 로그인 패널 띄우기
        ShowLoginPanel();

        // 4. 에러/상태 텍스트 초기화 및 안내
        if (errorText != null) errorText.text = "로그아웃 되었습니다.";

        Debug.Log("로그아웃 성공: 로그인 화면으로 돌아갑니다.");
    }
}