using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public int health = 20;
    public GameObject explosionPrefab;


    public void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            AudioManager.Instance.PlayExplosion();
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

}
