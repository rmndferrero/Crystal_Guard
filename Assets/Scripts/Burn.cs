using UnityEngine;
using System.Collections.Generic;

public class BurningGround : MonoBehaviour
{
    public float burnDamage = 5f;
    public float burnDuration = 3f;
    public float burnTickRate = 1f;

    private List<EnemyArcher> rangedEnemies = new List<EnemyArcher>();
    private List<MeleeEnemyAI> meleeEnemies = new List<MeleeEnemyAI>();
    private float nextBurnTime;

    void Start()
    {
        Destroy(gameObject, burnDuration);
        nextBurnTime = Time.time + burnTickRate;
    }

    void Update()
    {
        if (Time.time < nextBurnTime)
        {
            return;
        }

        nextBurnTime = Time.time + burnTickRate;

        foreach (var enemy in rangedEnemies)
        {
            if (enemy != null)
            {
                enemy.TakeDamage((int)burnDamage);
            }
        }
        foreach (var enemy in meleeEnemies)
        {
            if (enemy != null)
            {
                enemy.TakeDamage((int)burnDamage);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyArcher ranged = other.GetComponent<EnemyArcher>();
            if (ranged != null && !rangedEnemies.Contains(ranged))
            {
                rangedEnemies.Add(ranged);
            }

            MeleeEnemyAI melee = other.GetComponent<MeleeEnemyAI>();
            if (melee != null && !meleeEnemies.Contains(melee))
            {
                meleeEnemies.Add(melee);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyArcher ranged = other.GetComponent<EnemyArcher>();
            if (ranged != null)
            {
                rangedEnemies.Remove(ranged);
            }

            MeleeEnemyAI melee = other.GetComponent<MeleeEnemyAI>();
            if (melee != null)
            {
                meleeEnemies.Remove(melee);
            }
        }
    }
}