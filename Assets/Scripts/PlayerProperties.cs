using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProperties : NetworkBehaviour
{
    private static readonly Dictionary<string, PlayerProperties> PropertiesByNetworkId = new Dictionary<string, PlayerProperties>();
    private static readonly ReliableKey NameRequestKey = ReliableKey.FromInts(88, 3, 0, 0);
    private static readonly ReliableKey NameSyncKey = ReliableKey.FromInts(88, 3, 0, 1);

    [Serializable]
    private class NamePacket
    {
        public string networkId;
        public string displayName;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private string defaultNamePrefix = "Player";
    [SerializeField] private int fallbackMaxHp = 100;
    [SerializeField] private bool faceCamera = true;

    private string _displayName;

    private Health _health;
    private Camera _cachedMainCamera;
    private int _lastHp = int.MinValue;
    private string _lastName;
    private string _networkId;

    public string NetworkId => _networkId;
    public string DisplayName => !string.IsNullOrWhiteSpace(_displayName) ? _displayName : BuildDefaultName();

    private void Awake()
    {
        BindReferences();
    }

    public override void Spawned()
    {
        BindReferences();

        if (Object != null)
        {
            _networkId = Object.Id.ToString();
            if (!string.IsNullOrEmpty(_networkId))
            {
                PropertiesByNetworkId[_networkId] = this;
            }
        }

        if (Object != null && Object.HasInputAuthority)
        {
            string localName = ResolveLocalName();
            _displayName = localName;
            SendNameRequest(localName);
        }

        RefreshName(true);
        RefreshHealth(true);
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(_networkId))
        {
            PropertiesByNetworkId.Remove(_networkId);
        }
    }

    private void LateUpdate()
    {
        if (faceCamera)
        {
            FaceMainCamera();
        }

        RefreshName(false);
        RefreshHealth(false);
    }

    private void BindReferences()
    {
        if (_health == null)
        {
            _health = GetComponent<Health>() ?? GetComponentInParent<Health>() ?? GetComponentInChildren<Health>(true);
        }

        if (playerNameText == null)
        {
            playerNameText = GetComponentInChildren<TMP_Text>(true);
        }

        if (healthFillImage == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                {
                    continue;
                }

                string n = images[i].gameObject.name.ToLowerInvariant();
                if (n.Contains("fore") || (n.Contains("health") && n.Contains("fill")))
                {
                    healthFillImage = images[i];
                    break;
                }
            }
        }
    }

    private void FaceMainCamera()
    {
        if (_cachedMainCamera == null)
        {
            _cachedMainCamera = Camera.main;
        }

        if (_cachedMainCamera == null)
        {
            return;
        }

        Vector3 toCamera = _cachedMainCamera.transform.position - transform.position;
        if (toCamera.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(-toCamera.normalized, Vector3.up);
    }

    private void RefreshName(bool force)
    {
        if (playerNameText == null)
        {
            return;
        }

        string resolvedName = _displayName;
        if (string.IsNullOrWhiteSpace(resolvedName))
        {
            resolvedName = BuildDefaultName();
        }

        if (!force && resolvedName == _lastName)
        {
            return;
        }

        playerNameText.text = resolvedName;
        _lastName = resolvedName;
    }

    private void RefreshHealth(bool force)
    {
        if (healthFillImage == null)
        {
            return;
        }

        if (_health == null)
        {
            BindReferences();
            if (_health == null)
            {
                return;
            }
        }

        int maxHp = _health != null ? _health.GetMaxHP() : Mathf.Max(1, fallbackMaxHp);
        int hp = Mathf.Clamp(_health.HP, 0, Mathf.Max(1, maxHp));

        if (!force && hp == _lastHp)
        {
            return;
        }

        healthFillImage.type = Image.Type.Filled;
        healthFillImage.fillMethod = Image.FillMethod.Horizontal;
        healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthFillImage.fillAmount = Mathf.Clamp01((float)hp / Mathf.Max(1, maxHp));

        _lastHp = hp;
    }

    private string ResolveLocalName()
    {
        string fromPlayerData = PlayerData.PlayerName != null ? PlayerData.PlayerName.Trim() : string.Empty;
        if (!string.IsNullOrWhiteSpace(fromPlayerData))
        {
            return SanitizeName(fromPlayerData);
        }

        return BuildDefaultName();
    }

    private string BuildDefaultName()
    {
        int id = 0;
        if (Object != null && Object.InputAuthority != PlayerRef.None)
        {
            id = Object.InputAuthority.PlayerId;
        }

        if (id <= 0 && Object != null)
        {
            uint rawId = Object.Id.Raw;
            id = rawId > int.MaxValue ? int.MaxValue : (int)rawId;
        }

        return $"{defaultNamePrefix} {Mathf.Max(1, id)}";
    }

    private string SanitizeName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return BuildDefaultName();
        }

        string trimmed = raw.Trim();
        if (trimmed.Length > 32)
        {
            trimmed = trimmed.Substring(0, 32);
        }

        return trimmed;
    }

    private void SendNameRequest(string incomingName)
    {
        if (Runner == null || Object == null)
        {
            return;
        }

        NamePacket packet = new NamePacket
        {
            networkId = Object.Id.ToString(),
            displayName = SanitizeName(incomingName)
        };

        if (Object.HasStateAuthority)
        {
            BroadcastNameSync(packet);
            return;
        }

        byte[] payload = EncodePacket(packet);
        PlayerRef authority = Object.StateAuthority;

        if (authority != PlayerRef.None)
        {
            Runner.SendReliableDataToPlayer(authority, NameRequestKey, payload);
            return;
        }

        Runner.SendReliableDataToServer(NameRequestKey, payload);
    }

    private void BroadcastNameSync(NamePacket packet)
    {
        if (Runner == null || packet == null)
        {
            return;
        }

        byte[] payload = EncodePacket(packet);
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (player == Runner.LocalPlayer)
            {
                continue;
            }

            Runner.SendReliableDataToPlayer(player, NameSyncKey, payload);
        }
    }

    private static byte[] EncodePacket(NamePacket packet)
    {
        string json = JsonUtility.ToJson(packet);
        return Encoding.UTF8.GetBytes(json);
    }

    private static void HandleNameRequestPacket(NetworkRunner runner, NamePacket packet)
    {
        if (packet == null || string.IsNullOrEmpty(packet.networkId))
        {
            return;
        }

        if (!PropertiesByNetworkId.TryGetValue(packet.networkId, out PlayerProperties target) || target == null)
        {
            return;
        }

        if (target.Object == null || !target.Object.HasStateAuthority)
        {
            return;
        }

        target._displayName = target.SanitizeName(packet.displayName);
        target.BroadcastNameSync(packet);
    }

    private static void HandleNameSyncPacket(NamePacket packet)
    {
        if (packet == null || string.IsNullOrEmpty(packet.networkId))
        {
            return;
        }

        if (!PropertiesByNetworkId.TryGetValue(packet.networkId, out PlayerProperties target) || target == null)
        {
            return;
        }

        target._displayName = target.SanitizeName(packet.displayName);
    }

    public static void HandleReliableDataReceived(NetworkRunner runner, ReliableKey key, ArraySegment<byte> data)
    {
        if (data.Count <= 0)
        {
            return;
        }

        if (key != NameRequestKey && key != NameSyncKey)
        {
            return;
        }

        string json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
        NamePacket packet = JsonUtility.FromJson<NamePacket>(json);

        if (key == NameRequestKey)
        {
            HandleNameRequestPacket(runner, packet);
            return;
        }

        HandleNameSyncPacket(packet);
    }
}
