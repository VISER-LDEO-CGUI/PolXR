using Fusion;
using Fusion.Addons.ConnectionManagerAddon;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(ConnectionManager))]
public class UpdateConnectionStatus : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;
    private AudioSource audioSource;

    public AudioClip connectedToServer;
    public AudioClip disconnectedFromServer;
    public AudioClip shutdown;
    public AudioClip connectFailed;
    public AudioClip localUserSpawned;
    public AudioClip playerJoined;
    public AudioClip playerLeft;

    public TextMeshProUGUI sessionStatus;
       
    private void Start()
    {
        // Find the associated runner, if not defined
        if (runner == null) runner = GetComponentInParent<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("Should be stored under a NetworkRunner to be discoverable");
            return;
        }
        runner.AddCallbacks(this);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        var connectionManager = GetComponent<ConnectionManager>();
        connectionManager.onWillConnect.AddListener(OnWillConnect);
    }


    private void DebugLog(string debug)
    {
        if(sessionStatus) sessionStatus.text = debug;
        Debug.Log(debug);
    }

    void OnWillConnect()
    {
        DebugLog("Starting connection. Please wait...");
    }

    #region INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
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

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        DebugLog($"Shutdown : { shutdownReason} ");
        audioSource.PlayOneShot(shutdown);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        DebugLog("Connected to the server");
        audioSource.PlayOneShot(connectedToServer);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        DebugLog($"Disconnected From Server: {runner.SessionInfo} {reason}");
        audioSource.PlayOneShot(disconnectedFromServer);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        DebugLog($"Connect Failed : { reason} ");
        audioSource.PlayOneShot(connectFailed);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){}

    #endregion
}

