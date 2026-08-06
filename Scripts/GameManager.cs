using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Transform playerSpawnPoint;

    public void PlayerDied()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // pause game
    }

    public void RestartFromCorridor()
    {
        Time.timeScale = 1f;

        // Respawn player
        GameObject jetPrefab = Resources.Load<GameObject>("Jet");
        Instantiate(jetPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        gameOverPanel.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
