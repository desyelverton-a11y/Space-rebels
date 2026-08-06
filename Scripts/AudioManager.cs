using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Player Sounds")]
    public AudioSource jetHum;
    public AudioSource laserFire;
    public AudioSource barrelRollWhoosh;

    [Header("Enemy Sounds")]
    public AudioSource enemyLaser;
    public AudioSource explosion;

    [Header("Ambient")]
    public AudioSource spaceHum;
    public AudioSource backgroundMusic;

    void Awake()
    {
        spaceHum.Play();
        backgroundMusic.Play();
        Instance = this;
    }

    public void PlayLaser()
    {
        laserFire.Play();
    }

    public void PlayEnemyLaser()
    {
        enemyLaser.Play();
    }

    public void PlayExplosion()
    {
        explosion.Play();
    }

    public void PlayBarrelRoll()
    {
        barrelRollWhoosh.Play();
    }
}
