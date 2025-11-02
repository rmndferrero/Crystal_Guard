using UnityEngine;
using System.Collections;

public class BowController : MonoBehaviour
{
    public float reloadTime = 1f;
    public float fireRate = 0.5f;
    public int quiverSize = 20;

    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;

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

        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        nextTimeToFire = Time.time + fireRate;
        currentAmmo--;

        Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
    }

    public void TryReload()
    {
        if (isReloading || currentAmmo == quiverSize)
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