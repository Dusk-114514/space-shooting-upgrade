using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3; // 最大生命值
    private int currentHealth;
    private GameOverManager gameOverManager;

    void Start()
    {
        currentHealth = maxHealth;
        gameOverManager = FindObjectOfType<GameOverManager>();
        Debug.Log("PlayerHealth initialized. Max Health: " + maxHealth + ", GameOverManager found: " + (gameOverManager != null));
        if (gameOverManager == null)
        {
            Debug.LogError("GameOverManager not found in scene! Ensure GameOverManager is in the scene.");
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Player taking damage. Current Health before: " + currentHealth + ", Damage: " + damage);
        currentHealth -= damage;
        Debug.Log("Player health after damage: " + currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("Player health reached 0 or below. Calling GameOver.");
            GameOver();
        }
    }

    private void GameOver()
    {
        if (gameOverManager != null)
        {
            Debug.Log("GameOverManager found, attempting to call ShowGameOverScreen.");
            gameOverManager.ShowGameOverScreen();
            Debug.Log("ShowGameOverScreen called successfully.");
        }
        else
        {
            Debug.LogError("GameOverManager reference is null when calling GameOver!");
        }
    }
}