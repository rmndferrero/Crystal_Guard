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

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks = 1f;
    public int attackDamage = 15;
    public float attackRange = 2f;
    bool alreadyAttacked;
    private Transform currentTarget;

    // Hit Flash
    public Renderer modelRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;
    private Coroutine flashCoroutine;

    // Death
    public GameObject deathPoofPrefab;

    // States
    public float sightRange = 10f;
    public bool playerInSightRange;

    // Crystal Target
    private Transform crystal;
    private CrystalHealth crystalHealth;

    private Rigidbody rb;
    private bool isDead = false;
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        rb.isKinematic = true;
        rb.freezeRotation = true;

        if (modelRenderer != null)
            originalColor = modelRenderer.material.color;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        crystalHealth = FindFirstObjectByType<CrystalHealth>();
        if (crystalHealth != null)
            crystal = crystalHealth.transform;
    }

    private void Update()
    {
        if (isDead || !agent.isOnNavMesh) return;

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude / agent.speed);

        playerInSightRange = player != null && Physics.CheckSphere(transform.position, sightRange, Player);

        // Choose target priority
        if (playerInSightRange)
            currentTarget = player;
        else if (crystal != null)
            currentTarget = crystal;
        else
            currentTarget = null;

        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > attackRange)
        {
            ChaseTarget();
        }
        else
        {
            AttackTarget();
        }
    }

    private void ChaseTarget()
    {
        if (currentTarget == null) return;

        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);
    }

    private void AttackTarget()
    {
        if (alreadyAttacked || currentTarget == null) return;

        agent.isStopped = true;

        // Smoothly rotate to face target
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }

        // Play animation
        if (animator != null)
            animator.SetTrigger("Attack");

        // Apply damage
        if (currentTarget.CompareTag("Player"))
        {
            PlayerHealth ph = currentTarget.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(attackDamage);
        }
        else if (currentTarget.CompareTag("Crystal"))
        {
            if (crystalHealth != null)
                crystalHealth.TakeDamage(attackDamage);
        }

        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        agent.isStopped = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlash());

        if (health <= 0) Die();
    }

    private void Die()
    {
        isDead = true;

        if (deathPoofPrefab != null)
            Instantiate(deathPoofPrefab, transform.position, Quaternion.identity);

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.OnEnemyDied();

        Destroy(gameObject);
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
