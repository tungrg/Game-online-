using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class NetworkInputHandler : SimulationBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _cachedRunner;
    private bool _isRegistered;

    void Awake()
    {
        _cachedRunner = GetComponent<NetworkRunner>();
    }

    void OnEnable()
    {
        RegisterIfPossible();
    }

    void OnDisable()
    {
        UnregisterIfNeeded();
    }

    void OnDestroy()
    {
        UnregisterIfNeeded();
    }

    void Start()
    {
        RegisterIfPossible();
    }

    void RegisterIfPossible()
    {
        if (_isRegistered || !isActiveAndEnabled)
        {
            return;
        }

        if (_cachedRunner == null)
        {
            _cachedRunner = GetComponent<NetworkRunner>();
        }

        if (_cachedRunner == null)
        {
            return;
        }

        _cachedRunner.ProvideInput = true;
        _cachedRunner.AddCallbacks(this);
        _isRegistered = true;
    }

    void UnregisterIfNeeded()
    {
        if (!_isRegistered || _cachedRunner == null)
        {
            return;
        }

        _cachedRunner.RemoveCallbacks(this);
        _isRegistered = false;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        PlayerInputData data = new PlayerInputData();

        if (Chat.IsInputFocused)
        {
            data.moveX = 0f;
            data.moveY = 0f;
            data.isShooting = false;
            data.isShield = false;
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
        else
        {
            data.hasAim = false;
        }

        data.isShooting = Input.GetMouseButton(0);
        data.isShield = Input.GetKey(KeyCode.R);
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
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
        Fusion_OnConnectedToServer(runner);
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Fusion_OnDisconnectedFromServer(runner, reason);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void Fusion_OnConnectedToServer(NetworkRunner runner) { }
    public void Fusion_OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data)
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
}