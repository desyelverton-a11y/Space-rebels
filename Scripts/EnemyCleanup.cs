using UnityEngine;

public class EnemyCleanup : MonoBehaviour
{
    public Transform player;
    public float despawnDistance = 20f;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }
    void Update()
    {
        if (transform.position.z < player.position.z - despawnDistance)
        {
            Destroy(gameObject);
        }
    }
}
