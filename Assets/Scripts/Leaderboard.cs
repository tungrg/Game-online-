using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    private static readonly ReliableKey ScoreSyncKey = ReliableKey.FromInts(88, 4, 0, 0);
    private static Leaderboard _instance;

    [Serializable]
    private class ScoreSyncPacket
    {
        public string playerRowKey;
        public string playerName;
        public int score;
    }

    private class RowData
    {
        public string playerRowKey;
        public string playerName;
        public int score;
        public int joinOrder;
        public GameObject rowObject;
        public LeaderboardEntryUI rowUI;
    }

    [Serializable]
    public class SnapshotEntry
    {
        public string playerRowKey;
        public string playerName;
        public int score;
        public int rank;
    }

    [Header("UI")]
    [SerializeField] private Transform rowsParent;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private float refreshPlayerListEvery = 0.5f;
    [SerializeField] private float fullSyncEvery = 1.5f;

    [Header("Score")]
    [SerializeField] private int defaultEnemyKillScore = 10;

    private readonly Dictionary<string, RowData> _rowsByPlayerId = new Dictionary<string, RowData>();
    private int _joinCounter;
    private float _nextRefreshAt;
    private float _nextFullSyncAt;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        if (Time.time < _nextRefreshAt)
        {
            return;
        }

        _nextRefreshAt = Time.time + Mathf.Max(0.1f, refreshPlayerListEvery);
        SyncPlayersFromScene();
        RefreshRowsVisual();
        BroadcastFullSnapshotIfNeeded();
    }

    private void BroadcastFullSnapshotIfNeeded()
    {
        if (Time.time < _nextFullSyncAt)
        {
            return;
        }

        _nextFullSyncAt = Time.time + Mathf.Max(0.25f, fullSyncEvery);

        NetworkRunner runner = FindRunner();
        if (runner == null || !runner.IsServer)
        {
            return;
        }

        foreach (RowData row in _rowsByPlayerId.Values)
        {
            if (row == null || string.IsNullOrEmpty(row.playerRowKey))
            {
                continue;
            }

            BroadcastScore(runner, row);
        }
    }

    public static void ReportEnemyKilled(string killerNetworkId, Health victim)
    {
        if (_instance == null)
        {
            return;
        }

        _instance.TryReportEnemyKillInternal(killerNetworkId, victim);
    }

    public static List<SnapshotEntry> GetSnapshot()
    {
        if (_instance == null)
        {
            return new List<SnapshotEntry>();
        }

        return _instance.BuildSnapshot();
    }

    private void TryReportEnemyKillInternal(string killerNetworkId, Health victim)
    {
        if (victim == null || string.IsNullOrEmpty(killerNetworkId))
        {
            return;
        }

        EnemyShooting enemy = victim.GetComponentInParent<EnemyShooting>() ?? victim.GetComponentInChildren<EnemyShooting>();
        EnemyScoreValue scoreValue = victim.GetComponentInParent<EnemyScoreValue>() ?? victim.GetComponentInChildren<EnemyScoreValue>();
        TankController playerController = victim.GetComponentInParent<TankController>() ?? victim.GetComponentInChildren<TankController>();
        bool isEnemy = scoreValue != null || enemy != null || (playerController == null && victim.Team == 1);
        if (!isEnemy)
        {
            return;
        }

        NetworkRunner runner = victim.Runner;
        if (runner == null)
        {
            return;
        }

        if (victim.Object == null || !victim.Object.HasStateAuthority)
        {
            return;
        }

        int points = scoreValue != null ? Mathf.Max(0, scoreValue.KillScore) : Mathf.Max(0, defaultEnemyKillScore);
        if (points <= 0)
        {
            return;
        }

        string ownerPlayerKey = ResolvePlayerRowKey(killerNetworkId);
        AddScore(ownerPlayerKey, points, runner);
    }

    private string ResolvePlayerRowKey(string killerNetworkId)
    {
        if (string.IsNullOrEmpty(killerNetworkId))
        {
            return killerNetworkId;
        }

        if (_rowsByPlayerId.ContainsKey(killerNetworkId))
        {
            return killerNetworkId;
        }

        PlayerProperties[] players = FindObjectsByType<PlayerProperties>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            PlayerProperties p = players[i];
            if (p == null)
            {
                continue;
            }

            string rowKey = BuildPlayerRowKey(p);
            if (p.NetworkId == killerNetworkId || rowKey == killerNetworkId)
            {
                return rowKey;
            }
        }

        if (!Health.TryGetHealthByNetworkId(killerNetworkId, out Health killerHealth) || killerHealth == null || killerHealth.Object == null)
        {
            return killerNetworkId;
        }

        PlayerRef killerInputAuthority = killerHealth.Object.InputAuthority;
        PlayerRef killerStateAuthority = killerHealth.Object.StateAuthority;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerProperties p = players[i];
            if (p == null || p.Object == null)
            {
                continue;
            }

            if (killerInputAuthority != PlayerRef.None && p.Object.InputAuthority == killerInputAuthority)
            {
                return BuildPlayerRowKey(p);
            }

            if (killerStateAuthority != PlayerRef.None && p.Object.StateAuthority == killerStateAuthority)
            {
                return BuildPlayerRowKey(p);
            }
        }

        return killerNetworkId;
    }

    private void AddScore(string playerRowKey, int scoreToAdd, NetworkRunner runner)
    {
        if (string.IsNullOrEmpty(playerRowKey) || scoreToAdd <= 0)
        {
            return;
        }

        RowData row = GetOrCreateRow(playerRowKey);
        if (row == null)
        {
            return;
        }

        row.score = Mathf.Max(0, row.score + scoreToAdd);
        BroadcastScore(runner, row);
        RefreshRowsVisual();
    }

    private void BroadcastScore(NetworkRunner runner, RowData row)
    {
        if (row == null || string.IsNullOrEmpty(row.playerRowKey))
        {
            return;
        }

        if (runner == null)
        {
            runner = FindRunner();
        }

        if (runner == null)
        {
            return;
        }

        ScoreSyncPacket packet = new ScoreSyncPacket
        {
            playerRowKey = row.playerRowKey,
            playerName = row.playerName,
            score = Mathf.Max(0, row.score)
        };

        byte[] payload = Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet));
        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (player == runner.LocalPlayer)
            {
                continue;
            }

            runner.SendReliableDataToPlayer(player, ScoreSyncKey, payload);
        }
    }

    private void SyncPlayersFromScene()
    {
        NetworkRunner runner = FindRunner();
        bool isServer = runner == null || runner.IsServer;

        PlayerProperties[] players = FindObjectsByType<PlayerProperties>(FindObjectsSortMode.None);
        if (players == null || players.Length == 0)
        {
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            PlayerProperties p = players[i];
            if (p == null)
            {
                continue;
            }

            string playerRowKey = BuildPlayerRowKey(p);
            if (string.IsNullOrEmpty(playerRowKey))
            {
                continue;
            }

            RowData row = GetOrCreateRow(playerRowKey);
            if (row == null)
            {
                continue;
            }

            string sceneName = p.DisplayName;
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                continue;
            }

            if (isServer)
            {
                row.playerName = sceneName;
                continue;
            }

            bool rowHasFallback = IsFallbackName(row.playerName);
            bool sceneHasFallback = IsFallbackName(sceneName);

            // On clients, never let a fallback scene name overwrite a real synced name.
            if (!sceneHasFallback || rowHasFallback || string.IsNullOrWhiteSpace(row.playerName))
            {
                row.playerName = sceneName;
            }
        }
    }

    private static bool IsFallbackName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return true;
        }

        return name.TrimStart().StartsWith("Player ", StringComparison.OrdinalIgnoreCase);
    }

    private RowData GetOrCreateRow(string playerRowKey)
    {
        if (string.IsNullOrEmpty(playerRowKey))
        {
            return null;
        }

        if (_rowsByPlayerId.TryGetValue(playerRowKey, out RowData existing) && existing != null)
        {
            return existing;
        }

        RowData created = new RowData
        {
            playerRowKey = playerRowKey,
            playerName = $"Player {_joinCounter + 1}",
            score = 0,
            joinOrder = _joinCounter++
        };

        if (rowsParent != null && rowPrefab != null)
        {
            created.rowObject = Instantiate(rowPrefab, rowsParent);
            created.rowUI = created.rowObject.GetComponent<LeaderboardEntryUI>();
        }

        _rowsByPlayerId[playerRowKey] = created;
        return created;
    }

    private string BuildPlayerRowKey(PlayerProperties player)
    {
        if (player == null)
        {
            return string.Empty;
        }

        if (player.Object != null)
        {
            PlayerRef input = player.Object.InputAuthority;
            if (input != PlayerRef.None)
            {
                return $"player:{input.PlayerId}";
            }

            PlayerRef state = player.Object.StateAuthority;
            if (state != PlayerRef.None)
            {
                return $"state:{state.PlayerId}";
            }
        }

        return player.NetworkId;
    }

    private void RefreshRowsVisual()
    {
        List<RowData> rows = _rowsByPlayerId.Values
            .Where(r => r != null)
            .OrderByDescending(r => r.score)
            .ThenBy(r => r.joinOrder)
            .ToList();

        for (int i = 0; i < rows.Count; i++)
        {
            RowData row = rows[i];
            if (row == null || row.rowObject == null)
            {
                continue;
            }

            row.rowObject.transform.SetSiblingIndex(i);
            int rank = i + 1;
            string safeName = string.IsNullOrWhiteSpace(row.playerName) ? "Player" : row.playerName;

            if (row.rowUI != null)
            {
                row.rowUI.SetData(rank, safeName, row.score);
            }
        }

        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(rows.Count == 0);
        }
    }

    private List<SnapshotEntry> BuildSnapshot()
    {
        List<RowData> rows = _rowsByPlayerId.Values
            .Where(r => r != null)
            .OrderByDescending(r => r.score)
            .ThenBy(r => r.joinOrder)
            .ToList();

        List<SnapshotEntry> snapshot = new List<SnapshotEntry>(rows.Count);
        for (int i = 0; i < rows.Count; i++)
        {
            RowData row = rows[i];
            snapshot.Add(new SnapshotEntry
            {
                playerRowKey = row.playerRowKey,
                playerName = string.IsNullOrWhiteSpace(row.playerName) ? "Player" : row.playerName,
                score = Mathf.Max(0, row.score),
                rank = i + 1
            });
        }

        return snapshot;
    }

    private NetworkRunner FindRunner()
    {
        return FindAnyObjectByType<NetworkRunner>();
    }

    public static void HandleReliableDataReceived(NetworkRunner runner, ReliableKey key, ArraySegment<byte> data)
    {
        if (_instance == null || key != ScoreSyncKey || data.Count <= 0)
        {
            return;
        }

        string json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
        ScoreSyncPacket packet = JsonUtility.FromJson<ScoreSyncPacket>(json);
        if (packet == null || string.IsNullOrEmpty(packet.playerRowKey))
        {
            return;
        }

        _instance.ApplyScoreSyncPacket(packet);
    }

    private void ApplyScoreSyncPacket(ScoreSyncPacket packet)
    {
        RowData row = GetOrCreateRow(packet.playerRowKey);
        if (row == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(packet.playerName))
        {
            row.playerName = packet.playerName;
        }

        row.score = Mathf.Max(0, packet.score);
        RefreshRowsVisual();
    }
}
