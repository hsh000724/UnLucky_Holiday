using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance;

    public NetworkRunner runner;

    public bool IsConnected => runner != null && runner.IsRunning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 랜덤 매칭
    public async void StartRandomMatch()
    {
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            runner.AddCallbacks(this);
        }

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "RandomMatch",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log(result.Ok ? "랜덤 매칭 시작" : "랜덤 매칭 실패");
    }

    // 초대방 생성
    public async void CreateInviteRoom(string roomCode)
    {
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            runner.AddCallbacks(this);
        }

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomCode,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log(result.Ok ? $"방 생성 : {roomCode}" : "방 생성 실패");
    }

    // 초대방 참가
    public async void JoinInviteRoom(string roomCode)
    {
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            runner.AddCallbacks(this);
        }

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = roomCode,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log(result.Ok ? $"방 참가 : {roomCode}" : "방 참가 실패");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"플레이어 입장 : {player}");

        if (runner.ActivePlayers.Count() == 2)
        {
            Debug.Log("매칭 완료");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"플레이어 퇴장 : {player}");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("서버 연결 성공");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"연결 끊김 : {reason}");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"연결 실패 : {reason}");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Shutdown : {shutdownReason}");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}