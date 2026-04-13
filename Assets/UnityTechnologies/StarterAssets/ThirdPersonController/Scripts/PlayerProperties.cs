using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProperties : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private Image healthImage;
    [SerializeField] private Image manaImage;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxMana = 30;

    private Health _health;
    private int _lastHp = int.MinValue;
    private int _lastMana = int.MinValue;

    private void Awake()
    {
        _health = GetComponent<Health>();
        if (_health == null)
        {
            _health = GetComponentInParent<Health>();
        }

        if (_health == null)
        {
            _health = GetComponentInChildren<Health>(true);
        }

        if (healthImage == null || manaImage == null)
        {
            BindImagesFromChildren();
        }
    }

    private void BindImagesFromChildren()
    {
        Image[] images = GetComponentsInChildren<Image>(true);

        if (healthImage == null)
        {
            foreach (Image img in images)
            {
                string n = img.gameObject.name.ToLowerInvariant();
                if (n.Contains("health") && n.Contains("fill"))
                {
                    healthImage = img;
                    break;
                }
            }
        }

        if (manaImage == null)
        {
            foreach (Image img in images)
            {
                string n = img.gameObject.name.ToLowerInvariant();
                if (n.Contains("mana") && n.Contains("fill"))
                {
                    manaImage = img;
                    break;
                }
            }
        }

        if (healthImage == null && images.Length > 0)
        {
            healthImage = images[0];
        }

        if (manaImage == null && images.Length > 1)
        {
            manaImage = images[1];
        }
    }

    public override void Spawned()
    {
        UpdateCanvas();
    }

    private void LateUpdate()
    {
        UpdateCanvas();
    }

    private void UpdateCanvas()
    {
        if (_health == null)
        {
            _health = GetComponent<Health>() ?? GetComponentInParent<Health>() ?? GetComponentInChildren<Health>(true);
            if (_health == null)
            {
                return;
            }
        }

        if (healthImage == null || manaImage == null)
        {
            BindImagesFromChildren();
        }

        int hp = Mathf.Clamp(_health.HP, 0, Mathf.Max(1, maxHealth));
        int mana = Mathf.Clamp(_health.Mana, 0, Mathf.Max(1, maxMana));

        if (hp == _lastHp && mana == _lastMana)
        {
            return;
        }

        if (healthImage != null)
        {
            healthImage.type = Image.Type.Filled;
            healthImage.fillMethod = Image.FillMethod.Horizontal;
            healthImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthImage.fillAmount = Mathf.Clamp01((float)hp / Mathf.Max(1, maxHealth));
        }

        if (manaImage != null)
        {
            manaImage.type = Image.Type.Filled;
            manaImage.fillMethod = Image.FillMethod.Horizontal;
            manaImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            manaImage.fillAmount = Mathf.Clamp01((float)mana / Mathf.Max(1, maxMana));
        }

        _lastHp = hp;
        _lastMana = mana;
    }
}