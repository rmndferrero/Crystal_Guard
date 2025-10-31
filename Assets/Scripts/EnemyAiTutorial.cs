using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAiTutorial : MonoBehaviour
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
    bool alreadyAttacked;
    public GameObject projectile;
    public Transform firePoint;
    public float projectileSpeed = 32f;
    public float aimOffset = 0.5f;

    //Hit Flash
    public Renderer modelRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;
    private Coroutine flashCoroutine;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

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
    }

    private void Update()
    {
        if (isDead) return;
        if (player == null) return;
        if (!agent.isOnNavMesh) return;

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, Player);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, Player);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.tag == "Damage")
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(HitFlash());

            TakeDamage(10);
            Destroy(collision.gameObject);
        }
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, Ground))
            walkPointSet = true;
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
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y + aimOffset, player.position.z);
            Vector3 direction = (targetPosition - firePoint.position).normalized;

            Rigidbody rb_proj = Instantiate(projectile, firePoint.position, Quaternion.LookRotation(direction)).GetComponent<Rigidbody>();
            rb_proj.AddForce(direction * projectileSpeed, ForceMode.Impulse);

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}