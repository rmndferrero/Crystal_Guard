using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class MeleeEnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask Ground, Player;
    public float health = 100;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    public int attackDamage = 15;
    bool alreadyAttacked;

    //Hit Flash
    public Renderer modelRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;
    private Coroutine flashCoroutine;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    //Crystal Target
    private Transform crystal;
    private CrystalHealth crystalHealth;

    private Rigidbody rb;
    private bool isDead = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.freezeRotation = true;

        if (modelRenderer != null)
        {
            originalColor = modelRenderer.material.color;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        crystalHealth = FindFirstObjectByType<CrystalHealth>();
        if (crystalHealth != null)
        {
            crystal = crystalHealth.transform;
        }
    }

    private void Update()
    {
        if (isDead) return;
        if (player == null) return;
        if (!agent.isOnNavMesh) return;

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, Player);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, Player);

        if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }
        else
        {
            if (crystal != null)
            {
                ChaseCrystal();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Damage"))
        {
            Arrow arrow = collision.gameObject.GetComponent<Arrow>();
            if (arrow != null)
            {
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(HitFlash());

                TakeDamage((int)arrow.damage);
                Destroy(collision.gameObject);
            }
        }
    }

    private void ChaseCrystal()
    {
        if (crystal == null) return;
        agent.SetDestination(crystal.position);
        float distanceToCrystal = Vector3.Distance(transform.position, crystal.position);

        if (distanceToCrystal <= agent.stoppingDistance)
        {
            AttackCrystal();
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void AttackCrystal()
    {
        if (crystal == null) return;
        agent.SetDestination(transform.position);
        transform.LookAt(crystal);

        if (!alreadyAttacked)
        {
            if (crystalHealth != null)
            {
                crystalHealth.TakeDamage(attackDamage);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        agent.enabled = false;

        rb.isKinematic = false;
        rb.freezeRotation = false;

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.OnEnemyDied();

        Destroy(gameObject, 4f);
    }

    IEnumerator HitFlash()
    {
        if (modelRenderer != null)
        {
            modelRenderer.material.color = hitColor;
            yield return new WaitForSeconds(0.15f);
            modelRenderer.material.color = originalColor;
        }
    }
}