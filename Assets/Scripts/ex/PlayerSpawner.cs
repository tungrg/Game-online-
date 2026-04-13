using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float spawnYOffset = 1.0f;

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner == null || playerPrefab == null)
        {
            return; 
        }

        bool shouldSpawn = false;

        // Host/Server modes: authoritative side spawns all players.
        if (Runner.IsServer)
        {
            shouldSpawn = true;
        }
        // Shared mode: each client spawns only its own player object.
        else if (Runner.GameMode == GameMode.Shared && player == Runner.LocalPlayer)
        {
            shouldSpawn = true;
        }

        if (!shouldSpawn)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        var obj = Runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        if (obj != null)
        {
            Runner.SetPlayerObject(player, obj);
        }

        var controller = obj.GetComponent<TankController>();
        if (controller != null && player == Runner.LocalPlayer)
        {
            // controller.PlayerName = PlayerData.PlayerName;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        return transform.position + Vector3.up * spawnYOffset;
    }
}
