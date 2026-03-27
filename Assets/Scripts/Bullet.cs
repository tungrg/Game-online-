using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public int Team { get; set; }

    float lifeTime = 3f;
    public GameObject effect;
    public float speed = 10f;
    Vector3 prevPos;

public override void Spawned()
{
    prevPos = transform.position;
}

    public override void FixedUpdateNetwork()
    {
        // 🔥 CHỈ host điều khiển
        if (Object.HasStateAuthority)
        {
            transform.position += transform.forward * speed * Runner.DeltaTime;

            lifeTime -= Runner.DeltaTime;
            if (lifeTime <= 0f)
            {
                Runner.Despawn(Object);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
<<<<<<< HEAD
        if (!Object.HasStateAuthority) return;
=======
        if (!Object.HasStateAuthority) return; // Ensure only the state authority handles this logic
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4

        var hit = other.GetComponent<Health>();

        if (other.CompareTag("Wall"))
        {
            Runner.Spawn(effect, transform.position, Quaternion.identity);
            Runner.Despawn(Object);
            return;
        }

        if (hit != null)
        {
            if (hit.Team == Team)
<<<<<<< HEAD
                return;

            hit.RPC_TakeDamage(10);
=======
                return; // Ignore teammates


            hit.TakeDamage(10);
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4

            Runner.Spawn(effect, transform.position, Quaternion.identity);
            Runner.Despawn(Object);
        }
    }
}