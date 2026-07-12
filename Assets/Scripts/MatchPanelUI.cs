using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchPanelUI : MonoBehaviour
{
    [Header("메인 버튼 그룹")]
    public Button btnRandomMatch;
    public Button btnPlayTogether;
    public Button btnBack;

    [Header("같이하기 서브 버튼 그룹")]
    public GameObject togetherSubGroup;   // 방만들기 + 입장하기 묶음 오브젝트
    public Button btnCreateRoom;
    public Button btnEnterRoom;

    [Header("코드 입력 패널")]
    public GameObject codeInputPanel;
    public TMP_InputField inputRoomCode;
    public Button btnConfirmCode;
    public Button btnCancelCode;

    [Header("피드백 텍스트")]
    public TMP_Text txtFeedback;          // 에러/로딩 메시지 공용

    [Header("로딩 오버레이")]
    public GameObject loadingOverlay;
    public TMP_Text txtLoadingMsg;

    void Start()
    {
        // 메인 버튼
        btnRandomMatch.onClick.AddListener(OnClickRandom);
        btnPlayTogether.onClick.AddListener(OnClickPlayTogether);
        btnBack.onClick.AddListener(OnClickBack);

        // 같이하기 서브
        btnCreateRoom.onClick.AddListener(OnClickCreateRoom);
        btnEnterRoom.onClick.AddListener(OnClickEnterRoom);

        // 코드 입력 패널
        btnConfirmCode.onClick.AddListener(OnClickConfirmCode);
        btnCancelCode.onClick.AddListener(OnClickCancelCode);

        // 이벤트 구독
        MatchmakingManager.Instance.OnMatchmakingFailed += OnFailed;

        // 초기 상태
        ResetToMain();
    }

    void OnDestroy()
    {
        if (MatchmakingManager.Instance != null)
            MatchmakingManager.Instance.OnMatchmakingFailed -= OnFailed;
    }

    // ─────────────────────────────────────
    // 메인 버튼 이벤트
    // ─────────────────────────────────────

    // 1. 랜덤 매칭
    private void OnClickRandom()
    {
        ShowLoading("Random matching...");
        MatchmakingManager.Instance.StartRandomMatching();
    }

    // 2. 같이하기 → 서브 버튼 토글
    private void OnClickPlayTogether()
    {
        bool isActive = togetherSubGroup.activeSelf;

        // 서브 그룹 토글, 코드 패널은 닫기
        togetherSubGroup.SetActive(!isActive);
        codeInputPanel.SetActive(false);
        ClearFeedback();
    }

    // 3. 뒤로가기
    private void OnClickBack()
    {
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────
    // 같이하기 서브 버튼 이벤트
    // ─────────────────────────────────────

    // 방 만들기
    private void OnClickCreateRoom()
    {
        ShowLoading("Creating Secret Room ...");
        MatchmakingManager.Instance.CreateInviteRoom();
    }

    // 입장하기 → 코드 입력 패널 활성화
    private void OnClickEnterRoom()
    {
        codeInputPanel.SetActive(true);
        togetherSubGroup.SetActive(false);
        inputRoomCode.text = string.Empty;
        inputRoomCode.ActivateInputField();
        ClearFeedback();
    }

    // ─────────────────────────────────────
    // 코드 입력 패널 이벤트
    // ─────────────────────────────────────

    // 확인 버튼
    private void OnClickConfirmCode()
    {
        string code = inputRoomCode.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code))
        {
            ShowFeedback("방 코드를 입력해주세요.");
            return;
        }

        if (code.Length != 8)
        {
            ShowFeedback("방 코드는 8자리입니다.");
            return;
        }

        ShowLoading("방 입장 중...");
        MatchmakingManager.Instance.JoinByRoomCode(code);
    }

    // 취소 버튼
    private void OnClickCancelCode()
    {
        codeInputPanel.SetActive(false);
        togetherSubGroup.SetActive(true);
        ClearFeedback();
    }

    // ─────────────────────────────────────
    // 피드백 / 로딩
    // ─────────────────────────────────────

    private void OnFailed(string msg)
    {
        loadingOverlay.SetActive(false);
        ShowFeedback(msg);
    }

    private void ShowFeedback(string msg)
    {
        txtFeedback.text = msg;
        StopCoroutine(nameof(ClearFeedbackDelay));
        StartCoroutine(nameof(ClearFeedbackDelay));
    }

    private IEnumerator ClearFeedbackDelay()
    {
        yield return new WaitForSeconds(3f);
        ClearFeedback();
    }

    private void ClearFeedback()
    {
        txtFeedback.text = string.Empty;
    }

    private void ShowLoading(string msg)
    {
        // 서브 UI 모두 닫기
        togetherSubGroup.SetActive(false);
        codeInputPanel.SetActive(false);
        ClearFeedback();

        loadingOverlay.SetActive(true);
        txtLoadingMsg.text = msg;
    }

    // ─────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────

    private void ResetToMain()
    {
        togetherSubGroup.SetActive(false);
        codeInputPanel.SetActive(false);
        loadingOverlay.SetActive(false);
        ClearFeedback();
    }
}