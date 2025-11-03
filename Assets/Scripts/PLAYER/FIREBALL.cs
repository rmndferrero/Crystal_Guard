using UnityEngine;
using UnityEngine.InputSystem;

public class FireballAbility : MonoBehaviour
{
    [Header("Ability Stats")]
    public GameObject fireballPrefab;
    public float fireballCooldown = 3f;
    public float fireballDamage = 50f;
    public float aoeRadius = 3f;

    [Header("Visuals")]
    public Transform firePoint;
    public GameObject throwIndicatorPrefab;
    public GameObject explosionPrefab;

    [Header("State")]
    public bool isUnlocked = true;
    public bool isCharging = false;

    private float nextFireTime = 0f;
    private GameObject currentIndicator;

    // This is called by Send Messages (Action: "FIREBALL")
    void OnFIREBALL(InputValue value)
    {
        if (!isUnlocked) return;
        if (Time.time < nextFireTime && !isCharging) return;

        isCharging = !isCharging;

        if (isCharging)
        {
            if (throwIndicatorPrefab != null && currentIndicator == null)
            {
                currentIndicator = Instantiate(throwIndicatorPrefab, firePoint.position, firePoint.rotation, firePoint);
            }
        }
        else
        {
            if (currentIndicator != null)
            {
                Destroy(currentIndicator);
            }
        }
    }

    // This is called by Send Messages (Action: "Shoot")
    void OnShoot()
    {
        if (!isCharging || !isUnlocked)
        {
            return;
        }
        if (Time.time < nextFireTime) return;

        FireTheBall();

        isCharging = false;
        nextFireTime = Time.time + fireballCooldown;

        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
        }
    }

    void FireTheBall()
    {
        if (fireballPrefab != null && firePoint != null)
        {
            GameObject fireballObj = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
            Fireball fireball = fireballObj.GetComponent<Fireball>();
            if (fireball != null)
            {
                fireball.damage = this.fireballDamage;
                fireball.explosionRadius = this.aoeRadius;
                fireball.explosionEffect = this.explosionPrefab;
            }
        }
    }
}