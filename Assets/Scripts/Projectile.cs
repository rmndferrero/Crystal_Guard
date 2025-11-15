using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 3f;
    public float damage = 10f;
    public bool firedByPlayer = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (firedByPlayer)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = transform.forward * speed;
#else
            rb.velocity = transform.forward * speed;
#endif
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (firedByPlayer)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyArcher ranged = collision.gameObject.GetComponent<EnemyArcher>();
                if (ranged != null)
                {
                    ranged.TakeDamage((int)damage);
                }

                MeleeEnemyAI melee = collision.gameObject.GetComponent<MeleeEnemyAI>();
                if (melee != null)
                {
                    melee.TakeDamage((int)damage);
                }
            }
        }
        else
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }
            else if (collision.gameObject.CompareTag("Crystal"))
            {
                CrystalHealth crystal = collision.gameObject.GetComponent<CrystalHealth>();
                if (crystal != null)
                {
                    crystal.TakeDamage(damage);
                }
            }
        }

        if (firedByPlayer && collision.gameObject.CompareTag("Player")) return;
        if (!firedByPlayer && collision.gameObject.CompareTag("Enemy")) return;

        Destroy(gameObject);
    }
}