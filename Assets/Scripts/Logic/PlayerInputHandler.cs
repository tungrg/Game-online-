using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    bool _shootBuffered;
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        PlayerInputData data = new PlayerInputData();

        if (Chat.IsInputFocused)
        {
            input.Set(data);
            return;
        }

        data.moveX = Input.GetAxisRaw("Horizontal");
        data.moveY = Input.GetAxisRaw("Vertical");

        if (TryGetAimPoint(out Vector3 aimPoint))
        {
            data.aimX = aimPoint.x;
            data.aimZ = aimPoint.z;
            data.hasAim = true;
        }

        // 🔥 BUFFER INPUT
        if (Input.GetMouseButtonDown(0))
        {
            _shootBuffered = true;
        }

        data.isShooting = _shootBuffered || Input.GetMouseButton(0);

        // reset sau khi gửi
        _shootBuffered = false;

        data.isShield = Input.GetKeyDown(KeyCode.R);

        input.Set(data);
    }

    private static bool TryGetAimPoint(out Vector3 point)
    {
        point = Vector3.zero;

        Camera cam = Camera.main;
        if (cam == null)
        {
            return false;
        }

        Vector3 screenPos = Input.mousePosition;
        if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
        }

        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out float enter))
        {
            return false;
        }

        point = ray.GetPoint(enter);
        return true;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        request.Accept();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        Chat.HandleReliableDataReceived(runner, player, key, data);
        Health.HandleReliableDataReceived(runner, key, data);
        PlayerSkill.HandleReliableDataReceived(runner, key, data);
        PlayerProperties.HandleReliableDataReceived(runner, key, data);
        Leaderboard.HandleReliableDataReceived(runner, key, data);
        AfterMatch.HandleReliableDataReceived(runner, key, data);
        Trap.HandleReliableDataReceived(runner, key, data);
        TankController.HandleReliableDataReceived(runner, player, key, data);
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
}