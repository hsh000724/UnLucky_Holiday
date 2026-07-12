using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class MatchmakingManager : MonoBehaviour
{
    public static MatchmakingManager Instance { get; private set; }

    public static string CurrentRoomId { get; private set; }
    public static string CurrentPlayerSlot { get; private set; }

    public event Action OnRoomCreated;
    public event Action OnRoomJoined;
    public event Action<string> OnMatchmakingFailed;

    private FirebaseFirestore _db;

    // Firebase 초기화 완료 후 안전하게 가져오는 프로퍼티
    private FirebaseFirestore Db
    {
        get
        {
            if (_db == null)
                _db = FirebaseFirestore.DefaultInstance;
            return _db;
        }
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // _db 초기화 제거 (Db 프로퍼티가 대신 처리)
    }

    // ─────────────────────────────────────
    // 랜덤 매칭
    // ─────────────────────────────────────
    public async void StartRandomMatching()
    {
        while (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
            await Task.Delay(100);

        try
        {
            string waitingRoomId = await FindWaitingRoom(isInviteRoom: false);

            if (waitingRoomId != null)
                await JoinRoom(waitingRoomId);
            else
                await CreateRoom(isInviteRoom: false);

            OnRoomJoined?.Invoke();
            SceneManager.LoadScene("MatchRoom");
        }
        catch (Exception e)
        {
            Debug.LogError($"랜덤 매칭 실패: {e.Message}");
            OnMatchmakingFailed?.Invoke("매칭에 실패했습니다. 다시 시도해주세요.");
        }
    }

    // ─────────────────────────────────────
    // 초대방 생성 (방장)
    // ─────────────────────────────────────
    public async void CreateInviteRoom()
    {
        while (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
            await Task.Delay(100);

        try
        {
            await CreateRoom(isInviteRoom: true);
            OnRoomCreated?.Invoke();
            SceneManager.LoadScene("MatchRoom");
        }
        catch (Exception e)
        {
            Debug.LogError($"초대방 생성 실패: {e.Message}");
            OnMatchmakingFailed?.Invoke("방 생성에 실패했습니다.");
        }
    }

    // ─────────────────────────────────────
    // 초대 코드로 입장 (초대받은 친구)
    // ─────────────────────────────────────
    public async void JoinByRoomCode(string roomCode)
    {
        while (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
            await Task.Delay(100);

        try
        {
            QuerySnapshot snapshot = await Db.Collection("matchRooms")
                .WhereEqualTo("roomCode", roomCode)
                .WhereEqualTo("status", "waiting")
                .Limit(1)
                .GetSnapshotAsync();

            if (snapshot.Count == 0)
            {
                OnMatchmakingFailed?.Invoke("유효하지 않은 코드이거나 이미 시작된 방입니다.");
                return;
            }

            string roomId = snapshot.Documents.First().Id;
            await JoinRoom(roomId);
            SceneManager.LoadScene("MatchRoom");
        }
        catch (Exception e)
        {
            Debug.LogError($"코드 입장 실패: {e.Message}");
            OnMatchmakingFailed?.Invoke("입장에 실패했습니다.");
        }
    }

    // ─────────────────────────────────────
    // 내부 로직
    // ─────────────────────────────────────
    private async Task<string> FindWaitingRoom(bool isInviteRoom)
    {
        QuerySnapshot snapshot = await Db.Collection("matchRooms")
            .WhereEqualTo("status", "waiting")
            .WhereEqualTo("isInviteRoom", isInviteRoom)
            .OrderBy("createdAt")
            .Limit(1)
            .GetSnapshotAsync();

        return snapshot.Count > 0 ? snapshot.Documents.First().Id : null;
    }

    private async Task CreateRoom(bool isInviteRoom)
    {
        string roomCode = GenerateRoomCode();
        DocumentReference roomRef = Db.Collection("matchRooms").Document();

        await roomRef.SetAsync(new Dictionary<string, object>
        {
            { "roomCode",     roomCode },
            { "status",       "waiting" },
            { "isInviteRoom", isInviteRoom },
            { "createdAt",    FieldValue.ServerTimestamp }
        });

        await roomRef.Collection("players").Document("player1").SetAsync(
            BuildPlayerData()
        );

        CurrentRoomId = roomRef.Id;
        CurrentPlayerSlot = "player1";
    }

    private async Task JoinRoom(string roomId)
    {
        DocumentReference roomRef = Db.Collection("matchRooms").Document(roomId);

        await roomRef.UpdateAsync("status", "full");

        await roomRef.Collection("players").Document("player2").SetAsync(
            BuildPlayerData()
        );

        CurrentRoomId = roomId;
        CurrentPlayerSlot = "player2";
    }

    private Dictionary<string, object> BuildPlayerData()
    {
        string uid = SystemInfo.deviceUniqueIdentifier;
        string nickname = "Unknown";

        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            nickname = AuthManager.Instance.UserNickname ?? "Unknown";
        }

        return new Dictionary<string, object>
        {
            { "uid",      uid },
            { "nickname", nickname },
            { "isReady",  false },
            { "joinedAt", FieldValue.ServerTimestamp }
        };
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] code = new char[8];
        var rng = new System.Random();
        for (int i = 0; i < 8; i++)
            code[i] = chars[rng.Next(chars.Length)];
        return new string(code);
    }

    // ─────────────────────────────────────
    // 룸 나가기
    // ─────────────────────────────────────
    public async void LeaveRoom()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;
        try
        {
            DocumentReference roomRef = Db.Collection("matchRooms").Document(CurrentRoomId);

            if (CurrentPlayerSlot == "player1")
            {
                // 룸 삭제 → 상대방 ListenForOpponent가 snapshot.Exists = false 감지
                await roomRef.Collection("players").Document("player1").DeleteAsync();
                await roomRef.DeleteAsync();
            }
            else
            {
                // Player 2 문서 삭제 → 상대방이 감지
                await roomRef.Collection("players").Document("player2").DeleteAsync();
                await roomRef.UpdateAsync("status", "waiting");
            }

            CurrentRoomId = null;
            CurrentPlayerSlot = null;
        }
        catch (Exception e)
        {
            Debug.LogError($"룸 나가기 실패: {e.Message}");
        }
    }
}