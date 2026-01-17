using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private Text finalScoreText;

    private ScoreManager scoreManager;

    void Start()
    {
        if (gameOverPanel == null) Debug.LogError("GameOverPanel is not assigned!");
        else Debug.Log("GameOverPanel assigned successfully.");
        if (finalScoreText == null) Debug.LogError("FinalScoreText is not assigned!");
        else Debug.Log("FinalScoreText assigned successfully.");
        gameOverPanel.SetActive(false);
        scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager == null) Debug.LogError("ScoreManager not found!");
        else Debug.Log("ScoreManager found successfully.");
    }

    public void ShowGameOverScreen()
    {
        Debug.Log("ShowGameOverScreen called.");
        if (gameOverPanel != null && scoreManager != null && finalScoreText != null)
        {
            Debug.Log("All required components valid. Setting final score: " + scoreManager.score);
            finalScoreText.text = "Final Score: " + scoreManager.score;
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;
            Debug.Log("Game Over screen displayed with score: " + scoreManager.score);
        }
        else
        {
            if (gameOverPanel == null) Debug.LogError("GameOverPanel is null!");
            if (scoreManager == null) Debug.LogError("ScoreManager is null!");
            if (finalScoreText == null) Debug.LogError("FinalScoreText is null!");
        }
    }

    public void RestartGame()
    {
        Debug.Log("RestartGame called.");
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Game restarted.");
    }
}