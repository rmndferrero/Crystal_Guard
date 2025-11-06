using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 3f;
    public float damage = 10f;
    public string ownerTag; // "Player" or "Enemy"

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = transform.forward * speed;
#else
        rb.velocity = transform.forward * speed;
#endif

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // ✅ Ignore hitting whoever fired the arrow
        if (other.CompareTag(ownerTag))
            return;

        // ✅ Player arrows hitting enemies
        if (ownerTag == "Player" && other.CompareTag("Enemy"))
        {
            MeleeEnemyAI enemy = other.GetComponent<MeleeEnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }
            Destroy(gameObject);
            return;
        }

        // ✅ Enemy arrows hitting player
        if (ownerTag == "Enemy" && other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
            }
            Destroy(gameObject);
            return;
        }

        // ✅ Enemy arrows hitting crystal
        if (ownerTag == "Enemy" && other.CompareTag("Crystal"))
        {
            CrystalHealth crystal = other.GetComponent<CrystalHealth>();
            if (crystal != null)
            {
                crystal.TakeDamage((int)damage);
            }
            Destroy(gameObject);
            return;
        }

        // ✅ Optional: if it hits environment or something else, destroy arrow
        Destroy(gameObject);
    }
}
