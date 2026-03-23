using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using ExitGames.Client.Photon.StructWrapping;

public class Health : NetworkBehaviour
{
    public GameObject deathEffect;

    // ✅ Use explicit backing fields for networked properties
    private int _hp;
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int HP { get => _hp; set => _hp = value; }

    private int _mana;
    [Networked, OnChangedRender(nameof(OnManaChanged))]
    public int Mana { get => _mana; set => _mana = value; }

    private int _team;
    [Networked]
    public int Team { get => _team; set => _team = value; }

    public Slider healthSlider;
    public Slider manaSlider;

    private PlayerInput playerInput;
    private bool canShoot = true;

    // ✅ Set networked values only in the Spawned method
    public override void Spawned()
    {
        // Only set values if this is the input authority (the player who owns this object)
        if (Object.HasInputAuthority)
        {
            HP = 100;
            Mana = 30;
        }

        // Initialize UI
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

        playerInput = GetComponent<PlayerInput>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // ⚠️ Demo test only, use NetworkInput for production
        if (playerInput.actions["Attack"].triggered)
        {
            HP -= 10;
        }
    }

    void OnHealthChanged()
    {
        if (healthSlider != null)
            healthSlider.value = HP;
    }

    void OnManaChanged()
    {
        if (manaSlider != null)
            manaSlider.value = Mana;
    }

    public void TakeDamage(int damage)
    {
        if (!Object.HasStateAuthority) return; // Ensure only the state authority modifies HP

        HP -= damage;

        if (HP <= 0)
        {
            // Handle death logic here
            Debug.Log("Player is dead");
        }
    }
}