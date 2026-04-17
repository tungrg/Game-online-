using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : NetworkBehaviour
{
    private static readonly List<TankController> ActiveTanks = new List<TankController>();

    public Transform lowBody;
    public Transform turret;
    public CinemachineCamera cam;
    public float moveSpeed = 10f;
    public float rotateSpeed = 10f;
    public float friction = 0.85f;
    public int Team { get; set; }
    private Vector3 currentVelocity = Vector3.zero;
    private Collider _collider;
    private readonly RaycastHit[] _castHits = new RaycastHit[8];
    private readonly Collider[] _overlapHits = new Collider[16];
    private float _lockedY;
    private float _camSearchCooldown;

    public override void Spawned()
    {
        if (!ActiveTanks.Contains(this))
        {
            ActiveTanks.Add(this);
        }

        gameObject.tag = "Player";
        _collider = GetComponent<Collider>();

        if (_collider != null && _collider.sharedMaterial == null)
        {
            var noFriction = new PhysicsMaterial("TankRuntimeNoFriction");
            noFriction.dynamicFriction = 0f;
            noFriction.staticFriction = 0f;
            noFriction.frictionCombine = PhysicsMaterialCombine.Minimum;
            noFriction.bounciness = 0f;
            noFriction.bounceCombine = PhysicsMaterialCombine.Minimum;
            _collider.sharedMaterial = noFriction;
        }

        _lockedY = transform.position.y;

        ConfigureLocalCameraOwnership();

        if (Object.HasStateAuthority)
        {
            Team = 0;

            var health = GetComponent<Health>();
            if (health != null) health.Team = Team;

            var parentHealth = GetComponentInParent<Health>();
            if (parentHealth != null) parentHealth.Team = Team;

            var childHealth = GetComponentInChildren<Health>();
            if (childHealth != null) childHealth.Team = Team;
        }

        if (Object.HasInputAuthority)
        {
            TryAssignLocalCamera();
        }
    }

    private void OnDestroy()
    {
        ActiveTanks.Remove(this);
    }

    public static IReadOnlyList<TankController> GetActiveTanks()
    {
        return ActiveTanks;
    }

    public override void FixedUpdateNetwork()
    {
        PlayerInputData inputData = default;
        bool hasInput = GetInput(out inputData);

        if (!hasInput && Object != null && Object.HasInputAuthority)
        {
            inputData = ReadLocalFallbackInput();
            hasInput = true;
        }

        if (Object.HasInputAuthority)
        {
            TryAssignLocalCamera();
        }

        // In Fusion, physics/transform simulation should run on state authority only.
        bool canSimulateMovement = Object.HasStateAuthority;
        if (canSimulateMovement)
        {
            Vector2 input = hasInput ? new Vector2(inputData.moveX, inputData.moveY) : Vector2.zero;
            Vector2 normalizedInput = Vector2.ClampMagnitude(input, 1f);
            Vector3 targetVelocity = new Vector3(normalizedInput.x, 0f, normalizedInput.y) * moveSpeed;

            if (normalizedInput.magnitude < 0.01f)
            {
                currentVelocity *= friction;
            }
            else
            {
                currentVelocity = targetVelocity;
            }

            if (currentVelocity.magnitude > 0.01f)
            {
                Vector3 frameMove = currentVelocity * Runner.DeltaTime;
                MoveWithoutRigidbody(frameMove);
            }
            else
            {
                currentVelocity = Vector3.zero;
            }

            if (normalizedInput.magnitude > 0.1f && lowBody != null)
            {
                Vector3 moveDir = new Vector3(normalizedInput.x, 0f, normalizedInput.y);
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                lowBody.rotation = Quaternion.Slerp(lowBody.rotation, targetRot, rotateSpeed * Runner.DeltaTime);
            }
        }

        if (turret != null)
        {
            if (Object.HasStateAuthority && hasInput && inputData.hasAim)
            {
                Vector3 aimPoint = new Vector3(inputData.aimX, turret.position.y, inputData.aimZ);
                RotateTurretTowardPoint(aimPoint, Runner.DeltaTime);
            }
            else if (Object.HasInputAuthority)
            {
                RotateTurret();
            }
        }
    }

    PlayerInputData ReadLocalFallbackInput()
    {
        PlayerInputData data = default;

        float x = 0f;
        float y = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
            data.isShield = Keyboard.current.rKey.wasPressedThisFrame;
        }

        // Legacy input fallback for projects still using old Input Manager axes.
        if (Mathf.Abs(x) < 0.01f)
        {
            x = Input.GetAxisRaw("Horizontal");
        }
        if (Mathf.Abs(y) < 0.01f)
        {
            y = Input.GetAxisRaw("Vertical");
        }

        Vector2 move = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        data.moveX = move.x;
        data.moveY = move.y;

        if (TryGetMouseAimPoint(out Vector3 aimPoint))
        {
            data.aimX = aimPoint.x;
            data.aimZ = aimPoint.z;
            data.hasAim = true;
        }
        else
        {
            data.hasAim = false;
        }

        if (Mouse.current != null)
        {
            data.isShooting = Mouse.current.leftButton.isPressed;
        }
        else
        {
            data.isShooting = Input.GetMouseButton(0);
        }

        return data;
    }

    bool TryGetMouseAimPoint(out Vector3 point)
    {
        point = Vector3.zero;
        if (Camera.main == null || Mouse.current == null)
        {
            return false;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out float enter))
        {
            return false;
        }

        point = ray.GetPoint(enter);
        return true;
    }

    void MoveWithoutRigidbody(Vector3 frameMove)
    {
        Vector3 planarMove = new Vector3(frameMove.x, 0f, frameMove.z);
        float distance = planarMove.magnitude;
        if (distance <= 0.0001f)
        {
            return;
        }

        if (_collider != null)
        {
            ResolveHorizontalOverlap();
        }

        Vector3 direction = planarMove / distance;
        Vector3 moved = Vector3.zero;

        if (_collider != null && TryGetBlockingHit(direction, distance, out RaycastHit firstHit))
        {
            float firstDistance = Mathf.Max(0f, firstHit.distance - 0.01f);
            Vector3 firstMove = direction * firstDistance;
            moved += firstMove;

            Vector3 remaining = planarMove - firstMove;
            Vector3 slide = Vector3.ProjectOnPlane(remaining, firstHit.normal);
            float slideDistance = slide.magnitude;
            if (slideDistance > 0.0001f)
            {
                Vector3 slideDir = slide / slideDistance;
                transform.position += firstMove;

                if (TryGetBlockingHit(slideDir, slideDistance, out RaycastHit slideHit))
                {
                    float slideAllowed = Mathf.Max(0f, slideHit.distance - 0.01f);
                    moved += slideDir * slideAllowed;
                }
                else
                {
                    moved += slide;
                }

                transform.position -= firstMove;
            }
        }
        else
        {
            moved = planarMove;
        }

        Vector3 targetPosition = transform.position + moved;
        targetPosition.y = _lockedY;
        transform.position = targetPosition;
    }

    bool TryGetBlockingHit(Vector3 direction, float distance, out RaycastHit nearestHit)
    {
        nearestHit = default;
        if (_collider == null)
        {
            return false;
        }

        Bounds bounds = _collider.bounds;
        Vector3 halfExtents = bounds.extents;
        halfExtents.y = Mathf.Max(0.1f, halfExtents.y * 0.8f);

        int hitCount = Physics.BoxCastNonAlloc(
            bounds.center,
            halfExtents,
            direction,
            _castHits,
            transform.rotation,
            distance + 0.03f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        float nearest = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _castHits[i];
            if (hit.collider == null || hit.collider == _collider || hit.collider.isTrigger)
            {
                continue;
            }

            if (hit.normal.y > 0.5f)
            {
                continue;
            }

            if (hit.distance < nearest)
            {
                nearest = hit.distance;
                nearestHit = hit;
            }
        }

        return nearest < float.MaxValue;
    }

    void ResolveHorizontalOverlap()
    {
        if (_collider == null)
        {
            return;
        }

        Bounds bounds = _collider.bounds;
        Vector3 halfExtents = bounds.extents;
        halfExtents.y = Mathf.Max(0.1f, halfExtents.y * 0.8f);

        int overlapCount = Physics.OverlapBoxNonAlloc(
            bounds.center,
            halfExtents,
            _overlapHits,
            transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        Vector3 push = Vector3.zero;
        for (int i = 0; i < overlapCount; i++)
        {
            Collider other = _overlapHits[i];
            if (other == null || other == _collider || other.isTrigger)
            {
                continue;
            }

            if (Physics.ComputePenetration(
                _collider,
                transform.position,
                transform.rotation,
                other,
                other.transform.position,
                other.transform.rotation,
                out Vector3 dir,
                out float dist))
            {
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f && dist > 0.0001f)
                {
                    push += dir.normalized * (dist + 0.01f);
                }
            }
        }

        if (push.sqrMagnitude > 0.000001f)
        {
            Vector3 corrected = transform.position + push;
            corrected.y = _lockedY;
            transform.position = corrected;
        }
    }

    void RotateTurret()
    {
        if (Camera.main == null || Mouse.current == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 dir = hit.point - turret.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                turret.rotation = Quaternion.Lerp(turret.rotation, targetRot, 100f * Time.deltaTime);
            }
        }
    }

    void RotateTurretTowardPoint(Vector3 targetPoint, float deltaTime)
    {
        Vector3 dir = targetPoint - turret.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        turret.rotation = Quaternion.Lerp(turret.rotation, targetRot, 100f * deltaTime);
    }
    public static void HandleReliableDataReceived(NetworkRunner runner, PlayerRef sender, ReliableKey key, System.ArraySegment<byte> data) { }

    void TryAssignLocalCamera()
    {
        if (Runner == null || Object == null)
        {
            return;
        }

        // Prevent remote objects from stealing this client's camera target.
        if (Object.InputAuthority != Runner.LocalPlayer)
        {
            return;
        }

        if (cam == null)
        {
            if (_camSearchCooldown > 0f)
            {
                _camSearchCooldown -= Runner.DeltaTime;
                return;
            }

            cam = GetComponentInChildren<CinemachineCamera>(true);
            if (cam == null)
            {
                cam = FindAnyObjectByType<CinemachineCamera>();
                if (cam == null)
                {
                    _camSearchCooldown = 1f;
                    return;
                }
            }
        }

        if (!cam.gameObject.activeSelf)
        {
            cam.gameObject.SetActive(true);
        }

        if (cam.Follow != transform)
        {
            cam.Follow = transform;
        }

        Transform lookTarget = turret != null ? turret : transform;
        if (cam.LookAt != lookTarget)
        {
            cam.LookAt = lookTarget;
        }
    }

    void ConfigureLocalCameraOwnership()
    {
        if (Runner == null || Object == null)
        {
            return;
        }

        bool isLocalOwner = Object.HasInputAuthority && Object.InputAuthority == Runner.LocalPlayer;

        var childCameras = GetComponentsInChildren<CinemachineCamera>(true);
        foreach (var childCamera in childCameras)
        {
            if (childCamera == null)
            {
                continue;
            }

            if (childCamera.gameObject.activeSelf != isLocalOwner)
            {
                childCamera.gameObject.SetActive(isLocalOwner);
            }

            if (isLocalOwner && cam == null)
            {
                cam = childCamera;
            }
        }

        var childCameraFollow = GetComponentsInChildren<CameraFollow>(true);
        foreach (var follow in childCameraFollow)
        {
            if (follow != null)
            {
                follow.enabled = isLocalOwner;
            }
        }
    }
}