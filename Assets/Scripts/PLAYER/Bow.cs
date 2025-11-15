using UnityEngine;
using System.Collections;

public class BowController : MonoBehaviour
{
    public float reloadTime = 1f;
    public float fireRate = 0.5f;
    public int quiverSize = 20;
    public float arrowDamage = 10f;
    public GameObject arrowPrefab;
    public GameObject enhancedArrowPrefab;
    public Transform arrowSpawnPoint;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;

    // --- NEW UPGRADE VARIABLES ---
    public bool infiniteArrows = false;
    public bool enhancedThirdShot = false;
    private int shotCounter = 0;
    // --- END NEW ---

    private Quaternion initialRotation;
    private Vector3 reloadRotationOffset = new Vector3(30, 0, 0);

    void Start()
    {
        currentAmmo = quiverSize;
        initialRotation = transform.localRotation;
    }

    public void Shoot()
    {
        if (isReloading) return;
        if (Time.time < nextTimeToFire) return;

        if (currentAmmo <= 0 && !infiniteArrows)
        {
            TryReload();
            return;
        }

        nextTimeToFire = Time.time + fireRate;

        if (!infiniteArrows)
        {
            currentAmmo--;
        }

        shotCounter++;

        // --- NEW ENHANCED SHOT LOGIC ---
        GameObject prefabToSpawn = arrowPrefab;
        float damageToDeal = this.arrowDamage;

        if (enhancedThirdShot && shotCounter % 3 == 0)
        {
            prefabToSpawn = enhancedArrowPrefab;
            damageToDeal = 9999f; // One-shot damage
        }
        // --- END NEW ---

        GameObject arrowObj = Instantiate(prefabToSpawn, arrowSpawnPoint.position, arrowSpawnPoint.rotation);

        Projectile projectileScript = arrowObj.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.damage = damageToDeal;
            projectileScript.firedByPlayer = true;
        }
    }

    public void TryReload()
    {
        if (isReloading || currentAmmo == quiverSize || infiniteArrows)
        {
            return;
        }
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles + reloadRotationOffset);
        float halfReload = reloadTime / 2f;
        float t = 0f;

        while (t < halfReload)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t / halfReload);
            yield return null;
        }

        t = 0f;

        while (t < halfReload)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t / halfReload);
            yield return null;
        }

        currentAmmo = quiverSize;
        isReloading = false;
    }
}