using System.Collections.Generic;
using System;
using System.Text;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class PlayerSkill : NetworkBehaviour
{
    private static readonly List<PlayerSkill> ActiveSkills = new List<PlayerSkill>();
    private static readonly Dictionary<string, PlayerSkill> SkillByNetworkId = new Dictionary<string, PlayerSkill>();
    private static readonly ReliableKey ShieldSyncKey = ReliableKey.FromInts(88, 3, 0, 0);

    [Serializable]
    private class ShieldSyncPacket
    {
        public string networkId;
        public bool active;
    }

    [Header("Shield")]
    [SerializeField] private float shieldRadius = 2.5f;
    [SerializeField] private float shieldDuration = 2.5f;
    [SerializeField] private float shieldCooldown = 5f;
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private Transform shieldVisualAnchor;

    public bool IsShieldActive => _shieldActiveLocal;
    private TickTimer _shieldTimer;
    private TickTimer _cooldownTimer;
    private Health _health;
    private bool _shieldActiveLocal;
    private GameObject _runtimeShieldVisual;
    private string _networkId;

    public float ShieldRadius => Mathf.Max(0.1f, shieldRadius);
    public static IReadOnlyList<PlayerSkill> GetActiveSkills()
    {
        return ActiveSkills;
    }

    public override void Spawned()
    {
        if (!ActiveSkills.Contains(this))
        {
            ActiveSkills.Add(this);
        }

        _health = GetComponentInParent<Health>();

        if (Object != null)
        {
            _networkId = Object.Id.ToString();
            if (!string.IsNullOrEmpty(_networkId))
            {
                SkillByNetworkId[_networkId] = this;
            }
        }

        EnsureShieldVisualReference();
        
    }
    public override void Render()
    {
        if (_runtimeShieldVisual == null)
            return;

        _runtimeShieldVisual.SetActive(_shieldActiveLocal);
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(_networkId))
        {
            SkillByNetworkId.Remove(_networkId);
        }

        ActiveSkills.Remove(this);
    }

    private void LateUpdate()
    {
        if (_runtimeShieldVisual != null)
        {
            UpdateShieldVisualFollow();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (_shieldActiveLocal && _shieldTimer.ExpiredOrNotRunning(Runner))
        {
            DisableShield();
        }

        if (!GetInput<PlayerInputData>(out var input) || !input.isShield)
        {
            return;
        }

        if (_shieldActiveLocal)
        {
            return;
        }

        if (!_cooldownTimer.ExpiredOrNotRunning(Runner))
        {
            return;
        }

        EnableShield();
    }

    public bool TryBlockSegment(int bulletTeam, Transform shooterRoot, Vector3 segmentStart, Vector3 segmentEnd, out Vector3 blockPoint)
    {
        blockPoint = Vector3.zero;

        if (!_shieldActiveLocal)
        {
            return false;
        }

        Transform shieldRoot = transform.root;
        if (shooterRoot != null && shieldRoot == shooterRoot)
        {
            return false;
        }

        if (_health != null && _health.Team >= 0 && bulletTeam == _health.Team)
        {
            return false;
        }

        Vector3 center = transform.position;
        Vector3 segment = segmentEnd - segmentStart;
        float segmentSqrLength = segment.sqrMagnitude;

        float t = 0f;
        if (segmentSqrLength > 0.000001f)
        {
            t = Mathf.Clamp01(Vector3.Dot(center - segmentStart, segment) / segmentSqrLength);
        }

        Vector3 closestPoint = segmentStart + segment * t;
        float maxDistance = ShieldRadius;
        if ((closestPoint - center).sqrMagnitude > maxDistance * maxDistance)
        {
            return false;
        }

        blockPoint = closestPoint;
        return true;
    }

    private void EnableShield()
    {
        SetShieldActive(true);
        _shieldTimer = TickTimer.CreateFromSeconds(Runner, Mathf.Max(0.1f, shieldDuration));
        UpdateShieldVisualFollow();
    }

    private void DisableShield()
    {
        SetShieldActive(false);
        _cooldownTimer = TickTimer.CreateFromSeconds(Runner, Mathf.Max(0.1f, shieldCooldown));
        if (_runtimeShieldVisual != null)
        {
            _runtimeShieldVisual.SetActive(false);
        }
    }

    private void SetShieldActive(bool active)
    {
        if (_shieldActiveLocal == active)
        {
            return;
        }

        _shieldActiveLocal = active;
        BroadcastShieldState();
    }

    private void BroadcastShieldState()
    {
        if (Runner == null || Object == null)
        {
            return;
        }

        ShieldSyncPacket packet = new ShieldSyncPacket
        {
            networkId = Object.Id.ToString(),
            active = _shieldActiveLocal
        };

        string json = JsonUtility.ToJson(packet);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (player == Runner.LocalPlayer)
            {
                continue;
            }

            Runner.SendReliableDataToPlayer(player, ShieldSyncKey, payload);
        }
    }

    public static void HandleReliableDataReceived(NetworkRunner runner, ReliableKey key, ArraySegment<byte> data)
    {
        if (key != ShieldSyncKey || data.Count <= 0)
        {
            return;
            
        }

        string json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
        ShieldSyncPacket packet = JsonUtility.FromJson<ShieldSyncPacket>(json);
        if (packet == null || string.IsNullOrEmpty(packet.networkId))
        {
            return;
        }

        if (!SkillByNetworkId.TryGetValue(packet.networkId, out PlayerSkill skill) || skill == null)
        {
            return;
        }

        skill._shieldActiveLocal = packet.active;
        if (skill._runtimeShieldVisual != null)
        {
            skill._runtimeShieldVisual.SetActive(packet.active);
        }

    }

    void EnsureShieldVisualReference()
    {
        if (_runtimeShieldVisual != null)
            return;

        if (shieldVisual == null)
            return;

        // ❗ LUÔN instantiate
        _runtimeShieldVisual = Instantiate(shieldVisual);

        // ❗ LUÔN dùng transform trong scene
        Transform parent = transform;

        // ❗ nếu anchor hợp lệ thì dùng
        if (shieldVisualAnchor != null && shieldVisualAnchor.gameObject.scene.IsValid())
        {
            parent = shieldVisualAnchor;
        }

        _runtimeShieldVisual.transform.SetParent(parent, false);
        _runtimeShieldVisual.transform.localPosition = Vector3.zero;
        _runtimeShieldVisual.transform.localRotation = Quaternion.identity;
        _runtimeShieldVisual.SetActive(false);
    }

    private void UpdateShieldVisualFollow()
    {
        if (_runtimeShieldVisual == null)
        {
            return;
        }

        Transform target = transform;
        if (target == null)
        {
            return;
        }

        if (target.gameObject.scene.IsValid())
        {
            if (_runtimeShieldVisual.transform.parent != target)
            {
                _runtimeShieldVisual.transform.SetParent(target, false);
            }

            _runtimeShieldVisual.transform.localPosition = Vector3.zero;
            _runtimeShieldVisual.transform.localRotation = Quaternion.identity;
            return;
        }

        _runtimeShieldVisual.transform.SetPositionAndRotation(target.position, target.rotation);
    }
}
