using UnityEngine;

public class JetLaser : MonoBehaviour
{
    [Header("Laser Settings")]
    public float laserRange = 200f;
    public float fireRate = 0.15f;
    private float nextFireTime = 0f;

    [Header("Effects")]
    public LineRenderer laserLine;
    public float laserDuration = 0.05f;
    public LayerMask hitLayers;
    public ParticleSystem muzzleFlash;
    public ParticleSystem impactSpark;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            FireLaser();

            // Muzzle flash ALWAYS plays
            
            if (muzzleFlash != null)
            AudioManager.Instance.PlayLaser();
            muzzleFlash.Play();
        }
    }

    void FireLaser()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 startPos = transform.position + transform.forward * 2f;
        Vector3 endPos = ray.origin + ray.direction * laserRange;

        if (Physics.Raycast(ray, out hit, laserRange, hitLayers))
        {
            endPos = hit.point;

            // Damage system
            IDamageable dmg = hit.collider.GetComponent<IDamageable>();
            if (dmg != null)
                dmg.TakeDamage(10);

            // Impact spark ONLY on hit
            if (impactSpark != null)
            {
                impactSpark.transform.position = hit.point;
                impactSpark.Play();
            }
        }

        StartCoroutine(FireLaserEffect(startPos, endPos));
    }

    System.Collections.IEnumerator FireLaserEffect(Vector3 start, Vector3 end)
    {
        laserLine.enabled = true;
        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);

        yield return new WaitForSeconds(laserDuration);

        laserLine.enabled = false;
    }
}
