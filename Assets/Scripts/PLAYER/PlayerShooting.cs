using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public BowController bow;
    private bool isHoldingShoot = false;

    // This is called by Send Messages (Action: "Shoot")
    void OnShoot()
    {
        isHoldingShoot = true;
    }

    // This is called by Send Messages (Action: "ShootRelease")
    void OnShootRelease()
    {
        isHoldingShoot = false;
    }

    // This is called by Send Messages (Action: "Reload")
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