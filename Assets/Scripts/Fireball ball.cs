using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Fireball : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 50f;
    public float explosionRadius = 3f;
    public GameObject explosionEffect;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
        Destroy(gameObject);
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                MeleeEnemyAI meleeEnemy = hit.GetComponent<MeleeEnemyAI>();
                if (meleeEnemy != null)
                {
                    meleeEnemy.TakeDamage((int)damage);
                }

                EnemyArcher rangedEnemy = hit.GetComponent<EnemyArcher>();
                if (rangedEnemy != null)
                {
                    rangedEnemy.TakeDamage((int)damage);
                }
            }
        }
    }
}