using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleGameManager : MonoBehaviour
{
    public static BattleGameManager Instance { get; private set; }

    // ─────────────────────────────────────
    // Inspector 연결
    // ─────────────────────────────────────
    [Header("매니저 연결")]
    public BattleHinderManager hinderManager;
    public BattleUIManager battleUI;

    [Header("방해몬스터 설정")]
    public int killsPerHinder = 5; // 몇 마리마다 방해몬스터 전송

    // ─────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────
    private FirebaseFirestore _db;
    private ListenerRegistration _opponentListener;
    private ListenerRegistration _roomListener;

    private string _roomId;
    private string _mySlot;
    private string _opponentSlot;

    private HPSyncHelper _hpSyncHelper = new HPSyncHelper();

    // 내 데이터 로컬 캐시
    private int _killCount = 0;
    private int _hinderStack = 0; // 처치 누적 카운터 (방해 전송용)
    private bool _bossCleared = false;
    private bool _isDead = false;
    private HashSet<string> _myCollections = new HashSet<string>();

    // 필드 추가
    private int _lastSyncedEnemyCount = -1;

    // ─────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _db = FirebaseFirestore.DefaultInstance;
        _roomId = MatchmakingManager.CurrentRoomId;
        _mySlot = MatchmakingManager.CurrentPlayerSlot;
        _opponentSlot = _mySlot == "player1" ? "player2" : "player1";
    }

    void Start()
    {
        StartCoroutine(InitBattleRoom());
    }

    private IEnumerator InitBattleRoom()
    {
        // Firebase 초기화 대기
        while (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
            yield return new WaitForSeconds(0.1f);

        // 게임 시작 데이터 초기화
        _ = InitMyPlayerData();

        // 상대방 실시간 리슨 시작
        ListenOpponent();
        ListenRoomStatus();
    }

    // 게임 시작 시 내 플레이어 데이터 초기화
    private async System.Threading.Tasks.Task InitMyPlayerData()
    {
        await _db.Collection("gameRooms").Document(_roomId)
            .Collection("players").Document(_mySlot)
            .SetAsync(new Dictionary<string, object>
            {
                { "uid",         AuthManager.Instance.IsLoggedIn
                                   ? Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId
                                   : SystemInfo.deviceUniqueIdentifier },
                { "nickname",    AuthManager.Instance.UserNickname ?? "Unknown" },
                { "hpPercent",   100 },
                { "killCount",   0 },
                { "hinderStack", 0 },
                { "collections", new List<string>() },
                { "bossCleared", false },
                { "isDead",      false }
            });
    }

    // ─────────────────────────────────────
    // 내 데이터 업데이트 (외부에서 호출)
    // ─────────────────────────────────────

    // GameManager.health 변경 시 호출
    public void OnHpChanged(float currentHp, float maxHp)
    {
        if (_hpSyncHelper.ShouldSync((int)currentHp, (int)maxHp))
        {
            int percent = Mathf.Clamp(Mathf.FloorToInt(currentHp / maxHp * 100f), 0, 100);
            _ = UpdateMyField("hpPercent", percent);
        }
    }

    // Enemy.Dead() 에서 호출 (방해몬스터 제외)
    public void OnEnemyKilled(bool isHinderEnemy)
    {
        if (isHinderEnemy) return; // 방해몬스터는 카운트 제외

        _killCount++;
        _hinderStack++;

        // 킬카운트 Firestore 업데이트
        _ = UpdateMyField("killCount", _killCount);

        // 방해몬스터 전송 조건 확인
        if (_hinderStack >= killsPerHinder)
        {
            _hinderStack = 0;
            SendHinderMonster();
        }
    }

    // CollectionManager.Collect() 에서 호출
    public void OnCollectionAcquired(string collectionId)
    {
        _myCollections.Add(collectionId);
        _ = UpdateMyField("collections", new List<string>(_myCollections));
    }

    // 보스 처치 시 호출
    public async void OnBossCleared()
    {
        _bossCleared = true;
        await UpdateMyField("bossCleared", true);

        // 승리 처리
        OnBattleResult(isWin: true);
    }

    // 사망 시 호출
    public async void OnPlayerDead()
    {
        if (_isDead) return;
        _isDead = true;

        await UpdateMyField("isDead", true);
        await UpdateMyField("hpPercent", 0);

        OnBattleResult(isWin: false);
    }

    // ─────────────────────────────────────
    // 방해몬스터 전송
    // ─────────────────────────────────────
    private async void SendHinderMonster()
    {
        // Firestore에 방해몬스터 소환 이벤트 기록
        // 상대방이 리슨하다가 감지하면 본인 필드에 소환
        await _db.Collection("gameRooms").Document(_roomId)
            .Collection("hinderEvents").Document()
            .SetAsync(new Dictionary<string, object>
            {
                { "targetSlot", _opponentSlot },
                { "sentBy",     _mySlot },
                { "timestamp",  FieldValue.ServerTimestamp }
            });

        Debug.Log($"방해몬스터 전송 → {_opponentSlot}");
    }

    // ─────────────────────────────────────
    // 상대방 실시간 리슨
    // ─────────────────────────────────────
    private void ListenOpponent()
    {
        _opponentListener = _db
            .Collection("gameRooms").Document(_roomId)
            .Collection("players").Document(_opponentSlot)
            .Listen(snapshot =>
            {
                if (!snapshot.Exists) return;

                int hpPercent = snapshot.ContainsField("hpPercent")
                                   ? snapshot.GetValue<int>("hpPercent") : 100;
                int killCount = snapshot.ContainsField("killCount")
                                   ? snapshot.GetValue<int>("killCount") : 0;
                int enemyCount = snapshot.ContainsField("enemyCount")  // ✅ 추가
                                   ? snapshot.GetValue<int>("enemyCount") : 0;
                bool bossCleared = snapshot.ContainsField("bossCleared")
                                   && snapshot.GetValue<bool>("bossCleared");
                bool isDead = snapshot.ContainsField("isDead")
                                   && snapshot.GetValue<bool>("isDead");
                var collections = snapshot.ContainsField("collections")
                                   ? snapshot.GetValue<List<string>>("collections")
                                   : new List<string>();

                MainThreadDispatcher.Enqueue(() =>
                {
                    battleUI.UpdateOpponentHP(hpPercent);
                    battleUI.UpdateOpponentKillCount(killCount);
                    battleUI.UpdateOpponentEnemyCount(enemyCount);  // ✅ 추가
                    battleUI.UpdateOpponentCollections(collections);

                    if (bossCleared) OnBattleResult(isWin: false);
                    if (isDead && !_isDead) OnBattleResult(isWin: true);
                });
            });
    }
    public void OnEnemyCountChanged(int count)
    {
        // 내 UI → 로컬이라 항상 즉시 업데이트
        battleUI.UpdateMyEnemyCount(count);

        // Firestore → 10 단위로만 업데이트
        int currentBracket = (count / 10) * 10;
        int lastBracket = (_lastSyncedEnemyCount / 10) * 10;

        if (currentBracket != lastBracket || _lastSyncedEnemyCount == -1)
        {
            _lastSyncedEnemyCount = count;
            _ = UpdateMyField("enemyCount", count);
        }
    }

    // 방해몬스터 이벤트 리슨
    private void ListenRoomStatus()
    {
        _roomListener = _db
            .Collection("gameRooms").Document(_roomId)
            .Collection("hinderEvents")
            .Listen(snapshot =>
            {
                foreach (var change in snapshot.GetChanges())
                {
                    if (change.ChangeType != DocumentChange.Type.Added) continue;

                    var doc = change.Document;
                    string targetSlot = doc.ContainsField("targetSlot")
                                        ? doc.GetValue<string>("targetSlot") : "";

                    // 나를 대상으로 한 이벤트만 처리
                    if (targetSlot != _mySlot) continue;

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        hinderManager.SpawnHinderMonster();
                        Debug.Log("방해몬스터 소환됨!");
                    });
                }
            });
    }

    // ─────────────────────────────────────
    // 승패 처리
    // ─────────────────────────────────────
    private bool _battleResultHandled = false;

    public void OnBattleResult(bool isWin)
    {
        if (_battleResultHandled) return;
        _battleResultHandled = true;

        GameManager.instance.isLive = false;

        _ = _db.Collection("gameRooms").Document(_roomId)
               .UpdateAsync("status", "finished");

        StartCoroutine(LoadResultScene(isWin));
    }

    private IEnumerator LoadResultScene(bool isWin)
    {
        // 전적 저장
        string userId = AuthManager.Instance.IsLoggedIn
                          ? Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId
                          : SystemInfo.deviceUniqueIdentifier;
        string nickname = AuthManager.Instance.UserNickname ?? "Unknown";
        bool isBossKill = _bossCleared;

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.SaveBattleRecord(
                userId,
                nickname,
                isWin,
                _killCount,
                GameManager.instance.gameTime,
                isBossKill
            );
        }

        // 결과 데이터 전달
        BattleResultData.IsWin = isWin;
        BattleResultData.KillCount = _killCount;
        BattleResultData.GameTime = GameManager.instance.gameTime;

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Result");
    }

    // ─────────────────────────────────────
    // 유틸
    // ─────────────────────────────────────
    private async System.Threading.Tasks.Task UpdateMyField(string field, object value)
    {
        try
        {
            await _db.Collection("gameRooms").Document(_roomId)
                     .Collection("players").Document(_mySlot)
                     .UpdateAsync(field, value);
        }
        catch (Exception e)
        {
            Debug.LogError($"Firestore 업데이트 실패 [{field}]: {e.Message}");
        }
    }

    void OnDestroy()
    {
        _opponentListener?.Stop();
        _roomListener?.Stop();
    }
}