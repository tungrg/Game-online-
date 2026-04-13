using System.Collections.Generic;
using System.Text;
using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class Trap : NetworkBehaviour
{
    private static readonly ReliableKey TrapExplodeRequestKey = ReliableKey.FromInts(99, 1, 0, 0);

    [Serializable]
    private class TrapExplodePacket
    {
        public string networkId;
    }

    public enum TrapType
    {
        ShootOnlyBarrel,
        TouchOrShootMine
    }

    [Header("Trap Setup")]
    [SerializeField] private TrapType trapType = TrapType.ShootOnlyBarrel;
    [SerializeField] private int explosionDamage = 35;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private bool autoConfigureCollider = true;

    [Header("VFX")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float effectScaleMultiplier = 1f;
    [SerializeField] private bool spawnNetworkEffectsInMultiplayer = false;

    [Header("Debug")]
    [SerializeField] private bool drawExplosionGizmo = true;

    private bool _exploded;

    private void Awake()
    {
        ApplyColliderMode();
        EnsurePhysicsSetupRuntime();
    }

    private void OnValidate()
    {
        ApplyColliderMode();
        EnsurePhysicsSetupRuntime();
    }

    public bool TryTriggerExplosionByShot()
    {
        if (_exploded)
            return false;

        if (trapType != TrapType.ShootOnlyBarrel && trapType != TrapType.TouchOrShootMine)
            return false;

        if (Object == null)
            return false;

        if (!Object.HasStateAuthority)
        {
            SendExplodeRequest();
            return true;
        }

        Explode();
        return true;
    }

    void SendExplodeRequest()
    {
        if (Runner == null || Object == null)
        {
            return;
        }

        TrapExplodePacket packet = new TrapExplodePacket
        {
            networkId = Object.Id.ToString()
        };

        string json = JsonUtility.ToJson(packet);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        PlayerRef owner = Object.StateAuthority;
        if (owner != PlayerRef.None)
        {
            Runner.SendReliableDataToPlayer(owner, TrapExplodeRequestKey, payload);
            return;
        }

        Runner.SendReliableDataToServer(TrapExplodeRequestKey, payload);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_exploded || trapType != TrapType.TouchOrShootMine)
            return;

        if (Object == null)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (other == null)
            return;

        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            Explode();
        }
    }

    private void ApplyColliderMode()
    {
        if (!autoConfigureCollider)
        {
            return;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        if (colliders == null || colliders.Length == 0)
        {
            Debug.LogWarning($"Trap '{name}' has no collider. It cannot block or trigger explosion.", this);
            return;
        }

        bool shouldBeTrigger = trapType == TrapType.TouchOrShootMine;
        foreach (Collider col in colliders)
        {
            if (col == null)
            {
                continue;
            }

            col.isTrigger = shouldBeTrigger;
        }
    }

    private void EnsurePhysicsSetupRuntime()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (trapType != TrapType.TouchOrShootMine)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            return;
        }

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void Explode()
    {
        if (_exploded)
        {
            return;
        }

        if (Object != null && !Object.HasStateAuthority)
        {
            return;
        }

        _exploded = true;
        ApplyExplosionDamage();
        SpawnExplosionEffect();

        if (Runner != null && Object != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ApplyExplosionDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, ~0, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0)
        {
            return;
        }

        HashSet<Health> damagedTargets = new HashSet<Health>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            Health health = hit.GetComponentInParent<Health>();
            if (health == null || damagedTargets.Contains(health))
            {
                continue;
            }

            damagedTargets.Add(health);
            health.TakeDamage(explosionDamage);
        }
    }

    private void SpawnExplosionEffect()
    {
        if (explosionEffect == null)
        {
            return;
        }

        float scale = Mathf.Max(0.01f, effectScaleMultiplier);

        if (Runner != null)
        {
            if (!spawnNetworkEffectsInMultiplayer)
            {
                return;
            }

            Runner.Spawn(explosionEffect, transform.position, Quaternion.identity, null, (runner, obj) =>
            {
                if (obj == null)
                {
                    return;
                }

                EffectDestroy effectDestroy = obj.GetComponent<EffectDestroy>();
                if (effectDestroy != null)
                {
                    effectDestroy.SetSpawnScale(scale);
                }
                else
                {
                    obj.transform.localScale *= scale;
                }
            });
            return;
        }

        GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        EffectDestroy localEffectDestroy = fx.GetComponent<EffectDestroy>();
        if (localEffectDestroy != null)
        {
            localEffectDestroy.SetSpawnScale(scale);
        }
        else
        {
            fx.transform.localScale *= scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawExplosionGizmo)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    static Trap FindTrapById(string networkId)
    {
        if (string.IsNullOrEmpty(networkId))
        {
            return null;
        }

        Trap[] traps = FindObjectsByType<Trap>();
        for (int i = 0; i < traps.Length; i++)
        {   
            Trap trap = traps[i];
            if (trap == null || trap.Object == null)
            {
                continue;
            }

            if (trap.Object.Id.ToString() == networkId)
            {
                return trap;
            }
        }

        return null;
    }

    public static void HandleReliableDataReceived(NetworkRunner runner, ReliableKey key, ArraySegment<byte> data)
    {
        if (key != TrapExplodeRequestKey || data.Count <= 0)
        {
            return;
        }

        string json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
        TrapExplodePacket packet = JsonUtility.FromJson<TrapExplodePacket>(json);
        if (packet == null || string.IsNullOrEmpty(packet.networkId))
        {
            return;
        }

        Trap trap = FindTrapById(packet.networkId);
        if (trap == null || trap.Object == null || !trap.Object.HasStateAuthority)
        {
            return;
        }

        trap.Explode();
    }
}
