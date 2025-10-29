using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5; // Boss最大生命值
    private int currentHealth;
    private ScoreManager scoreManager;

    void Start()
    {
        currentHealth = maxHealth;
        scoreManager = FindObjectOfType<ScoreManager>();
        Debug.Log("Enemy health initialized to: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took damage, health now: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (scoreManager != null)
        {
            scoreManager.AddScore(50); // 击败Boss加50分
        }
        Destroy(gameObject); // Boss生命为0时销毁
    }
}