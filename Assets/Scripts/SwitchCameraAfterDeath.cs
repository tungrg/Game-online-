using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SwitchCameraAfterDeath : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode previousTargetKey = KeyCode.A;
    [SerializeField] private KeyCode nextTargetKey = KeyCode.D;

    [Header("Spectator Camera")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 9f, -2f);
    [SerializeField] private float positionLerpSpeed = 8f;
    [SerializeField] private float rotationLerpSpeed = 10f;
    [SerializeField] private bool onlyWhenLocalPlayerIsDead = true;

    private readonly List<TankController> _candidates = new List<TankController>();
    private int _currentIndex = -1;
    private TankController _currentTarget;

    void Update()
    {
        bool shouldSpectate = !onlyWhenLocalPlayerIsDead || !HasLocalOwnedTankAlive();
        if (!shouldSpectate)
        {
            return;
        }

        RebuildCandidateList();
        if (_candidates.Count == 0)
        {
            _currentTarget = null;
            _currentIndex = -1;
            return;
        }

        EnsureCurrentTargetIsValid();

        bool previousPressed = WasPreviousPressed();
        bool nextPressed = WasNextPressed();

        if (previousPressed)
        {
            StepTarget(-1);
        }
        else if (nextPressed)
        {
            StepTarget(1);
        }
    }

    void LateUpdate()
    {
        bool shouldSpectate = !onlyWhenLocalPlayerIsDead || !HasLocalOwnedTankAlive();
        if (!shouldSpectate || _currentTarget == null)
        {
            return;
        }

        Transform targetTransform = _currentTarget.transform;
        Vector3 desiredPosition = targetTransform.position + worldOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionLerpSpeed);

        Vector3 lookTarget = targetTransform.position;
        if (_currentTarget.turret != null)
        {
            lookTarget = _currentTarget.turret.position;
        }

        Quaternion desiredRotation = Quaternion.LookRotation((lookTarget - transform.position).normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationLerpSpeed);
    }

    bool HasLocalOwnedTankAlive()
    {
        IReadOnlyList<TankController> allTanks = TankController.GetActiveTanks();
        for (int i = 0; i < allTanks.Count; i++)
        {
            TankController tank = allTanks[i];
            if (tank == null || tank.Object == null)
            {
                continue;
            }

            if (!tank.Object.HasInputAuthority)
            {
                continue;
            }

            Health health = tank.GetComponent<Health>();
            if (health == null || health.HP > 0)
            {
                return true;
            }
        }

        return false;
    }

    void RebuildCandidateList()
    {
        _candidates.Clear();

        IReadOnlyList<TankController> allTanks = TankController.GetActiveTanks();
        for (int i = 0; i < allTanks.Count; i++)
        {
            TankController tank = allTanks[i];
            if (tank == null || tank.Object == null)
            {
                continue;
            }

            if (tank.Object.HasInputAuthority)
            {
                continue;
            }

            Health health = tank.GetComponent<Health>();
            if (health != null && health.HP <= 0)
            {
                continue;
            }

            _candidates.Add(tank);
        }
    }

    void EnsureCurrentTargetIsValid()
    {
        if (_currentTarget != null)
        {
            int existingIndex = _candidates.IndexOf(_currentTarget);
            if (existingIndex >= 0)
            {
                _currentIndex = existingIndex;
                return;
            }
        }

        _currentIndex = 0;
        _currentTarget = _candidates[_currentIndex];
    }

    void StepTarget(int direction)
    {
        if (_candidates.Count == 0)
        {
            return;
        }

        _currentIndex += direction;
        if (_currentIndex < 0)
        {
            _currentIndex = _candidates.Count - 1;
        }
        else if (_currentIndex >= _candidates.Count)
        {
            _currentIndex = 0;
        }

        _currentTarget = _candidates[_currentIndex];
    }

    bool WasPreviousPressed()
    {
        return Input.GetKeyDown(previousTargetKey);
    }

    bool WasNextPressed()
    {
        return Input.GetKeyDown(nextTargetKey);
    }
}
