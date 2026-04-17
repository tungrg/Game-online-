using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class Health : NetworkBehaviour
{
    private static readonly Dictionary<string, Health> HealthByNetworkId = new Dictionary<string, Health>();
    
    public GameObject deathEffect;
    [SerializeField] private bool spawnNetworkEffectsInMultiplayer = false;

    private static readonly ReliableKey DamageRequestKey = ReliableKey.FromInts(88, 2, 0, 0);
    private static readonly ReliableKey VitalsSyncKey = ReliableKey.FromInts(88, 2, 0, 1);

    [Serializable]
    private class DamageRequestPacket
    {
        public string networkId;
        public int damage;
        public string sourceNetworkId;
    }

    [Serializable]
    private class VitalsSyncPacket
    {
        public string networkId;
        public int hp;
        public int mana;
    }

    public int HP = 100;
    public int Mana = 30;

    [SerializeField] private int startHP = 100;
    

    public int Team = -1;

    private PlayerInput playerInput;
    private string _networkId;
    private string _lastDamageSourceNetworkId;

    public string NetworkId => _networkId;

    public override void Spawned()
    {
        if (Object != null)
        {
            _networkId = Object.Id.ToString();
            if (!string.IsNullOrEmpty(_networkId))
            {
                HealthByNetworkId[_networkId] = this;
            }
        }

        if (Object.HasStateAuthority)
        {
            HP = startHP;
            Mana = 30;
            BroadcastVitalsState();
        }

        playerInput = GetComponent<PlayerInput>();
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(_networkId))
        {
            HealthByNetworkId.Remove(_networkId);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // 🔥 Host handles death state
        if (Object.HasStateAuthority && HP <= 0)
        {
            if (deathEffect != null && (spawnNetworkEffectsInMultiplayer || Runner == null || !Runner.IsRunning))
            {
                Runner.Spawn(deathEffect, transform.position, Quaternion.identity);
            }
            Runner.Despawn(Object);
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, (string)null);
    }

    public void TakeDamage(int damage, Health source)
    {
        TakeDamage(damage, source != null ? source.NetworkId : null);
    }

    public void TakeDamage(int damage, string sourceNetworkId)
    {
        if (damage <= 0 || Object == null)
        {
            return;
        }

        if (Object.HasStateAuthority)
        {
            ApplyDamage(damage, sourceNetworkId);
            return;
        }

        SendDamageRequest(damage, sourceNetworkId);
    }

    void ApplyDamage(int damage, string sourceNetworkId)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        int previousHp = HP;

        if (!string.IsNullOrEmpty(sourceNetworkId))
        {
            _lastDamageSourceNetworkId = sourceNetworkId;
        }

        HP = Mathf.Max(0, HP - damage);
        BroadcastVitalsState();

        if (previousHp > 0 && HP <= 0)
        {
            Leaderboard.ReportEnemyKilled(_lastDamageSourceNetworkId, this);
        }
    }

    public int GetMaxHP()
    {
        return Mathf.Max(1, startHP);
    }

    void BroadcastVitalsState()
    {
        if (Runner == null || Object == null || !Object.HasStateAuthority)
        {
            return;
        }

        VitalsSyncPacket packet = new VitalsSyncPacket
        {
            networkId = Object.Id.ToString(),
            hp = HP,
            mana = Mana
        };

        SendVitalsPacketToOtherPlayers(Runner, packet);
    }

    static void SendVitalsPacketToOtherPlayers(NetworkRunner runner, VitalsSyncPacket packet)
    {
        if (runner == null || packet == null)
        {
            return;
        }

        string json = JsonUtility.ToJson(packet);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (player == runner.LocalPlayer)
            {
                continue;
            }

            runner.SendReliableDataToPlayer(player, VitalsSyncKey, payload);
        }
    }

    static void HandleVitalsSyncPacket(VitalsSyncPacket packet)
    {
        if (packet == null || string.IsNullOrEmpty(packet.networkId))
        {
            return;
        }

        Health target = FindHealthById(packet.networkId);
        if (target == null)
        {
            return;
        }

        target.HP = Mathf.Max(0, packet.hp);
        target.Mana = Mathf.Max(0, packet.mana);
    }

    void SendDamageRequest(int damage, string sourceNetworkId)
    {
        if (Runner == null || Object == null || damage <= 0)
        {
            return;
        }

        DamageRequestPacket packet = new DamageRequestPacket
        {
            networkId = Object.Id.ToString(),
            damage = damage,
            sourceNetworkId = sourceNetworkId
        };

        string json = JsonUtility.ToJson(packet);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        if (Runner.IsServer)
        {
            HandleDamageRequestPacket(packet);
            return;
        }

        PlayerRef owner = Object.StateAuthority;
        if (owner != PlayerRef.None)
        {
            Runner.SendReliableDataToPlayer(owner, DamageRequestKey, payload);
            return;
        }

        Runner.SendReliableDataToServer(DamageRequestKey, payload);
    }

    static void HandleDamageRequestPacket(DamageRequestPacket packet)
    {
        if (packet == null || string.IsNullOrEmpty(packet.networkId) || packet.damage <= 0)
        {
            return;
        }

        Health target = FindHealthById(packet.networkId);
        if (target == null || target.Object == null || !target.Object.HasStateAuthority)
        {
            return;
        }

        target.ApplyDamage(packet.damage, packet.sourceNetworkId);
    }

    static Health FindHealthById(string networkId)
    {
        if (string.IsNullOrEmpty(networkId))
        {
            return null;
        }

        HealthByNetworkId.TryGetValue(networkId, out Health health);
        return health;
    }

    public static bool TryGetHealthByNetworkId(string networkId, out Health health)
    {
        health = FindHealthById(networkId);
        return health != null;
    }

    public static void HandleReliableDataReceived(NetworkRunner runner, ReliableKey key, ArraySegment<byte> data)
    {
        if (data.Count <= 0)
        {
            return;
        }

        string json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);

        if (key == DamageRequestKey)
        {
            DamageRequestPacket damagePacket = JsonUtility.FromJson<DamageRequestPacket>(json);
            HandleDamageRequestPacket(damagePacket);
            return;
        }

        if (key == VitalsSyncKey)
        {
            VitalsSyncPacket vitalsPacket = JsonUtility.FromJson<VitalsSyncPacket>(json);
            HandleVitalsSyncPacket(vitalsPacket);
        }
    }
}