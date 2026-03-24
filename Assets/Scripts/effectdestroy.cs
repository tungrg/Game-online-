using UnityEngine;
using Fusion;

public class effectdestroy : NetworkBehaviour
{
    public float life;
    private float timer;

    public override void Spawned()
    {
        timer = life;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        timer -= Runner.DeltaTime;

        if (timer <= 0f)
        {
            Runner.Despawn(Object);
        }
    }
}
