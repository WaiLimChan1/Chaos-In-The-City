using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerController : MonoBehaviour, INetworkRunnerCallbacks
{
    public event Action OnStartedRunnerConnection;
    public event Action OnPlayerJoinedSuccessfully;

    [SerializeField] private NetworkRunner _networkRunnerPrefab;
    private NetworkRunner _networkRunnerInstance;

    [Header("Multiplayer Information")]
    public string LocalPlayerName;
    public string Region;
    public string RoomCode;
    public GameMode LocalGameMode;

    public void ShutDownRunner()
    {
        _networkRunnerInstance.Shutdown();
    }

    public AppSettings BuildCustomAppSetting()
    {
        AppSettings appSettings = PhotonAppSettings.Instance.AppSettings.GetCopy();
        appSettings.FixedRegion = Region;
        return appSettings;
    }

    public async void StartGame(string playerName, string region, GameMode mode, string roomCode)
    {
        LocalPlayerName = playerName;
        Region = region;
        LocalGameMode = mode;
        RoomCode = roomCode;

        OnStartedRunnerConnection?.Invoke();

        if (_networkRunnerInstance == null)
        {
            _networkRunnerInstance = Instantiate(_networkRunnerPrefab);
        }

        //Register so we will get the callbacks as well
        _networkRunnerInstance.AddCallbacks(this);
        _networkRunnerInstance.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = LocalGameMode,
            SessionName = RoomCode,
            PlayerCount = 8,
            SceneManager = _networkRunnerInstance.GetComponent<INetworkSceneManager>(),
            CustomPhotonAppSettings = BuildCustomAppSetting()
        };

        var result = await _networkRunnerInstance.StartGame(startGameArgs);

        if (result.Ok)
        {
            const string SCENE_NAME = "Main Game";
            _networkRunnerInstance.SetActiveScene(SCENE_NAME);
        }
        else
        {
            Debug.LogError($"Failed to start: {result.ShutdownReason}");
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        //Debug.Log("OnConnectedToServer");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        //Debug.Log("OnConnectFailed");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        //Debug.Log("OnConnectRequest");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        //Debug.Log("OnCustomAuthenticationResponse");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        //Debug.Log("OnDisconnectedFromServer");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        //Debug.Log("OnHostMigration");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //Debug.Log("OnInput");
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        //Debug.Log("OnInputMissing");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //Debug.Log("OnPlayerJoined");
        OnPlayerJoinedSuccessfully?.Invoke();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        //Debug.Log("OnPlayerLeft");
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        //Debug.Log("OnReliableDataReceived");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        //Debug.Log("OnSceneLoadDone");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        //Debug.Log("OnSceneLoadStart");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //Debug.Log("OnSessionListUpdated");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //Debug.Log("OnShutdown");
        SceneManager.LoadScene("Start Menu");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        //Debug.Log("OnUserSimulationMessage");
    }
}


