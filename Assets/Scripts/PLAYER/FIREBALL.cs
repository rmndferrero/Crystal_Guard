using UnityEngine;
using UnityEngine.InputSystem;

public class FireballAbility : MonoBehaviour
{
    [Header("Ability Stats")]
    public GameObject fireballPrefab;
    public float fireballCooldown = 3f;
    public float fireballDamage = 50f;
    public float aoeRadius = 3f;
    public float fireballBurnDuration = 3f; // <-- FIX

    [Header("Visuals")]
    public Transform firePoint;
    public GameObject explosionPrefab;
    public GameObject burningGroundPrefab; // <-- NEW

    [Header("State")]
    public bool isUnlocked = true;

    private float nextFireTime = 0f;

    void OnFIREBALL(InputValue value)
    {
        if (!isUnlocked) return;
        if (Time.time < nextFireTime) return;

        if (!value.isPressed)
        {
            return;
        }

        nextFireTime = Time.time + fireballCooldown;
        FireTheBall();
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
                fireball.burningGroundEffect = this.burningGroundPrefab; // <-- NEW
                fireball.burnDuration = this.fireballBurnDuration; // <-- NEW
            }
        }
    }
}