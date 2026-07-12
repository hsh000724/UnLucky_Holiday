using System.Collections;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchRoomUI : MonoBehaviour
{
    [Header("접속 인원")]
    public TMP_Text txtPlayerCount;

    [Header("방 정보")]
    public TMP_Text txtRoomCode;
    public Button btnCopyCode;

    [Header("내 슬롯")]
    public Image myProfileImage;
    public TMP_Text myNickname;
    public TMP_Text myReadyState;
    public Sprite defaultProfileSprite;

    [Header("상대 슬롯")]
    public GameObject opponentWaitingGroup;   // 로딩 스피너
    public GameObject opponentProfileGroup;   // 프로필
    public Image opponentProfileImage;
    public TMP_Text opponentNickname;
    public TMP_Text opponentReadyState;

    [Header("내 카드 상태 표시")]
    public Image myStatusDot;        // 초록/빨간 원
    public TMP_Text myStatusText;       // "Ready" / "Not Ready"

    [Header("상대 카드 상태 표시")]
    public Image opponentStatusDot;
    public TMP_Text opponentStatusText;

    [Header("버튼")]
    public Button btnReady;
    public TMP_Text txtReadyBtn;

    private FirebaseFirestore _db;
    private ListenerRegistration _opponentListener;
    private ListenerRegistration _roomListener;

    private string _roomId;
    private string _mySlot;
    private string _opponentSlot;
    private bool _isReady = false;

    // 색상 상수
    private static readonly Color ColorReady = new Color(0f, 1f, 0f);   // #00FF00
    private static readonly Color ColorNotReady = new Color(1f, 0.2f, 0.2f); // #FF3333

    void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _roomId = MatchmakingManager.CurrentRoomId;
        _mySlot = MatchmakingManager.CurrentPlayerSlot;
        _opponentSlot = _mySlot == "player1" ? "player2" : "player1";
        UpdatePlayerCount(1);

        // 내 프로필
        myNickname.text = AuthManager.Instance.UserNickname;
        myProfileImage.sprite = defaultProfileSprite;
        myReadyState.text = "준비 중...";

        // 상대 슬롯 초기 상태
        SetOpponentWaiting(true);

        // 방 코드 로드
        LoadRoomCode();

        // 실시간 리슨 시작
        ListenForOpponent();
        ListenForRoomStatus();

        // 초기 상태 → 둘 다 Not Ready
        SetReadyState(false, myStatusDot, myStatusText);
        SetReadyState(false, opponentStatusDot, opponentStatusText);

        btnReady.onClick.AddListener(OnClickReady);
        btnCopyCode.onClick.AddListener(OnClickCopyCode);
    }

    private void UpdatePlayerCount(int count)
    {
        txtPlayerCount.text = $"{count} / 2";
    }

    // ─────────────────────────────────────
    // 방 코드
    // ─────────────────────────────────────
    private async void LoadRoomCode()
    {
        var snap = await _db.Collection("matchRooms").Document(_roomId).GetSnapshotAsync();
        if (snap.Exists)
            txtRoomCode.text = snap.GetValue<string>("roomCode");
    }

    private void OnClickCopyCode()
    {
        GUIUtility.systemCopyBuffer = txtRoomCode.text;
        StartCoroutine(ShowCopyFeedback());
    }

    private IEnumerator ShowCopyFeedback()
    {
        txtRoomCode.text = "복사됨!";
        yield return new WaitForSeconds(1.5f);
        LoadRoomCode();
    }

    // ─────────────────────────────────────
    // 상대방 입장 리슨
    // ─────────────────────────────────────
    private void ListenForOpponent()
    {
        _opponentListener = _db
            .Collection("matchRooms").Document(_roomId)
            .Collection("players").Document(_opponentSlot)
            .Listen(snapshot =>
            {
                if (!snapshot.Exists)
                {
                    MainThreadDispatcher.Enqueue(() => OnOpponentLeft());
                    return;
                }

                string nickname = snapshot.GetValue<string>("nickname");
                bool isReady = snapshot.ContainsField("isReady") &&
                                  snapshot.GetValue<bool>("isReady");

                MainThreadDispatcher.Enqueue(() =>
                {
                    opponentNickname.text = nickname;
                    opponentReadyState.text = isReady ? "준비 완료" : "준비 중...";
                    opponentProfileImage.sprite = defaultProfileSprite;
                    SetOpponentWaiting(false);
                    UpdatePlayerCount(2);

                    // ✅ 상대 카드 상태 업데이트
                    SetReadyState(isReady, opponentStatusDot, opponentStatusText);
                });
            });
    }
    private void OnOpponentLeft()
    {
        // 상대 슬롯 다시 대기 상태로
        SetOpponentWaiting(true);
        UpdatePlayerCount(1);

        // 룸 status를 waiting으로 복구 (새 상대 받을 수 있게)
        _ = _db.Collection("matchRooms").Document(_roomId)
               .UpdateAsync("status", "waiting");

        // 안내 메시지 (코루틴으로 잠시 표시)
        StartCoroutine(ShowOpponentLeftMessage());
    }

    private IEnumerator ShowOpponentLeftMessage()
    {
        // opponentNickname 텍스트를 안내 메시지로 임시 사용
        // 또는 별도 안내 TMP를 Inspector에서 연결해도 됩니다
        opponentNickname.text = "상대방이 나갔습니다.";
        opponentReadyState.text = "";

        // 상대 프로필 그룹 잠깐 보여줬다가 다시 로딩으로
        opponentProfileGroup.SetActive(true);
        opponentWaitingGroup.SetActive(false);

        yield return new WaitForSeconds(2f);

        // 로딩 상태로 복구
        SetOpponentWaiting(true);
    }

    // ─────────────────────────────────────
    // 양쪽 준비 완료 시 게임 씬 전환 리슨
    // ─────────────────────────────────────
    private void ListenForRoomStatus()
    {
        _roomListener = _db.Collection("matchRooms").Document(_roomId)
            .Listen(snapshot =>
            {
                if (!snapshot.Exists) return;
                if (!snapshot.ContainsField("bothReady")) return;

                bool bothReady = snapshot.GetValue<bool>("bothReady");
                if (bothReady)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // TODO: 게임 씬 이름으로 교체
                        ModeManager.instance.SetMode(ModeManager.GameMode.Battle);
                        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleMode");
                    });
                }
            });
    }

    // ─────────────────────────────────────
    // 준비 버튼
    // ─────────────────────────────────────
    private async void OnClickReady()
    {
        _isReady = !_isReady;
        btnReady.interactable = false;

        await _db.Collection("matchRooms").Document(_roomId)
                 .Collection("players").Document(_mySlot)
                 .UpdateAsync("isReady", _isReady);

        if (_mySlot == "player1")
            await CheckBothReady();

        // ✅ 내 카드 상태 업데이트
        myReadyState.text = _isReady ? "준비 완료" : "준비 중...";
        txtReadyBtn.text = _isReady ? "준비 취소" : "준비하기";
        SetReadyState(_isReady, myStatusDot, myStatusText);

        btnReady.interactable = true;
    }
    // 준비 상태 UI 갱신 메서드 추가
    private void SetReadyState(bool isReady, Image dot, TMP_Text text)
    {
        if (dot != null)
            dot.color = isReady ? ColorReady : ColorNotReady;

        if (text != null)
        {
            text.text = isReady ? "Ready" : "Not Ready";
            text.color = isReady ? ColorReady : ColorNotReady;
        }
    }

    private async System.Threading.Tasks.Task CheckBothReady()
    {
        var p1Snap = await _db.Collection("matchRooms").Document(_roomId)
                              .Collection("players").Document("player1").GetSnapshotAsync();
        var p2Snap = await _db.Collection("matchRooms").Document(_roomId)
                              .Collection("players").Document("player2").GetSnapshotAsync();

        bool p1Ready = p1Snap.Exists && p1Snap.GetValue<bool>("isReady");
        bool p2Ready = p2Snap.Exists && p2Snap.GetValue<bool>("isReady");

        if (p1Ready && p2Ready)
        {
            await _db.Collection("matchRooms").Document(_roomId)
                     .UpdateAsync("bothReady", true);
        }
    }

    // ─────────────────────────────────────
    // 방 나가기
    // ─────────────────────────────────────
    public void OnClickLeave()
    {
        MatchmakingManager.Instance.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    // ─────────────────────────────────────
    // UI 헬퍼
    // ─────────────────────────────────────
    private void SetOpponentWaiting(bool isWaiting)
    {
        opponentWaitingGroup.SetActive(isWaiting);
        opponentProfileGroup.SetActive(!isWaiting);
    }

    void OnDestroy()
    {
        _opponentListener?.Stop();
        _roomListener?.Stop();
    }
}