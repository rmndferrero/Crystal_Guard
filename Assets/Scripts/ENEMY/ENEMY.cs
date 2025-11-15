using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyArcher : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask Ground, Player;
    public float health = 100;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange = 10f;

    // Attacking
    public float timeBetweenAttacks = 2f;
    bool alreadyAttacked;
    public GameObject projectile;
    public Transform firePoint;
    public float projectileSpeed = 32f;
    public float aimHeightOffset = 1.4f;

    // Hit Flash
    public Renderer modelRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;
    private Coroutine flashCoroutine;

    // Death
    public GameObject deathPoofPrefab;

    // States
    public float sightRange = 25f;
    public float attackRange = 15f;
    public float playerAggroRange = 12f;
    private bool playerInSightRange, playerInAttackRange;

    // Crystal Target
    private Transform crystal;
    private CrystalHealth crystalHealth;

    private Rigidbody rb;
    private bool isDead = false;
    private Animator animator;
    public float rotationSpeed = 5f;

    [Header("Audio")]
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
        playerInAttackRange = player != null && Physics.CheckSphere(transform.position, attackRange, Player);

        if (crystal != null)
        {
            float distanceToCrystal = Vector3.Distance(transform.position, crystal.position);
            float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

            if (distanceToPlayer < playerAggroRange && distanceToPlayer < distanceToCrystal)
            {
                if (playerInAttackRange)
                    AttackPlayer();
                else
                    ChasePlayer();
            }
            else
            {
                if (distanceToCrystal <= attackRange)
                    AttackCrystal();
                else
                    ChaseCrystal();
            }
        }
        else
        {
            if (playerInSightRange && playerInAttackRange)
                AttackPlayer();
            else if (playerInSightRange)
                ChasePlayer();
            else
                Patroling();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Damage"))
        {
            Projectile projectileScript = collision.gameObject.GetComponent<Projectile>();
            if (projectileScript != null && projectileScript.firedByPlayer)
            {
                TakeDamage((int)projectileScript.damage);
                Destroy(collision.gameObject);
            }
        }
    }

    private void Patroling()
    {
        agent.updateRotation = true; // <-- Agent controls rotation
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;
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

    private void ChasePlayer()
    {
        if (player == null) return;
        agent.updateRotation = true; // <-- Agent controls rotation
        agent.SetDestination(player.position);
    }

    private void ChaseCrystal()
    {
        if (crystal == null) return;
        agent.updateRotation = true; // <-- Agent controls rotation
        agent.SetDestination(crystal.position);
    }

    private void AttackPlayer()
    {
        if (alreadyAttacked || player == null) return;
        agent.updateRotation = false; // <-- FIX: Script controls rotation
        agent.SetDestination(transform.position);

        RotateTowards(player.position);

        if (animator != null) animator.SetTrigger("Attack");

        if (audioSource != null && attackSFX != null)
            audioSource.PlayOneShot(attackSFX);

        Vector3 targetPosition = player.position + Vector3.up * aimHeightOffset;
        FireArrow(targetPosition);

        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void AttackCrystal()
    {
        if (alreadyAttacked || crystal == null) return;
        agent.updateRotation = false; // <-- FIX: Script controls rotation
        agent.SetDestination(transform.position);

        RotateTowards(crystal.position);

        if (animator != null) animator.SetTrigger("Attack");

        if (audioSource != null && attackSFX != null)
            audioSource.PlayOneShot(attackSFX);

        Vector3 targetPosition = crystal.position + Vector3.up * 0.5f;
        FireArrow(targetPosition);

        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void FireArrow(Vector3 target)
    {
        Vector3 fireDirection = (target - firePoint.position).normalized;

        GameObject arrowObj = Instantiate(projectile, firePoint.position, Quaternion.LookRotation(fireDirection));
        Rigidbody rb_proj = arrowObj.GetComponent<Rigidbody>();
        Projectile projectileScript = arrowObj.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.firedByPlayer = false;
            projectileScript.damage = 10f;
        }

        if (rb_proj != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb_proj.linearVelocity = fireDirection * projectileSpeed;
#else
            rb_proj.velocity = fireDirection * projectileSpeed;
#endif
        }
    }

    private void RotateTowards(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health -= damage;

        if (audioSource != null && hitSFX != null)
            audioSource.PlayOneShot(hitSFX);

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlash());

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;

        if (deathPoofPrefab != null)
            Instantiate(deathPoofPrefab, transform.position, Quaternion.identity);

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
            waveManager.OnEnemyDied();

        if (deathSFX != null)
        {
            AudioSource.PlayClipAtPoint(deathSFX, transform.position);
        }

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