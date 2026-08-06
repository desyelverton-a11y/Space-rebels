using System.Collections;
using TGD.InfiniteTunnelGenerator;
using UnityEngine;
using UnityEngine.Rendering;

public class Jetmovement1 : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public InfiniteTunnelGenerator tunnel;

    [Header("Forward Motion")]
    public float forwardSpeed = 40f;

    [Header("Horizontal Drift")]
    public float horizontalSpeed = 15f;
    public float maxHorizontalOffset = 20f; // corridor width limit

    [Header("Tilt / Banking")]
    public float tiltAmount = 30f; // degrees of rotation
    public float tiltSpeed = 5f;   // how fast the jet tilts
   
    private float horizontalInput;

    [Header("Barrel Roll")]
    public float rollDuration = 0.5f;
    public float rollSpeed = 720f; // degrees per second
    public float rollCooldown = 2f;
    private bool isRolling = false;
    private float nextRollTime = 0f;

   
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

    }


    void Update()
    {
        // A/D or Arrow Keys
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextRollTime)
        {
            StartCoroutine(DoBarrelRoll());
        }

    }


    void FixedUpdate()
    {
        MoveForward();
    }
    void MoveForward()
    {
        if (isRolling) return;
        
        // Get current Z position inside tunnel space
        float z = tunnel.transform.InverseTransformPoint(rb.position).z;

        // Calculate next Z position
        float nextZ = z + forwardSpeed * Time.fixedDeltaTime;

        // Get tunnel center position at next Z
        Vector3 tunnelCenter = tunnel.GetTunnelPositionAtZ(nextZ);

        // Calculate drift offset (local X movement)
        float driftOffset = horizontalInput * horizontalSpeed * Time.fixedDeltaTime;

        // Clamp drift inside tunnel radius
        driftOffset = Mathf.Clamp(driftOffset, -maxHorizontalOffset, maxHorizontalOffset);

        // Convert drift offset into world space
        Vector3 driftWorld = tunnel.transform.right * driftOffset;

        // Final position = tunnel center + drift
        Vector3 finalPos = tunnelCenter + driftWorld;

        rb.MovePosition(finalPos);

        // Rotate jet to match tunnel direction
        Vector3 forwardDir = tunnel.GetTunnelPositionAtZ(nextZ + 1f) - tunnelCenter;
        Quaternion targetRot = Quaternion.LookRotation(forwardDir);

        // Add banking tilt
        float tilt = -horizontalInput * tiltAmount;
        targetRot *= Quaternion.Euler(0, 0, tilt);

        rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRot, tiltSpeed * Time.fixedDeltaTime));
        

    }



    IEnumerator DoBarrelRoll()
    {
        AudioManager.Instance.PlayBarrelRoll();
        isRolling = true;
        nextRollTime = Time.time + rollCooldown;
        PlayerHealth ph = GetComponent<PlayerHealth>();
        ph.isInvulnerable = true;


        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            float rollAngle = rollSpeed * Time.deltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, rollAngle));
            elapsed += Time.deltaTime;
            yield return null;
        }

        isRolling = false;
        ph.isInvulnerable = false;

    }

}


