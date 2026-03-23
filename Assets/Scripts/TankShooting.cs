using Fusion;
using UnityEngine;

public class TankShooting : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed;

    float firecooldown = 1.5f;
    float timer = 0f;

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        timer -= Runner.DeltaTime;

        if (Input.GetMouseButton(0) && timer <= 0f)
        {
            Shoot();
            timer = firecooldown;
        }
    }

    void Shoot()
    {
        var bullet = Runner.Spawn(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation,
            Object.InputAuthority
        );
    }
}