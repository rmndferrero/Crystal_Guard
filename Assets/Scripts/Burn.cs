using UnityEngine;
using System.Collections.Generic;

public class BurningGround : MonoBehaviour
{
    [Header("Burn Settings")]
    public float burnDamage = 5f;
    public float burnDuration = 3f;
    public float burnTickRate = 1f;

    [Header("Audio")]
    public AudioClip burnSound;
    public float burnVolume = 1f;
    public float soundMinDistance = 3f;
    public float soundMaxDistance = 15f;

    private AudioSource audioSource;
    private List<EnemyArcher> rangedEnemies = new List<EnemyArcher>();
    private List<MeleeEnemyAI> meleeEnemies = new List<MeleeEnemyAI>();
    private float nextBurnTime;

    void Start()
    {
        // Destroy after burn duration
        Destroy(gameObject, burnDuration);

        nextBurnTime = Time.time + burnTickRate;

        // Setup Audio Source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = burnSound;
        audioSource.loop = true;
        audioSource.volume = burnVolume;
        audioSource.playOnAwake = false;

        // Enable 3D positional audio
        audioSource.spatialBlend = 1f; // fully 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = soundMinDistance;
        audioSource.maxDistance = soundMaxDistance;
        audioSource.dopplerLevel = 0f; // prevents pitch shifting

        if (burnSound != null)
            audioSource.Play();
    }

    void Update()
    {
        if (Time.time < nextBurnTime) return;

        nextBurnTime = Time.time + burnTickRate;

        foreach (var enemy in rangedEnemies)
        {
            if (enemy != null)
                enemy.TakeDamage((int)burnDamage);
        }

        foreach (var enemy in meleeEnemies)
        {
            if (enemy != null)
                enemy.TakeDamage((int)burnDamage);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyArcher ranged = other.GetComponent<EnemyArcher>();
        if (ranged != null && !rangedEnemies.Contains(ranged))
            rangedEnemies.Add(ranged);

        MeleeEnemyAI melee = other.GetComponent<MeleeEnemyAI>();
        if (melee != null && !meleeEnemies.Contains(melee))
            meleeEnemies.Add(melee);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyArcher ranged = other.GetComponent<EnemyArcher>();
        if (ranged != null)
            rangedEnemies.Remove(ranged);

        MeleeEnemyAI melee = other.GetComponent<MeleeEnemyAI>();
        if (melee != null)
            meleeEnemies.Remove(melee);
    }

    private void OnDestroy()
    {
        // Stop sound when object ends
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
