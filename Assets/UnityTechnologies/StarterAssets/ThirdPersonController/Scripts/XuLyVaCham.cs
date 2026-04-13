using UnityEngine;
using Fusion;

public class XuLyVaCham : NetworkBehaviour
{
    
    void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!hit.gameObject.CompareTag("Player"))
                {
                    return;
                }
            NetworkObject otherNetObj = hit.gameObject.GetComponent<NetworkObject>();
            if(otherNetObj == null || otherNetObj == Object)
                {
                    return;
                }
            var health = otherNetObj.GetComponent<Health>();
            if (health != null)
                {
                    health.TakeDamage(10);
                }
        }
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_Despawn()
        {
            if (Object != null)
                Runner.Despawn(Object);
        }
     
}

