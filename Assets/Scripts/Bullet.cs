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
        if (!Object.HasStateAuthority) return; // Ensure only the state authority handles this logic

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
                return; // Ignore teammates


            hit.TakeDamage(10);

            Runner.Spawn(effect, transform.position, Quaternion.identity);
            Runner.Despawn(Object);
        }
    }
}