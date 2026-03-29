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
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int damage)
    {
        if (!Object.HasStateAuthority) return;
        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;

            Debug.Log("Player is dead");

            if (deathEffect != null)
            {
                Runner.Spawn(deathEffect, transform.position, Quaternion.identity);
            }

            Runner.Despawn(Object);
        }
    }
}