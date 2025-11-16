using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyArcher : MonoBehaviour
{
    [Header("Core Components")]
    public NavMeshAgent agent;
    public Transform player;
    private Animator animator;
    private Rigidbody rb;
    public LayerMask Ground, Player;

    [Header("Stats")]
    public float health = 100;
    public float rotationSpeed = 10f;

    [Header("Patrol Logic")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange = 10f;
    public float patrolWaitTime = 2f;
    private float patrolTimer = 0f;

    [Header("Combat Logic")]
    public float timeBetweenAttacks = 2f;
    public float attackRange = 15f;
    public float sightRange = 25f;
    public float playerAggroRange = 12f;
    bool alreadyAttacked;

    [Header("Ranged Attack")]
    public GameObject projectile;
    public Transform firePoint;
    public float projectileSpeed = 32f;
    public float aimHeightOffset = 1.4f;

    [Header("Crystal Target")]
    private Transform crystal;
    private CrystalHealth crystalHealth;

    [Header("Visual & Audio")]
    public Renderer modelRenderer;
    public Color hitColor = Color.red;
    public GameObject deathPoofPrefab;
    public AudioSource audioSource;
    public AudioClip attackSFX;
    public AudioClip deathSFX;
    public AudioClip hitSFX;

    // Private State
    private bool isDead = false;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private bool playerInSightRange, playerInAttackRange;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Auto-find renderer
        if (modelRenderer == null)
            modelRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (modelRenderer != null)
            originalColor = modelRenderer.material.color;

        // Find the player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Find the crystal
        crystalHealth = FindFirstObjectByType<CrystalHealth>();
        if (crystalHealth != null)
            crystal = crystalHealth.transform;

        rb.isKinematic = true;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (isDead || !agent.isOnNavMesh) return;

        // Animator speed fixes walking properly
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        // Check player ranges
        playerInSightRange = player != null && Physics.CheckSphere(transform.position, sightRange, Player);
        playerInAttackRange = player != null && Physics.CheckSphere(transform.position, attackRange, Player);

        Transform currentTarget = crystal; // Default: crystal

        // If player steps between archer & crystal OR gets too close → target player
        if (player != null)
        {
            float distPlayer = Vector3.Distance(transform.position, player.position);
            float distCrystal = crystal ? Vector3.Distance(player.position, crystal.position) : Mathf.Infinity;

            bool playerBlockingView = distPlayer < distCrystal;

            if (playerInSightRange || distPlayer < playerAggroRange || playerBlockingView)
                currentTarget = player;
        }

        if (currentTarget == null)
        {
            Patroling();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= attackRange)
        {
            // Move sideways a bit if the player gets too close
            if (currentTarget == player && distanceToTarget < 7f)
                StrafeMovement();

            float heightOffset = currentTarget == player ? aimHeightOffset : 0.5f;
            AttackTarget(currentTarget, heightOffset);
        }
        else
        {
            ChaseTarget(currentTarget);
        }
    }

    private void Patroling()
    {
        agent.updateRotation = true;

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            float distance = Vector3.Distance(transform.position, walkPoint);

            if (distance < 1f)
            {
                agent.isStopped = true;
                patrolTimer += Time.deltaTime;

                if (patrolTimer >= patrolWaitTime)
                {
                    walkPointSet = false;
                    patrolTimer = 0f;
                    agent.isStopped = false;
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(walkPoint);
            }
        }
    }

    private void SearchWalkPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkPointRange;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    private void ChaseTarget(Transform target)
    {
        if (target == null) return;
        agent.updateRotation = true;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    private void StrafeMovement()
    {
        Vector3 strafeDir = transform.right * (Random.Range(0, 2) == 0 ? 1 : -1);
        Vector3 movePos = transform.position + strafeDir * 3f;

        if (NavMesh.SamplePosition(movePos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void AttackTarget(Transform target, float heightOffset)
    {
        if (target == null) return;

        agent.isStopped = true;
        agent.updateRotation = false;

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;

        if (dir == Vector3.zero) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);

        if (alreadyAttacked) return;

        transform.rotation = lookRot;

        if (animator != null)
            animator.SetTrigger("Attack");

        if (audioSource && attackSFX)
            audioSource.PlayOneShot(attackSFX);

        Vector3 targetPos = target.position + Vector3.up * heightOffset;
        FireArrow(targetPos);

        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void FireArrow(Vector3 target)
    {
        if (firePoint == null || projectile == null) return;

        Vector3 fireDirection = (target - firePoint.position).normalized;
        GameObject arrowObj = Instantiate(projectile, firePoint.position, Quaternion.LookRotation(fireDirection));

        Projectile projectileScript = arrowObj.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.firedByPlayer = false;
            projectileScript.damage = 10f;
        }

        Rigidbody rb_proj = arrowObj.GetComponent<Rigidbody>();
        if (rb_proj != null)
            rb_proj.linearVelocity = fireDirection * projectileSpeed;
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health -= damage;

        if (audioSource && hitSFX)
            audioSource.PlayOneShot(hitSFX);

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlash());

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        agent.enabled = false;

        if (deathPoofPrefab)
            Instantiate(deathPoofPrefab, transform.position, Quaternion.identity);

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
            waveManager.OnEnemyDied();

        if (deathSFX)
            AudioSource.PlayClipAtPoint(deathSFX, transform.position);

        Destroy(gameObject);
    }

    IEnumerator HitFlash()
    {
        if (modelRenderer == null) yield break;

        modelRenderer.material.color = hitColor;
        yield return new WaitForSeconds(0.15f);
        modelRenderer.material.color = originalColor;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, playerAggroRange);
    }
}
