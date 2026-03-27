using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerPrefab;
<<<<<<< HEAD

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
=======
    public void PlayerJoined(PlayerRef player)
    {
        if(player==Runner.LocalPlayer)
        {
            Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
        }
    }
   
}
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
