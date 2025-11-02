using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public BowController bow;
    private bool isHoldingShoot = false;

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
        if (isHoldingShoot && bow != null)
        {
            bow.Shoot();
        }
    }
}