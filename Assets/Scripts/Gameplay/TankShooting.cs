using Fusion;
using UnityEngine;

public class TankShooting : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    [System.NonSerialized] public float bulletSpeed = 20f;
    public float fireCooldown = 1.5f;
    [SerializeField] float fallbackMuzzleDistance = 1.1f;
    [SerializeField] float fallbackMuzzleHeight = 0.2f;

    float _timer;
    int _team;
    TankController _tankController;
    Health _health;
    public override void Spawned()
    {
        _tankController = GetComponent<TankController>();
        _health = GetComponentInParent<Health>();
        CacheMuzzleLocalOffset();
        _team = ResolveShooterTeam();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (!GetInput<PlayerInputData>(out var input))
        {
            return;
        }

        _timer -= Runner.DeltaTime;
        if (!input.isShooting || _timer > 0f)
        {
            return;
        }

        Shoot(input);
        _timer = fireCooldown;
    }

    void Shoot(PlayerInputData input)
    {
        if (bulletPrefab == null)
        {
            return;
        }

        _team = ResolveShooterTeam();

        Vector3 spawnPos = firePoint.position;
        Quaternion spawnRot = firePoint.rotation;

        Runner.Spawn(
            bulletPrefab,
            spawnPos,
            spawnRot,
            Object.StateAuthority,
            (runner, obj) =>
            {
                Bullet bullet = obj.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.SetInitialState(_team, bulletSpeed, GetComponentInParent<Health>(), transform.root, spawnPos, spawnRot);
                }
            }
        );
    }

    void CacheMuzzleLocalOffset()
    {
        Transform turret = _tankController != null ? _tankController.turret : null;
        if (turret == null || firePoint == null)
        {
            return;
        }

        bool isTurretChild = firePoint == turret || firePoint.IsChildOf(turret);
        if (!isTurretChild)
        {
            return;
        }
    }

    int ResolveShooterTeam()
    {
        if (_health != null && _health.Team >= 0)
        {
            return _health.Team;
        }

        if (_tankController != null)
        {
            return _tankController.Team;
        }

        return _team;
    }
}