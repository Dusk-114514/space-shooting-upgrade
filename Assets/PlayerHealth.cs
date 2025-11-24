using UnityEngine;
using UnityEngine.UI; // 必须引用 UI

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("UI Settings")]
    public Image healthOrbFill; // 红色液体图片
    public Text healthText;     // 【新增】显示数字的 Text
    public float flowSpeed = 5f;

    private float targetFillAmount = 1f;
    private GameOverManager gameOverManager;

    void Start()
    {
        currentHealth = maxHealth;
        targetFillAmount = 1f;

        gameOverManager = FindObjectOfType<GameOverManager>();

        // 初始化 UI (文字和水位)
        UpdateHealthUI();

        if (gameOverManager == null)
        {
            Debug.LogError("GameOverManager not found!");
        }
    }

    void Update()
    {
        // 平滑流动效果
        if (healthOrbFill != null)
        {
            healthOrbFill.fillAmount = Mathf.Lerp(healthOrbFill.fillAmount, targetFillAmount, Time.deltaTime * flowSpeed);
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Player taking {damage} damage.");
        currentHealth -= damage;

        if (currentHealth < 0) currentHealth = 0;

        // 计算新水位
        targetFillAmount = (float)currentHealth / maxHealth;

        // 【关键】每次受伤立刻更新文字显示
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        targetFillAmount = (float)currentHealth / maxHealth;
        UpdateHealthUI();
    }

    // 更新文字的方法
    private void UpdateHealthUI()
    {
        // 1. 如果有文字组件，更新为 "当前 / 最大" 的格式
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        // 注意：fillAmount 是在 Update 里平滑变化的，这里只负责设置文字和瞬间水位(如果需要)
        // targetFillAmount 已经在 TakeDamage 里设置了
    }

    private void GameOver()
    {
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOverScreen();
        }
    }
}