using Fusion;
using UnityEngine;

public class EffectDestroy : NetworkBehaviour
{
    public float life = 1.5f;
    float timer;
    bool scaleApplied;
    float spawnScale = 1f;
    public AudioSource audioSource;

    public void SetSpawnScale(float scale)
    {
        spawnScale = Mathf.Max(0.01f, scale);
        ApplyScaleIfNeeded();
    }

    public override void Spawned()
    {
        timer = life;
        audioSource?.Play();
        ApplyScaleIfNeeded();
    }

    public override void FixedUpdateNetwork()
    {
        ApplyScaleIfNeeded();

        if (!Object.HasStateAuthority) return;

        timer -= Runner.DeltaTime;

        if (timer <= 0f)
        {
            Runner.Despawn(Object);
        }
    }

    void ApplyScaleIfNeeded()
    {
        if (scaleApplied)
        {
            return;
        }

        float finalScale = spawnScale <= 0f ? 1f : spawnScale;
        transform.localScale *= finalScale;

        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem ps = particleSystems[i];
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.startSizeMultiplier *= finalScale;
        }

        TrailRenderer[] trails = GetComponentsInChildren<TrailRenderer>(true);
        for (int i = 0; i < trails.Length; i++)
        {
            TrailRenderer t = trails[i];
            t.startWidth *= finalScale;
            t.endWidth *= finalScale;
        }

        LineRenderer[] lines = GetComponentsInChildren<LineRenderer>(true);
        for (int i = 0; i < lines.Length; i++)
        {
            LineRenderer l = lines[i];
            l.startWidth *= finalScale;
            l.endWidth *= finalScale;
        }

        scaleApplied = true;
    }
}
