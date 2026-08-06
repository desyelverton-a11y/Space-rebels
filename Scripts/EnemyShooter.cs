using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public enum FireMode
    {
        Single,
        Burst,
        Spread,
        Charge
    }
    public float fireRate = 1.5f;
    public float burstRate = 0.1f;
    public int burstCount = 3;

    public float spreadAngle = 15f;

    public float chargeTime = 1f;
    public GameObject chargeEffectPrefab;

   
    private bool charging = false;
    public float laserRange = 200f;
    public int damage = 10;
    public LayerMask playerLayer;

    private float nextFireTime = 0f;
    private FireMode fireMode;

    public LineRenderer laserLine;
    public ParticleSystem impactSpark;
    public ParticleSystem muzzleFlash;


    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            switch (fireMode)
            {
                case FireMode.Single:
                    FireSingle();
                    break;

                case FireMode.Burst:
                    StartCoroutine(FireBurst());
                    break;

                case FireMode.Spread:
                    FireSpread();
                    break;

                case FireMode.Charge:
                    StartCoroutine(FireCharge());
                    break;
            }

            nextFireTime = Time.time + fireRate;
            FireLaser();
        }
    }


    void FireSingle()
    {
        ShootRay(transform.forward);
    }
    
    System.Collections.IEnumerator FireBurst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            ShootRay(transform.forward);
            yield return new WaitForSeconds(burstRate);
        }
    }
    void FireSpread()
    {
        ShootRay(transform.forward);

        ShootRay(Quaternion.Euler(0, spreadAngle, 0) * transform.forward);
        ShootRay(Quaternion.Euler(0, -spreadAngle, 0) * transform.forward);
    }
    IEnumerator FireCharge()
    {
        charging = true;

        GameObject chargeFX = Instantiate(chargeEffectPrefab, transform.position, Quaternion.identity);
        chargeFX.transform.SetParent(transform);

        yield return new WaitForSeconds(chargeTime);

        Destroy(chargeFX);

        charging = false;

        ShootRay(transform.forward * 2f); // stronger shot
    }
    void ShootRay(Vector3 direction)
    {
        AudioManager.Instance.PlayEnemyLaser();
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, laserRange, playerLayer))
        {
            PlayerHealth ph = hit.collider.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);

            // Hard-light impact VFX
            impactSpark.transform.position = hit.point;
            impactSpark.Play();
        }

        // Hard-light beam VFX
        StartCoroutine(FireBeam(direction));
    }
    IEnumerator FireBeam(Vector3 direction)
    {
        Vector3 start = transform.position;
        Vector3 end = start + direction * laserRange;

        laserLine.enabled = true;
        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);

        yield return new WaitForSeconds(0.05f);

        laserLine.enabled = false;
    }




    void FireLaser()
    {
        // Aim at the player
        Transform player = GameObject.FindWithTag("Player").transform;
        Vector3 direction = (player.position - transform.position).normalized;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, laserRange, playerLayer))
        {
            PlayerHealth ph = hit.collider.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
        }
    }
}


