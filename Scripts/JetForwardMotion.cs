using UnityEngine;

public class JetForwardMotion : MonoBehaviour
{
    [Header("Forward Motion")]
    public float forwardSpeed = 40f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move the jet forward constantly
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;
    }
}