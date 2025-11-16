using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class BowController : MonoBehaviour
{
    [Header("Bow Settings")]
    public float reloadTime = 1f;
    public float fireRate = 0.5f;
    public int quiverSize = 20;
    public float arrowDamage = 10f;
    public GameObject arrowPrefab;
    public GameObject enhancedArrowPrefab;
    public Transform arrowSpawnPoint;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    [Header("UI Panels (Block Shooting)")]
    public GameObject startScreenPanel;
    public GameObject upgradePanel;
    public GameObject waveCompletePanel;
    public GameObject gameOverPanel;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;

    // Upgrades
    public bool infiniteArrows = false;
    public bool enhancedThirdShot = false;
    private int shotCounter = 0;

    private Quaternion initialRotation;
    private Vector3 reloadRotationOffset = new Vector3(30, 0, 0);

    private WaveManager waveManager;

    void Start()
    {
        currentAmmo = quiverSize;
        initialRotation = transform.localRotation;

        waveManager = FindFirstObjectByType<WaveManager>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Shoot()
    {
        // Block shooting if any UI panel is visible
        if (IsPanelVisible(startScreenPanel)) return;
        if (IsPanelVisible(upgradePanel)) return;
        if (IsPanelActiveInHierarchyOrVisible(waveCompletePanel)) return;
        if (IsPanelActiveInHierarchyOrVisible(gameOverPanel)) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (waveManager != null && !waveManager.GameStarted)
            return;

        if (isReloading) return;
        if (Time.time < nextTimeToFire) return;

        if (currentAmmo <= 0 && !infiniteArrows)
        {
            TryReload();
            return;
        }

        nextTimeToFire = Time.time + fireRate;

        if (!infiniteArrows)
            currentAmmo--;

        shotCounter++;

        GameObject prefabToSpawn = arrowPrefab;
        float damageToDeal = arrowDamage;

        if (enhancedThirdShot && shotCounter % 3 == 0)
        {
            prefabToSpawn = enhancedArrowPrefab;
            damageToDeal = 9999f;
        }

        GameObject arrowObj = Instantiate(prefabToSpawn, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        Projectile projectileScript = arrowObj.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.damage = damageToDeal;
            projectileScript.firedByPlayer = true;
        }

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);
    }

    public void TryReload()
    {
        if (isReloading || currentAmmo == quiverSize || infiniteArrows)
            return;

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles + reloadRotationOffset);
        float half = reloadTime / 2f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t / half);
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t / half);
            yield return null;
        }

        currentAmmo = quiverSize;
        isReloading = false;
    }

    private bool IsPanelVisible(GameObject panelRoot)
    {
        if (panelRoot == null) return false;

        CanvasGroup[] groups = panelRoot.GetComponentsInChildren<CanvasGroup>(true);
        if (groups != null && groups.Length > 0)
        {
            foreach (var cg in groups)
            {
                if (cg == null) continue;
                if (!cg.gameObject.activeInHierarchy) continue;
                if (cg.alpha > 0.01f) return true;
            }
            return false;
        }

        return panelRoot.activeInHierarchy;
    }

    private bool IsPanelActiveInHierarchyOrVisible(GameObject panelRoot)
    {
        if (panelRoot == null) return false;

        CanvasGroup[] groups = panelRoot.GetComponentsInChildren<CanvasGroup>(true);
        if (groups != null && groups.Length > 0)
            return IsPanelVisible(panelRoot);

        return panelRoot.activeInHierarchy;
    }
}
