using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Fireball : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 50f;
    public float explosionRadius = 3f;
    public GameObject explosionEffect;
    public GameObject burningGroundEffect;
    public float burnDuration = 3f; // <-- NEW

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = transform.forward * speed;
#else
        rb.velocity = transform.forward * speed;
#endif
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode(collision.contacts[0].point);
        Destroy(gameObject);
    }

    private void Explode(Vector3 hitPosition)
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, hitPosition, Quaternion.identity);
        }

        if (burningGroundEffect != null)
        {
            GameObject burnObj = Instantiate(burningGroundEffect, hitPosition, Quaternion.identity);
            BurningGround burnScript = burnObj.GetComponent<BurningGround>();
            if (burnScript != null)
            {
                burnScript.burnDuration = this.burnDuration; // <-- NEW
            }
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