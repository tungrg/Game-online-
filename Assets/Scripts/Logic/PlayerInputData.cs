using Fusion;

public struct PlayerInputData : INetworkInput
{
    public float moveX;
    public float moveY;
    public float aimX;
    public float aimZ;
    public NetworkBool hasAim;
    public NetworkBool isShooting;
    public NetworkBool isShield;
}