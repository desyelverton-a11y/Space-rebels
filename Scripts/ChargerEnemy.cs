using UnityEngine;

public class ChargerEnemy : MonoBehaviour
{
    public float moveSpeed = 10f;
    private Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}



