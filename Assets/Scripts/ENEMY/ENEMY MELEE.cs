using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class MeleeEnemyAI : MonoBehaviour
{
    [Header("Core Components")]
    public NavMeshAgent agent;
    public Transform player;
    private Animator animator;
    private Rigidbody rb;
    public LayerMask Ground, Player;

    [Header("Stats")]
    public float health = 100;
    public int attackDamage = 15;
    public float rotationSpeed = 20f;

    [Header("Behavior")]
    public float sightRange = 10f;
    public float attackRange = 2f;
    public float timeBetweenAttacks = 1f;

    [Header("Attack Sync")]
    [Tooltip("Delay before attack damage happens.")]
    public float attackDamageDelay = 0.5f;

    bool alreadyAttacked;
    private Transform currentTarget;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange = 10f;

    // Crystal
    private Transform crystal;
    private CrystalHealth crystalHealth;
    private bool isDead = false;

    // Visuals
    public Renderer modelRenderer;
    public Color hitColor = Color.red;
    private Coroutine flashCoroutine;
    private Color originalColor;
    public GameObject deathPoofPrefab;

    // Audio
    public AudioSource audioSource;
    public AudioClip attackSFX;
    public AudioClip deathSFX;
    public AudioClip hitSFX;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        rb.isKinematic = true;
        rb.freezeRotation = true;

        if (modelRenderer != null)
            originalColor = modelRenderer.material.color;

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }

        crystalHealth = FindFirstObjectByType<CrystalHealth>();
        if (crystalHealth != null)
            crystal = crystalHealth.transform;
    }

    private void Update()
    {
        if (isDead || !agent.isOnNavMesh) return;

        // --- NEW ANIMATION SPEED UPDATE ---
        animator.SetFloat("Speed", agent.velocity.magnitude);

        // Target selection
        bool playerInSight = player != null && Physics.CheckSphere(transform.position, sightRange, Player);

        if (playerInSight)
            currentTarget = player;
        else if (crystal != null)
            currentTarget = crystal;
        else
            currentTarget = null;

        if (currentTarget == null)
        {
            Patroling();
        }
        else
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);

            if (dist > agent.stoppingDistance)
                ChaseTarget();
            else
                AttackTarget();
        }
    }

    private void Patroling()
    {
        agent.isStopped = false;
        agent.updateRotation = true;

        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        Vector3 random = Random.insideUnitSphere * walkPointRange;
        random += transform.position;

        if (NavMesh.SamplePosition(random, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    private void ChaseTarget()
    {
        if (currentTarget == null) return;

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(currentTarget.position);
    }

    private void AttackTarget()
    {
        agent.isStopped = true;

        if (currentTarget == null) return;

        // Rotate toward target
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotationSpeed);
        }

        if (alreadyAttacked) return;

        // Trigger attack animation
        animator.SetTrigger("Attack");

        StartCoroutine(DelayedAttack());

        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    IEnumerator DelayedAttack()
    {
        yield return new WaitForSeconds(attackDamageDelay);

        if (audioSource != null && attackSFX != null)
            audioSource.PlayOneShot(attackSFX);

        if (currentTarget == null) yield break;

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        if (dist <= attackRange)
        {
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
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        health -= dmg;

        if (audioSource != null && hitSFX != null)
            audioSource.PlayOneShot(hitSFX);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(HitFlash());

        if (health <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        agent.enabled = false;

        if (deathPoofPrefab != null)
            Instantiate(deathPoofPrefab, transform.position, Quaternion.identity);

        WaveManager wm = FindFirstObjectByType<WaveManager>();
        if (wm != null) wm.OnEnemyDied();

        if (audioSource != null && deathSFX != null)
            AudioSource.PlayClipAtPoint(deathSFX, transform.position);

        Destroy(gameObject);
    }

    IEnumerator HitFlash()
    {
        modelRenderer.material.color = hitColor;
        yield return new WaitForSeconds(0.15f);
        modelRenderer.material.color = originalColor;
    }
}
