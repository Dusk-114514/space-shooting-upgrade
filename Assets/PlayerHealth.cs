using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Protection (受伤保护)")]
    public float damageCooldown = 0.5f; // 受伤后的无敌时间 (秒)
    private float lastDamageTime = -100f; // 上次受伤的时间点

    [Header("UI Settings")]
    public Image healthOrbFill;
    public Text healthText;
    public float flowSpeed = 5f;

    private float targetFillAmount = 1f;
    private GameOverManager gameOverManager;

    void Start()
    {
        currentHealth = maxHealth;
        targetFillAmount = 1f;
        gameOverManager = FindObjectOfType<GameOverManager>();
        UpdateHealthUI();
    }

    void Update()
    {
        if (healthOrbFill != null)
        {
            healthOrbFill.fillAmount = Mathf.Lerp(healthOrbFill.fillAmount, targetFillAmount, Time.deltaTime * flowSpeed);
        }
    }

    public void TakeDamage(int damage)
    {
        // 【核心修复】检查无敌时间
        // 如果距离上次受伤还没过完冷却时间，直接忽略这次伤害
        if (Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        // 更新受伤时间
        lastDamageTime = Time.time;

        Debug.Log($"Player taking {damage} damage.");
        currentHealth -= damage;

        if (currentHealth < 0) currentHealth = 0;

        targetFillAmount = (float)currentHealth / maxHealth;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    // ... (Heal, UpdateHealthUI, GameOver 保持不变) ...
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        targetFillAmount = (float)currentHealth / maxHealth;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null) healthText.text = $"{currentHealth} / {maxHealth}";
    }

    private void GameOver()
    {
        if (gameOverManager != null) gameOverManager.ShowGameOverScreen();
    }
}