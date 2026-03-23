using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : NetworkBehaviour
{
    private CharacterController tankController;
    private PlayerInput playerInput;

    public Transform lowBody;
    public Transform turret;
    public CinemachineCamera cam;


    public float moveSpeed = 10f;
    public float rotateSpeed = 10f;

    void Awake()
    {
        tankController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    public override void Spawned()
    {
        if (!Object.HasInputAuthority) return;

        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();

        cam.Follow = transform;
        cam.LookAt = turret;
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();

        Vector3 move = new Vector3(input.x, 0, input.y);

        tankController.Move(move * moveSpeed * Runner.DeltaTime);

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            lowBody.rotation = Quaternion.Slerp(
                lowBody.rotation,
                targetRot,
                rotateSpeed * Runner.DeltaTime
            );
        }

        RotateTurret();
    }

    void RotateTurret()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 dir = hit.point - turret.position;
            dir.y = 0;

            Quaternion targetRot = Quaternion.LookRotation(dir);

            turret.rotation = Quaternion.Lerp(
                turret.rotation,
                targetRot,
                100f * Time.deltaTime
            );
        }
    }
}