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
    // Assign the root GameObjects for these UI panels in the inspector.
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
        // NEW: Use a robust check that inspects all CanvasGroups on the panel + children.
        if (IsPanelVisible(startScreenPanel)) return;
        if (IsPanelVisible(upgradePanel)) return;
        if (IsPanelActiveInHierarchyOrVisible(waveCompletePanel)) return;
        if (IsPanelActiveInHierarchyOrVisible(gameOverPanel)) return;

        // Block if clicking UI (standard check).
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Also block until the WaveManager says the game started.
        if (waveManager != null && !waveManager.GameStarted)
            return;

        // Normal shooting logic
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

        // Play shoot sound
        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);
    }

    public void TryReload()
    {
        if (isReloading || currentAmmo == quiverSize || infiniteArrows)
            return;

        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        // Play reload sound
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

    // Returns true if the panel is visible (has any CanvasGroup with alpha > 0.01 and active),
    // or if the root GameObject is active and no canvasgroup exists (fallback).
    private bool IsPanelVisible(GameObject panelRoot)
    {
        if (panelRoot == null) return false;

        // Check any CanvasGroup on panelRoot or children
        CanvasGroup[] groups = panelRoot.GetComponentsInChildren<CanvasGroup>(true);
        if (groups != null && groups.Length > 0)
        {
            foreach (var cg in groups)
            {
                if (cg == null) continue;
                if (!cg.gameObject.activeInHierarchy) continue;
                if (cg.alpha > 0.01f) return true;
            }
            // If we had CanvasGroups but none have alpha > 0.01, treat as not visible.
            return false;
        }

        // Fallback: no CanvasGroup found; use activeInHierarchy
        return panelRoot.activeInHierarchy;
    }

    // Slightly different fallback used for panels that may not use CanvasGroup fade but are simply activated.
    private bool IsPanelActiveInHierarchyOrVisible(GameObject panelRoot)
    {
        if (panelRoot == null) return false;

        // If it has CanvasGroups, rely on IsPanelVisible
        CanvasGroup[] groups = panelRoot.GetComponentsInChildren<CanvasGroup>(true);
        if (groups != null && groups.Length > 0)
            return IsPanelVisible(panelRoot);

        // Otherwise, fallback to activeInHierarchy
        return panelRoot.activeInHierarchy;
    }
}
