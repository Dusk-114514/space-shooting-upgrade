using UnityEngine;
using System; // 1. 引入 System 命名空间以使用 Action

public class EnemyHealth : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHealth = 100; // Boss 血量建议设大一点，比如 100 或 200
    [SerializeField] private int currentHealth;

    [Header("无敌状态")]
    public bool isInvulnerable = false;

    [Header("视觉反馈")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color invulnerableColor = new Color(1f, 1f, 1f, 0.5f);

    private SpriteRenderer sr;
    private ScoreManager scoreManager;

    // --- 新增：血量变化事件 ---
    // UI 脚本会订阅这个事件，参数是 (当前血量, 最大血量)
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();

        if (gameObject.CompareTag("SmallEnemy"))
        {
            isInvulnerable = false;
        }

        UpdateColor();

        // 游戏开始时，广播一次初始血量，确保 UI 显示是满的
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetInvulnerable(bool state)
    {
        if (state == true && gameObject.CompareTag("SmallEnemy")) return;
        isInvulnerable = state;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (sr != null) sr.color = isInvulnerable ? invulnerableColor : normalColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable && !gameObject.CompareTag("SmallEnemy")) return;

        currentHealth -= damage;

        // --- 关键修改：广播血量变化 ---
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (scoreManager != null)
        {
            if (gameObject.CompareTag("SmallEnemy")) scoreManager.AddEnemyScore();
            else scoreManager.AddBossScore();
        }

        // 注意：如果是 Boss，死的时候最好先把血条隐藏，这里暂且简单销毁
        Destroy(gameObject);
    }
}