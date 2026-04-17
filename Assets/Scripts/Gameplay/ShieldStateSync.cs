using Fusion;

public class ShieldStateSync : NetworkBehaviour
{
    [Networked]
    public NetworkBool ShieldActive { get; set; }

    public void SetShieldActive(bool active)
    {
        if (Object != null && Object.HasStateAuthority)
        {
            ShieldActive = active;
        }
    }
}
