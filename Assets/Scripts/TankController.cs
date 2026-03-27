using Fusion;
<<<<<<< HEAD
using TMPro;
=======
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
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
<<<<<<< HEAD
    [Networked] public string PlayerName { get; set; }

    public TMP_Text nameText;

=======
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4


    public float moveSpeed = 10f;
    public float rotateSpeed = 10f;
    public int Team { get; set; }

    void Awake()
    {
        tankController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }
<<<<<<< HEAD
 
    public void SetName(string name)
    {
        if (Object.HasInputAuthority)
        {
            PlayerName = name;
        }
    }

    public override void Render()
    {
        if (nameText != null)
        {
            nameText.text = PlayerName;
        }
    }
    public override void Spawned()
    {
        if (nameText == null)
        {
            nameText = GetComponentInChildren<TMP_Text>();
=======

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Team = 0;
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
        }

        if (Object.HasInputAuthority)
        {
<<<<<<< HEAD
            cam = FindAnyObjectByType<CinemachineCamera>();

            if (cam != null)
            {
                cam.Follow = transform;
                cam.LookAt = turret;
            }
=======
            CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();

            cam.Follow = transform;
            cam.LookAt = turret;
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
        }
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
<<<<<<< HEAD
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetName(string name)
    {
        PlayerName = name;
    }
=======
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
}