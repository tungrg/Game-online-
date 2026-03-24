using UnityEngine;
using Fusion;

public class EnemyShooting : NetworkBehaviour
{
    public int Team { get; set; }
    public GameObject bulletPrefab;
    public Transform firePointLeft;
    public Transform firePointRight;
    public float bulletSpeed = 10f;
    public float attackRange = 10f;
    public float fireRate = 1f;

    float fireCooldown;
    void Start()
    {
        Team = 1;
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        fireCooldown -= Runner.DeltaTime;

        GameObject target = GetNearestPlayer();
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance <= attackRange)
        {
            RotateToTarget(target);

            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = fireRate;
            }
        }
    }

    GameObject GetNearestPlayer()
    {
        // 🔥 KHÔNG cache, lấy realtime
        var players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0) return null;

        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDist)
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

        // 🔥 LEFT
        Runner.Spawn(
            bulletPrefab,
            firePointLeft.position,
            firePointLeft.rotation,
            null,
            (runner, obj) =>
            {
                obj.GetComponent<Bullet>().Team = shooterTeam;
            }
        );

        // 🔥 RIGHT
        Runner.Spawn(
            bulletPrefab,
            firePointRight.position,
            firePointRight.rotation,
            null,
            (runner, obj) =>
            {
                obj.GetComponent<Bullet>().Team = shooterTeam;
            }
        );
    }
}