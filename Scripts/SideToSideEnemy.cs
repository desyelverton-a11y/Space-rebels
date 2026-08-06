using UnityEngine;

public class SideToSideEnemy : MonoBehaviour
{
    public float moveRange = 5f;
    public float moveSpeed = 2f;

    private float startX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        float xOffset = Mathf.Sin(Time.time * moveSpeed) * moveRange;
        transform.position = new Vector3(startX + xOffset, transform.position.y, transform.position.z);
    }
}



