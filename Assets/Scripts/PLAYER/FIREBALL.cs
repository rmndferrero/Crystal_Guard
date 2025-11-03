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
    public GameObject explosionPrefab;

    [Header("State")]
    public bool isUnlocked = true;

    private float nextFireTime = 0f;

    // This function name MUST match your Input Action (e.g., "FIREBALL")
    void OnFIREBALL(InputValue value)
    {
        if (!isUnlocked) return;
        if (Time.time < nextFireTime) return;

        // Only run on the press-down frame
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
            }
        }
    }
}