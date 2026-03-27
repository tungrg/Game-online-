using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Health : NetworkBehaviour
{
    public GameObject deathEffect;

    [Networked] public int HP { get; set; }
    [Networked] public int Mana { get; set; }

    [SerializeField] private int startHP = 100;
    [SerializeField] private int startMana = 30;

    public int Team;

    public Slider healthSlider;
    public Slider manaSlider;

    private PlayerInput playerInput;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            HP = startHP;
            Mana = startMana;
        }

        playerInput = GetComponent<PlayerInput>();
        UpdateUI();
    }

    public override void FixedUpdateNetwork()
    {
        // 🔥 luôn update UI theo giá trị local (fake sync)
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = 100;
            healthSlider.value = HP;
        }

        if (manaSlider != null)
        {
            manaSlider.maxValue = 30;
            manaSlider.value = Mana;
        }
    }

    // 🔥 DAMAGE CHỈ HOST XỬ LÝ
<<<<<<< HEAD
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int damage)
    {
        if (!Object.HasStateAuthority) return;
        HP -= damage;

        if (HP <= 0)
=======
    public void TakeDamage(int damage)
    {
        if (!Object.HasStateAuthority) return;

        HP -= damage;

        if (HP <= 0)    
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
        {
            HP = 0;

            Debug.Log("Player is dead");

<<<<<<< HEAD
=======
            // 🔥 spawn effect = tất cả client đều thấy
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
            if (deathEffect != null)
            {
                Runner.Spawn(deathEffect, transform.position, Quaternion.identity);
            }

<<<<<<< HEAD
=======
            // 🔥 xóa player
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
            Runner.Despawn(Object);
        }
    }
}