using UnityEngine;

public class HoverEnemy : MonoBehaviour
{
    public float hoverAmplitude = 0.5f;
    public float hoverSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
    }
}

