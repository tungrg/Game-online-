using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public int Team { get; set; }

    private float speed;
    public int damage = 10;
    [SerializeField] private float lifeTime = 5f;
    private float _remainingLife;
    public GameObject effect;

    float _radius = 0.15f;
    bool _hasHit;
    Health _shooterHealth;
    Transform _shooterRoot;
    Vector3 _moveDirection;
    bool _isInitialized;
    [SerializeField] LayerMask hitMask;

    public void SetInitialState(int team, float initialSpeed, Health shooterHealth)
    {
        SetInitialState(team, initialSpeed, shooterHealth, shooterHealth != null ? shooterHealth.transform.root : null, transform.position, transform.rotation);
    }

    public void SetInitialState(int team, float initialSpeed, Health shooterHealth, Transform shooterRoot, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        Team = team;
        speed = Mathf.Max(0f, initialSpeed);
        _shooterHealth = shooterHealth;
        _shooterRoot = shooterRoot != null ? shooterRoot : (shooterHealth != null ? shooterHealth.transform.root : null);

        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        _moveDirection = spawnRotation * Vector3.forward;
        if (_moveDirection.sqrMagnitude <= 0.0001f)
        {
            _moveDirection = transform.forward;
        }
        _moveDirection.Normalize();
        _isInitialized = true;
    }

    public override void Spawned()
    {
        _remainingLife = lifeTime;

        if (!_isInitialized)
        {
            speed = Mathf.Max(0f, speed);
            _moveDirection = transform.forward;
            if (_moveDirection.sqrMagnitude <= 0.0001f)
            {
                _moveDirection = Vector3.forward;
            }
            _moveDirection.Normalize();
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            _radius = Mathf.Max(0.05f, col.bounds.extents.magnitude * 0.35f);
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (_hasHit)
            return;

        if (!Object.HasStateAuthority)
        {
            return;
        }   

        Vector3 start = transform.position;
        Vector3 move = _moveDirection * speed * Runner.DeltaTime;
        float dist = move.magnitude;
        int castMask = hitMask.value != 0 ? hitMask.value : ~0;

        if (dist > 0.0001f)
        {
            bool hasShieldHit = TryGetShieldHit(start, move, out Vector3 shieldHitPoint, out float shieldHitDistance);

            bool hasWorldHit = Physics.SphereCast(
                start,
                _radius,
                move.normalized,
                out RaycastHit worldHit,
                dist,
                castMask,
                QueryTriggerInteraction.Collide
            );

            if (hasShieldHit && (!hasWorldHit || shieldHitDistance <= worldHit.distance))
            {
                transform.position = shieldHitPoint;
                DespawnBullet();
                return;
            }

            if (hasWorldHit)
            {
                HandleHit(worldHit.collider);
                if (_hasHit)
                {
                    return;
                }
            }
        }

        transform.position = transform.position + move;

        _remainingLife -= Runner.DeltaTime;
        if (_remainingLife <= 0f)
        {
            Runner.Despawn(Object);
        }
    }

    bool TryGetShieldHit(Vector3 start, Vector3 move, out Vector3 hitPoint, out float hitDistance)
    {
        hitPoint = Vector3.zero;
        hitDistance = float.MaxValue;

        IReadOnlyList<PlayerSkill> skills = PlayerSkill.GetActiveSkills();
        if (skills == null || skills.Count == 0)
        {
            return false;
        }

        bool found = false;
        Vector3 end = start + move;

        for (int i = 0; i < skills.Count; i++)
        {
            PlayerSkill skill = skills[i];
            if (skill == null)  
            {
                continue;
            }

            if (!skill.TryBlockSegment(Team, _shooterRoot, start, end, out Vector3 candidatePoint))
            {
                continue;
            }

            float candidateDistance = (candidatePoint - start).magnitude;
            if (candidateDistance < hitDistance)
            {
                hitDistance = candidateDistance;
                hitPoint = candidatePoint;
                found = true;
            }
        }

        return found;
    }
    void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority || _hasHit)
        {
            return;
        }

        HandleHit(other);
    }

    void HandleHit(Collider other)
    {
        if (other == null || _hasHit)
        {
            return;
        }

        if (other.transform == transform || other.transform.IsChildOf(transform))
        {
            return;
        }

        if (_shooterRoot != null && other.transform.IsChildOf(_shooterRoot))
        {
            return;
        }

        Trap trap = other.GetComponentInParent<Trap>();
        if (trap != null)
        {
            trap.TryTriggerExplosionByShot();
            DespawnBullet();
            return;
        }
        Health health = FindHealth(other);
        if (health != null)
        {
            if (_shooterHealth != null && health == _shooterHealth)
            {
                Debug.Log("Bullet hit shooter, ignoring damage.");
                return;
            }

            if (other.CompareTag("Player") && health.Team == Team)
            {
                Debug.Log("Bullet hit teammate, ignoring damage.");
                return;
            }

            if (Object.HasStateAuthority)
            {
                health.TakeDamage(damage, _shooterHealth);
            }
            DespawnBullet();
            return;
        }

        if (other.CompareTag("Wall"))
        {
            DespawnBullet();
        }
    }
    Health FindHealth(Collider col)
    {
        Health h = col.GetComponentInParent<Health>();
        if (h != null)
        {
            return h;
        }

        if (col.attachedRigidbody != null)
        {
            return col.attachedRigidbody.GetComponentInParent<Health>();
        }

        return null;
    }

    void DespawnBullet()
    {
        if (_hasHit)
            return;

        _hasHit = true;

        // ✅ chỉ host làm thật
        if (effect != null && Runner != null)
        {
            Runner.Spawn(effect, transform.position, Quaternion.identity);
        }

        if (Runner != null && Object != null)
        {
            Runner.Despawn(Object);
        }
    }
}