using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public BowController bow;
    private bool isHoldingShoot = false;
    private FireballAbility fireballAbility;

    void Awake()
    {
        fireballAbility = GetComponent<FireballAbility>();
    }

    void OnShoot()
    {
        isHoldingShoot = true;
    }

    void OnShootRelease()
    {
        isHoldingShoot = false;
    }

    void OnReload()
    {
        if (bow != null)
        {
            bow.TryReload();
        }
    }

    void Update()
    {
        if (fireballAbility != null && fireballAbility.isCharging)
        {
            return;
        }

        if (isHoldingShoot && bow != null)
        {
            bow.Shoot();
        }
    }
}