using UnityEngine;

public class FlyingRocket : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;

    [Header("Combat")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float poisonDPS = 0f;
    [SerializeField] private float poisonDuration = 4f;
    [SerializeField] private GameObject explosionPrefab;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        Destroy(gameObject, lifetime);
    }

    void Start()
    {
        rb.linearVelocity = transform.forward * speed;
    }
    public void SetPoison(float dps, float duration)
    {
        poisonDPS = dps;
        poisonDuration = duration;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            Explode();
            return;
        }

        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
            
            if (poisonDPS > 0)
            {
                targetHealth.ApplyPoison(poisonDPS, poisonDuration);
            }
            
            Explode();
        }
    }

    private void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(explosion, ps.main.duration);
        }
        Destroy(gameObject);
    }
}
