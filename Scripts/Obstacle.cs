using UnityEngine;

public class Obstacle : MonoBehaviour, IDamageable
{
    public int health = 20;
    public int collisionDamage = 25;
    public GameObject explosionPrefab;
    public Transform player;
    public float despawnDistance = 30f;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player").transform;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Player"))
        {
            PlayerHealth ph = col.collider.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(collisionDamage);

            Explode();
        }
    }

    private void Explode()
    {

        AudioManager.Instance.PlayExplosion();
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (transform.position.z < player.position.z - despawnDistance)
            Destroy(gameObject);
    }
}
