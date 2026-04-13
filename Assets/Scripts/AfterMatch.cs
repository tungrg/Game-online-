using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fusion;
using Fusion.Sockets;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class AfterMatch : MonoBehaviour
{
    private static readonly ReliableKey MatchEndKey = ReliableKey.FromInts(88, 5, 0, 0);
    private static AfterMatch _instance;

    [Serializable]
    private class MatchEndPacket
    {
        public bool isWin;
        public string endedAtUtc;
    }
    [Header("Match End")]
    [SerializeField] private float checkInterval = 0.5f;
    [SerializeField] private float shutdownDelayAfterEnd = 2f;
    [SerializeField] private bool waitForPlayFabBeforeShutdown = true;
    [SerializeField] private float playFabSubmitTimeout = 10f;

    [Header("PlayFab")]
    [SerializeField] private bool autoLoginPlayFab = true;
    [SerializeField] private string scoreStatisticName = "TotalScore";
    [SerializeField] private string rankStatisticName = "LastMatchRank";
    [SerializeField] private string summaryDataKey = "LastMatchLeaderboard";
    [SerializeField] private int statsConflictRetryMax = 6;
    [SerializeField] private float statsConflictRetryDelay = 0.5f;

    private bool _matchEnded;
    private bool _isWin;
    private bool _submittedToPlayFab;
    private bool _playFabSubmitStarted;
    private bool _playFabLoginAttempted;
    private bool _playFabLoginSucceeded;
    private bool _shutdownFlowStarted;
    private bool _titleIdWarningLogged;
    private int _pendingPlayFabCalls;
    private bool _statsRequestFinished;
    private bool _summaryRequestFinished;
    private bool _seenAnyPlayers;
    private bool _seenAnyEnemies;
    private float _nextCheckAt;
    private string _cachedLocalRowKey;
    private bool _clientSubmitFlowStarted;
    private int _statsConflictRetryCount;
    private string _lastResolvedCustomId;

    private void Awake()
    {
        _instance = this;
        _nextCheckAt = Time.time + Mathf.Max(0.1f, checkInterval);

        if (string.IsNullOrEmpty(_cachedLocalRowKey))
        {
            string key = ResolveLocalPlayerRowKey();
            if (!string.IsNullOrEmpty(key))
            {
                _cachedLocalRowKey = key;
            }
        }
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
        if (Time.time < _nextCheckAt)
        {
            return;
        }

        _nextCheckAt = Time.time + Mathf.Max(0.1f, checkInterval);

        if (autoLoginPlayFab)
        {
            TryEnsurePlayFabLogin();
        }

        if (!_matchEnded)
        {
            EvaluateMatchEndCondition();
        }

        if (_matchEnded)
        {
            TrySubmitResultToPlayFab();
        }
    }

    private void EvaluateMatchEndCondition()
    {
        int alivePlayers = CountAlivePlayers(out int totalPlayers);
        int aliveEnemies = CountAliveEnemies(out int totalEnemies);

        if (totalPlayers > 0)
        {
            _seenAnyPlayers = true;
        }

        if (totalEnemies > 0)
        {
            _seenAnyEnemies = true;
        }

        bool allPlayersDead = _seenAnyPlayers && alivePlayers <= 0;
        bool allEnemiesDead = _seenAnyEnemies && aliveEnemies <= 0;

        if (!allPlayersDead && !allEnemiesDead)
        {
            return;
        }

        _matchEnded = true;
        _isWin = allEnemiesDead && !allPlayersDead;

        NetworkRunner runner = FindAnyObjectByType<NetworkRunner>();
        if (!_clientSubmitFlowStarted)
        {
            _clientSubmitFlowStarted = true;
            StartCoroutine(ClientSubmitFlow());
        }

        if (runner != null && runner.IsServer && !_shutdownFlowStarted)
        {
            BroadcastMatchEnded(runner, _isWin);
            _shutdownFlowStarted = true;
            StartCoroutine(ShutdownRunnerFlow(runner));
        }
    }

    private IEnumerator ClientSubmitFlow()
    {
        float timeoutAt = Time.time + Mathf.Max(1f, playFabSubmitTimeout);
        while (Time.time < timeoutAt && !_submittedToPlayFab)
        {
            if (autoLoginPlayFab)
            {
                TryEnsurePlayFabLogin();
            }

            TrySubmitResultToPlayFab();
            yield return null;
        }
    }

    private void BroadcastMatchEnded(NetworkRunner runner, bool isWin)
    {
        if (runner == null)
        {
            return;
        }

        MatchEndPacket packet = new MatchEndPacket
        {
            isWin = isWin,
            endedAtUtc = DateTime.UtcNow.ToString("o")
        };

        byte[] payload = Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet));
        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (player == runner.LocalPlayer)
            {
                continue;
            }

            runner.SendReliableDataToPlayer(player, MatchEndKey, payload);
        }
    }

    private void ApplyRemoteMatchEnded(MatchEndPacket packet)
    {
        if (packet == null)
        {
            return;
        }

        _matchEnded = true;
        _isWin = packet.isWin;

        if (!_clientSubmitFlowStarted)
        {
            _clientSubmitFlowStarted = true;
            StartCoroutine(ClientSubmitFlow());
        }
    }

    public static void HandleReliableDataReceived(NetworkRunner runner, ReliableKey key, ArraySegment<byte> data)
    {
        if (_instance == null || key != MatchEndKey || data.Count <= 0)
        {
            return;
        }

        string json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
        MatchEndPacket packet = JsonUtility.FromJson<MatchEndPacket>(json);
        _instance.ApplyRemoteMatchEnded(packet);
    }

    private IEnumerator ShutdownRunnerFlow(NetworkRunner runner)
    {
        float startedAt = Time.time;
        float minWaitUntil = startedAt + Mathf.Max(0.25f, shutdownDelayAfterEnd);
        float timeoutAt = startedAt + Mathf.Max(1f, playFabSubmitTimeout);

        while (Time.time < timeoutAt)
        {
            if (autoLoginPlayFab)
            {
                TryEnsurePlayFabLogin();
            }

            TrySubmitResultToPlayFab();

            bool reachedMinimumDelay = Time.time >= minWaitUntil;
            bool doneSubmitting = _submittedToPlayFab || !waitForPlayFabBeforeShutdown;
            if (reachedMinimumDelay && doneSubmitting)
            {
                break;
            }

            yield return null;
        }

        if (runner != null && runner.IsRunning)
        {
            runner.Shutdown();
        }
    }

    private int CountAlivePlayers(out int totalPlayers)
    {
        totalPlayers = 0;
        int alive = 0;

        IReadOnlyList<TankController> tanks = TankController.GetActiveTanks();
        for (int i = 0; i < tanks.Count; i++)
        {
            TankController tank = tanks[i];
            if (tank == null)
            {
                continue;
            }

            totalPlayers++;
            Health health = tank.GetComponent<Health>();
            if (health == null || health.HP > 0)
            {
                alive++;
            }
        }

        return alive;
    }

    private int CountAliveEnemies(out int totalEnemies)
    {
        totalEnemies = 0;
        int alive = 0;

        EnemyShooting[] enemies = FindObjectsByType<EnemyShooting>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyShooting enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }

            totalEnemies++;
            Health health = enemy.GetComponent<Health>() ?? enemy.GetComponentInParent<Health>();
            if (health == null || health.HP > 0)
            {
                alive++;
            }
        }

        return alive;
    }

    private void TryEnsurePlayFabLogin()
    {
        if (_playFabLoginSucceeded || _playFabLoginAttempted)
        {
            return;
        }

        if (PlayFabSettings.staticPlayer != null && PlayFabSettings.staticPlayer.IsClientLoggedIn())
        {
            _playFabLoginSucceeded = true;
            return;
        }

        string titleId = PlayFabSettings.staticSettings != null ? PlayFabSettings.staticSettings.TitleId : null;
        if (string.IsNullOrWhiteSpace(titleId))
        {
            if (!_titleIdWarningLogged)
            {
                _titleIdWarningLogged = true;
                Debug.LogWarning("AfterMatch: PlayFab TitleId is empty, skipping PlayFab upload.");
            }

            _submittedToPlayFab = true;
            return;
        }

        _playFabLoginAttempted = true;
        string customId = GetLocalPlayFabCustomId();
        _lastResolvedCustomId = customId;
        Debug.Log($"AfterMatch: PlayFab login request customId={customId}");
        PlayFabClientAPI.LoginWithCustomID(
            new LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = true
            },
            result =>
            {
                _playFabLoginSucceeded = true;
                Debug.Log("PlayFab login success for AfterMatch sync.");
            },
            error =>
            {
                _playFabLoginAttempted = false;
                Debug.LogWarning($"PlayFab login failed: {error.GenerateErrorReport()}");
            }
        );
    }

    private string GetLocalPlayFabCustomId()
    {
        string playerName = PlayerData.PlayerName != null ? PlayerData.PlayerName.Trim() : string.Empty;
        string safeName = string.IsNullOrWhiteSpace(playerName) ? "anon" : playerName.ToLowerInvariant().Replace(" ", "_");
        string identityScope = ResolveCustomIdScope();
        string key = $"aftermatch_playfab_custom_id_{safeName}_{identityScope}";
        string existing = PlayerPrefs.GetString(key, string.Empty);
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        string seed = SystemInfo.deviceUniqueIdentifier;
        if (string.IsNullOrWhiteSpace(seed) || seed == SystemInfo.unsupportedIdentifier)
        {
            seed = Guid.NewGuid().ToString("N");
        }

        string customId = $"{Application.productName}_{safeName}_{identityScope}_{seed}";
        PlayerPrefs.SetString(key, customId);
        PlayerPrefs.Save();
        return customId;
    }

    private string ResolveCustomIdScope()
    {
        string rowKey = !string.IsNullOrWhiteSpace(_cachedLocalRowKey) ? _cachedLocalRowKey : ResolveLocalPlayerRowKey();
        if (!string.IsNullOrWhiteSpace(rowKey))
        {
            return SanitizeForId(rowKey);
        }

        NetworkRunner runner = FindAnyObjectByType<NetworkRunner>();
        if (runner != null && runner.LocalPlayer != PlayerRef.None)
        {
            return $"lp_{runner.LocalPlayer.PlayerId}";
        }

        int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
        return $"proc_{processId}";
    }

    private static string SanitizeForId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        string trimmed = value.Trim();
        StringBuilder sb = new StringBuilder(trimmed.Length);
        for (int i = 0; i < trimmed.Length; i++)
        {
            char c = trimmed[i];
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
            {
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append('_');
            }
        }

        return sb.ToString();
    }

    private void TrySubmitResultToPlayFab()
    {
        if (_submittedToPlayFab)
        {
            return;
        }

        if (_playFabSubmitStarted)
        {
            return;
        }

        if (PlayFabSettings.staticPlayer == null || !PlayFabSettings.staticPlayer.IsClientLoggedIn())
        {
            return;
        }

        List<Leaderboard.SnapshotEntry> snapshot = Leaderboard.GetSnapshot();
       if (snapshot == null || snapshot.Count == 0)
        {
            Debug.LogWarning("Snapshot empty → vẫn gửi default score");

            snapshot = new List<Leaderboard.SnapshotEntry>();
        }

        _playFabSubmitStarted = true;
        _pendingPlayFabCalls = 0;
        _statsRequestFinished = false;
        _summaryRequestFinished = false;
        _statsConflictRetryCount = 0;

        string localRowKey = !string.IsNullOrEmpty(_cachedLocalRowKey) ? _cachedLocalRowKey : ResolveLocalPlayerRowKey();
        Leaderboard.SnapshotEntry localEntry = null;
        for (int i = 0; i < snapshot.Count; i++)
        {
            if (snapshot[i] != null && snapshot[i].playerRowKey == localRowKey)
            {
                localEntry = snapshot[i];
                break;
            }
        }

        if (localEntry == null)
        {
            string localName = PlayerData.PlayerName != null ? PlayerData.PlayerName.Trim() : string.Empty;
            if (!string.IsNullOrWhiteSpace(localName))
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if (snapshot[i] == null || string.IsNullOrWhiteSpace(snapshot[i].playerName))
                    {
                        continue;
                    }

                    if (string.Equals(snapshot[i].playerName.Trim(), localName, StringComparison.OrdinalIgnoreCase))
                    {
                        localEntry = snapshot[i];
                        break;
                    }
                }
            }
        }

        int score = localEntry != null ? localEntry.score : 100;
        int rank = localEntry != null ? localEntry.rank : 1;

        string playFabId = PlayFabSettings.staticPlayer != null ? PlayFabSettings.staticPlayer.PlayFabId : "unknown";
        string loginName = PlayerData.PlayerName != null ? PlayerData.PlayerName.Trim() : string.Empty;
        Debug.Log($"AfterMatch submit -> name:{loginName}, playFabId:{playFabId}, rowKey:{localRowKey}, score:{score}, rank:{rank}");

        SubmitStatisticsWithRetry(score, rank, playFabId);

        string jsonSummary = JsonUtility.ToJson(new MatchSummaryPayload
        {
            isWin = _isWin,
            timestampUtc = DateTime.UtcNow.ToString("o"),
            entries = snapshot.ToArray()
        });

        _pendingPlayFabCalls++;
        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { summaryDataKey, jsonSummary },
                    { "LastMatchResult", _isWin ? "WIN" : "LOSE" }
                }
            },
            result =>
            {
                Debug.Log("AfterMatch: uploaded match summary to PlayFab.");
                _summaryRequestFinished = true;
                OnPlayFabCallFinished();
            },
            error =>
            {
                Debug.LogWarning($"UpdateUserData failed: {error.GenerateErrorReport()}");
                _summaryRequestFinished = true;
                OnPlayFabCallFinished();
            }
        );
    }

    private void SubmitStatisticsWithRetry(int score, int rank, string playFabId)
    {
        var stats = new List<StatisticUpdate>
        {
            new StatisticUpdate { StatisticName = scoreStatisticName, Value = score },
            new StatisticUpdate { StatisticName = rankStatisticName, Value = rank }
        };

        _pendingPlayFabCalls++;

        PlayFabClientAPI.UpdatePlayerStatistics(
            new UpdatePlayerStatisticsRequest { Statistics = stats },
            result =>
            {
                Debug.Log($"AfterMatch: UpdatePlayerStatistics success for {playFabId}");
                _statsRequestFinished = true;
                OnPlayFabCallFinished();
            },
            error =>
            {
                if (error != null && error.HttpCode == 409 && _statsConflictRetryCount < Mathf.Max(0, statsConflictRetryMax))
                {
                    _statsConflictRetryCount++;
                    Debug.LogWarning(
                        $"AfterMatch: UpdatePlayerStatistics conflict retry {_statsConflictRetryCount}/{statsConflictRetryMax} for {playFabId}. " +
                        $"Http:{error.HttpCode}, Code:{error.Error}, Message:{error.ErrorMessage}"
                    );

                    // This attempt is complete; keep stats unfinished and retry shortly.
                    _pendingPlayFabCalls = Mathf.Max(0, _pendingPlayFabCalls - 1);
                    StartCoroutine(RetryStatisticsSubmit(score, rank, playFabId));
                    return;
                }

                Debug.LogError(
                    "AfterMatch: UpdatePlayerStatistics failed. " +
                    $"Http:{error?.HttpCode}, Code:{error?.Error}, Message:{error?.ErrorMessage}, Report:{error?.GenerateErrorReport()}"
                );
                _statsRequestFinished = true;
                OnPlayFabCallFinished();
            }
        );
    }

    private IEnumerator RetryStatisticsSubmit(int score, int rank, string playFabId)
    {
        float expDelay = statsConflictRetryDelay * Mathf.Pow(2f, Mathf.Max(0, _statsConflictRetryCount - 1));
        float jitter = UnityEngine.Random.Range(0f, 0.35f);
        float delay = Mathf.Clamp(expDelay + jitter, 0.1f, 8f);
        yield return new WaitForSeconds(delay);
        SubmitStatisticsWithRetry(score, rank, playFabId);
    }

    private void OnPlayFabCallFinished()
    {
        _pendingPlayFabCalls = Mathf.Max(0, _pendingPlayFabCalls - 1);
        if (_pendingPlayFabCalls > 0)
        {
            return;
        }

        if (_statsRequestFinished && _summaryRequestFinished)
        {
            _submittedToPlayFab = true;
        }
    }

    private string ResolveLocalPlayerRowKey()
    {
        PlayerProperties[] players = FindObjectsByType<PlayerProperties>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            PlayerProperties p = players[i];
            if (p == null || p.Object == null || !p.Object.HasInputAuthority)
            {
                continue;
            }

            PlayerRef input = p.Object.InputAuthority;
            if (input != PlayerRef.None)
            {
                return $"player:{input.PlayerId}";
            }

            PlayerRef state = p.Object.StateAuthority;
            if (state != PlayerRef.None)
            {
                return $"state:{state.PlayerId}";
            }

            return p.NetworkId;
        }

        return string.Empty;
    }

    [Serializable]
    private class MatchSummaryPayload
    {
        public bool isWin;
        public string timestampUtc;
        public Leaderboard.SnapshotEntry[] entries;
    }
}
