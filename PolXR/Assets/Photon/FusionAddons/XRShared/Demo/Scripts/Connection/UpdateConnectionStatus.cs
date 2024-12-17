using Fusion;
using Fusion.Addons.ConnectionManagerAddon;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateConnectionStatus : MonoBehaviour, INetworkRunnerCallbacks
{
    protected NetworkRunner runner;
    private AudioSource audioSource;

    public AudioClip connectedToServer;
    public AudioClip disconnectedFromServer;
    public AudioClip shutdown;
    public AudioClip connectFailed;
    public AudioClip localUserSpawned;
    public AudioClip playerJoined;
    public AudioClip playerLeft;

    public TextMeshProUGUI sessionStatus;
       
    protected virtual void Start()
    {
        FindRunner();
        runner.AddCallbacks(this);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        var connectionManager = runner.GetComponent<ConnectionManager>();
        connectionManager.onWillConnect.AddListener(OnWillConnect);
    }

    protected virtual void FindRunner()
    {
        // Find the associated runner, if not defined
        if (runner == null) runner = GetComponentInParent<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("Should be stored under a NetworkRunner to be discoverable");
            return;
        }
    }

    protected virtual void DebugLog(string debug, bool permanentError = false)
    {
        sessionStatus.text = debug;
        if (permanentError)
        {
            Debug.LogError(debug);
        }
        else
        {
            Debug.Log(debug);
        }
    }

    void OnWillConnect()
    {
        DebugLog("Starting connection. Please wait...");
    }

    #region INetworkRunnerCallbacks
    public virtual void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

        audioSource.PlayOneShot(playerJoined);

        if (player == runner.LocalPlayer)
        {
            DebugLog("You have joined !");
        }
        else
            DebugLog("A player joined !");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        audioSource.PlayOneShot(playerLeft);
        DebugLog("A player left !");
    }


    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        DebugLog($"Shutdown : { shutdownReason} ", permanentError: true);
        audioSource.PlayOneShot(shutdown);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        DebugLog("Connected to the server");
        audioSource.PlayOneShot(connectedToServer);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        DebugLog($"Disconnected From Server: {runner.SessionInfo} ({reason})", permanentError: true);
        audioSource.PlayOneShot(disconnectedFromServer);
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        DebugLog($"Connect Failed : { reason} ", permanentError: true);
        audioSource.PlayOneShot(connectFailed);
    }
    #endregion

    #region INetworkRunnerCallbacks (unused)
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}

