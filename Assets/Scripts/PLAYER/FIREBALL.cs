using UnityEngine;
using UnityEngine.InputSystem;

public class FireballAbility : MonoBehaviour
{
    [Header("Ability Stats")]
    public GameObject fireballPrefab;
    public float fireballCooldown = 3f;
    public float fireballDamage = 50f;
    public float aoeRadius = 3f;
    public float fireballBurnDuration = 3f;

    [Header("Visuals")]
    public Transform firePoint;
    public GameObject explosionPrefab;
    public GameObject burningGroundPrefab;

    [Header("Audio")]
    public AudioSource audioSource;    // AudioSource to play SFX
    public AudioClip fireballSound;    // Sound for casting fireball

    [Header("State")]
    public bool isUnlocked = false; // Keep locked logic

    private float nextFireTime = 0f;

    void Awake()
    {
        // Ensure a working AudioSource is attached
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnFIREBALL(InputValue value)
    {
        if (!isUnlocked) return;                  // Only unlocked abilities can fire
        if (Time.time < nextFireTime) return;     // Respect cooldown
        if (!value.isPressed) return;             // Only fire on press

        nextFireTime = Time.time + fireballCooldown;
        FireTheBall();
    }

    void FireTheBall()
    {
        // Play sound (2D sound, full volume)
        if (audioSource != null && fireballSound != null)
        {
            audioSource.spatialBlend = 0f; // Make it 2D so position doesn't matter
            audioSource.PlayOneShot(fireballSound, 1f);
        }

        // Spawn fireball only if prefab and firePoint exist
        if (fireballPrefab != null && firePoint != null)
        {
            GameObject fireballObj = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
            Fireball fireball = fireballObj.GetComponent<Fireball>();
            if (fireball != null)
            {
                fireball.damage = fireballDamage;
                fireball.explosionRadius = aoeRadius;
                fireball.explosionEffect = explosionPrefab;
                fireball.burningGroundEffect = burningGroundPrefab;
                fireball.burnDuration = fireballBurnDuration;
            }
        }
    }
}
