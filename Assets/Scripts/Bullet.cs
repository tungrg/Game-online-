using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public int Team { get; set; }

    float lifeTime = 3f;
    public GameObject effect;
    public float speed = 10f;
    [Networked] public Vector3 NetPos { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            NetPos = transform.position;
        }
    }
    public override void FixedUpdateNetwork()
    {       
        if (Object.HasStateAuthority)
        {
            NetPos += transform.forward * speed * Runner.DeltaTime;
        }
        transform.position = NetPos;

        if (Object.HasStateAuthority)
        {
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

        if (hit != null)
        {
            // 🔥 FIX QUAN TRỌNG
            if (!hit.Object || !hit.Object.IsValid)
                return;
            if (hit.Team == Team)
                return; // Ignore teammates

            hit.TakeDamage(10);

            Runner.Despawn(Object);
            Instantiate(effect, transform.position, Quaternion.identity);
        }
    }
}