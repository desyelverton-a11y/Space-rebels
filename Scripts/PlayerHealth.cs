using UnityEngine;
using UnityEngine.UI;
public class PlayerHealth : MonoBehaviour
{
    public int health = 100;
    public GameObject explosionPrefab;
    public Slider healthBar;
    public bool isInvulnerable = false;

    private void Start()
    {
        healthBar.maxValue = health;
        healthBar.value = health;
    }

    public void TakeDamage(int amount)
    {
 
    if (isInvulnerable) return;

    health -= amount;
    healthBar.value = health;

        if (health <= 0)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Object.FindFirstObjectByType<GameManager>().PlayerDied();
            Destroy(gameObject);
        }
    }
}