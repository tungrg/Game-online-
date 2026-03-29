using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            var obj = Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);

            var controller = obj.GetComponent<TankController>();
            controller.RPC_SetName(PlayerData.PlayerName);
        }
    }
}
