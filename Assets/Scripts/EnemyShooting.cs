using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class EnemyShooting : NetworkBehaviour
{
    public int Team { get; set; }
    public GameObject bulletPrefab;
    public float bulletSpeed = 8f;
    public float attackRange = 15f;
    public float fireRate = 2f;
    public float searchRange = 20f;

    float fireCooldown;
    GameObject cachedTarget = null;
    float targetSearchCooldown = 0.5f;
    float timeSinceLastSearch = 0f;

    [SerializeField] private Transform[] leftfirePoints;
    [SerializeField] private Transform[] rightfirePoints;


    public override void Spawned()
    {
        Team = 1;

        if (Object != null && Object.HasStateAuthority)
        {
            Health health = GetComponent<Health>();
            if (health != null) health.Team = Team;

            Health parentHealth = GetComponentInParent<Health>();
            if (parentHealth != null) parentHealth.Team = Team;

            Health childHealth = GetComponentInChildren<Health>();
            if (childHealth != null) childHealth.Team = Team;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // Update target search with cooldown
        timeSinceLastSearch += Runner.DeltaTime;
        if (timeSinceLastSearch >= targetSearchCooldown)
        {
            cachedTarget = GetNearestPlayer();
            timeSinceLastSearch = 0f;
        }

        fireCooldown -= Runner.DeltaTime;

        if (cachedTarget == null) return;

        // Check if target still exists and within range
        float distance = Vector3.Distance(transform.position, cachedTarget.transform.position);

        if (distance <= attackRange)
        {
            RotateToTarget(cachedTarget);

            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = fireRate;
            }
        }
    }

    GameObject GetNearestPlayer()
    {
        IReadOnlyList<TankController> players = TankController.GetActiveTanks();
        if (players == null || players.Count == 0)
        {
            return null;
        }

        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (TankController player in players)
        {
            if (player == null)
            {
                continue;
            }

            if (player.Object == null || !player.Object.IsValid)
            {
                continue;
            }

            GameObject p = player.gameObject;
            if (p == null)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDist && dist <= searchRange)
            {
                minDist = dist;
                nearest = p;
            }
        }

        return nearest;
    }

    void RotateToTarget(GameObject target)
    {
        Vector3 dir = (target.transform.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                Runner.DeltaTime * 5f
            );
        }
    }

    void Shoot()
    {
        if (!Object.HasStateAuthority) return;

        int shooterTeam = Team;
        Health shooterHealth = GetComponentInParent<Health>();

        // 🔥 LEFT
        foreach (var leftfirePoint in leftfirePoints)
        {
            if (leftfirePoint == null) continue;

            Runner.Spawn(
                bulletPrefab,
                leftfirePoint.position,   // ✅ dùng đúng biến
                leftfirePoint.rotation,   // ✅ dùng đúng biến
                null,
                (runner, obj) =>
                {
                    var bullet = obj.GetComponent<Bullet>();
                    bullet.SetInitialState(shooterTeam, bulletSpeed, shooterHealth);
                }
            );
        }

        // 🔥 RIGHT
        foreach (var rightfirePoint in rightfirePoints)
        {
            if (rightfirePoint == null) continue;

            Runner.Spawn(
                bulletPrefab,
                rightfirePoint.position,   // ✅ đúng
                rightfirePoint.rotation,   // ✅ đúng
                null,
                (runner, obj) =>
                {
                    var bullet = obj.GetComponent<Bullet>();
                    bullet.SetInitialState(shooterTeam, bulletSpeed, shooterHealth);
                }
            );
        }
    }
}