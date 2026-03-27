<<<<<<< HEAD
﻿using Fusion;
using UnityEngine;

public class EffectDestroy : NetworkBehaviour
{
    public float life = 1.5f;
    float timer;
=======
﻿using UnityEngine;
using Fusion;

public class effectdestroy : NetworkBehaviour
{
    public float life;
    private float timer;
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4

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
<<<<<<< HEAD
}
=======
}
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
