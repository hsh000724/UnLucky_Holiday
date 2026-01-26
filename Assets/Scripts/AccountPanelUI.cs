using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Firebase.Auth;

public class AccountPanelUI : MonoBehaviour
{
    [Header("UI References")]
    public Text nicknameText;
    public Text userIdText;
    public Text loginTypeText;
    public Button changeNicknameButton;

    [Header("Nickname Change Panel")]
    public GameObject nicknameChangePanel;
    public InputField nicknameInputField;
    public Button saveNicknameButton;
    public Button cancelNicknameButton;
    public Text statusMessageText;

    private void OnEnable()
    {
        // 이벤트 구독: AuthManager의 데이터가 바뀌면 UI를 새로고침함
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnUserDataChanged += RefreshAccountInfo;
        }

        InitializePanel();
    }

    private void OnDisable()
    {
        // 구독 해제 (메모리 누수 방지)
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnUserDataChanged -= RefreshAccountInfo;
        }

        changeNicknameButton.onClick.RemoveAllListeners();
        saveNicknameButton.onClick.RemoveAllListeners();
        cancelNicknameButton.onClick.RemoveAllListeners();
    }

    private async void InitializePanel()
    {
        // 초기화 대기
        if (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
        {
            UpdateStatusMessage("정보 동기화 중...");
            while (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
                await Task.Delay(100);
        }

        RefreshAccountInfo();

        // 버튼 리스너 등록
        changeNicknameButton.onClick.AddListener(OnChangeNicknameButtonClicked);
        saveNicknameButton.onClick.AddListener(async () => await OnSaveNicknameButtonClicked());
        cancelNicknameButton.onClick.AddListener(OnCancelNicknameButtonClicked);
    }

    private void RefreshAccountInfo()
    {
        if (AuthManager.Instance == null) return;

        string nickname = AuthManager.Instance.UserNickname ?? "설정 필요";
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        string userId = user?.UserId ?? "Unknown";

        string loginType = "Guest";
        if (user != null)
        {
            foreach (var profile in user.ProviderData)
            {
                if (profile.ProviderId == "google.com") loginType = "Google";
            }
        }

        nicknameText.text = $"닉네임 : {nickname}";
        userIdText.text = $"유저 ID : {userId}";
        loginTypeText.text = $"로그인 타입 : {loginType}";

        UpdateStatusMessage(""); // 메시지 초기화
    }

    public void OnChangeNicknameButtonClicked()
    {
        nicknameInputField.text = AuthManager.Instance.UserNickname;
        nicknameChangePanel.SetActive(true);
        UpdateStatusMessage("새 닉네임을 입력해 주세요.");
    }

    private async Task OnSaveNicknameButtonClicked()
    {
        string newNickname = nicknameInputField.text.Trim();

        if (newNickname == AuthManager.Instance.UserNickname)
        {
            UpdateStatusMessage("현재 닉네임과 동일합니다.");
            return;
        }

        // NicknameValidator 체크 (기존 로직 유지)
        UpdateStatusMessage("닉네임 변경 중...");
        string result = await AuthManager.Instance.SetNickname(newNickname);

        if (result == "Success")
        {
            nicknameChangePanel.SetActive(false);
            // 여기서 RefreshAccountInfo를 직접 부를 필요가 없습니다. 
            // AuthManager에서 이벤트를 쐈기 때문입니다.
            UpdateStatusMessage("닉네임 변경 성공!");
        }
        else if (result == "Exists")
            UpdateStatusMessage("이미 사용 중인 닉네임입니다.");
        else
            UpdateStatusMessage("오류가 발생했습니다.");
    }

    private void OnCancelNicknameButtonClicked() => nicknameChangePanel.SetActive(false);

    private void UpdateStatusMessage(string message)
    {
        if (statusMessageText != null) statusMessageText.text = message;
    }
}