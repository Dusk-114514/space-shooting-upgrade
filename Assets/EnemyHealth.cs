using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHealth = 500;
    [SerializeField] private int currentHealth;

    [Header("无敌状态")]
    public bool isInvulnerable = false;

    [Header("方向性护盾 (Phase 3)")]
    public bool hasDirectionalShield = false;
    public float vulnerableAngle = 0f; // 弱点角度
    public float shieldWidth = 90f;   // 缺口宽度

    [Header("视觉反馈")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color invulnerableColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color shieldedColor = Color.cyan; // 建议改成青色，更显眼

    private SpriteRenderer sr;
    private ScoreManager scoreManager;

    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        if (gameObject.CompareTag("SmallEnemy")) isInvulnerable = false;

        UpdateColor();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 设置普通无敌
    public void SetInvulnerable(bool state)
    {
        if (state == true && gameObject.CompareTag("SmallEnemy")) return;
        isInvulnerable = state;
        hasDirectionalShield = false;
        UpdateColor();
    }

    // 设置方向性护盾
    public void SetDirectionalShield(bool active, float angle)
    {
        isInvulnerable = false;
        hasDirectionalShield = active;
        vulnerableAngle = angle;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (sr != null)
        {
            if (isInvulnerable) sr.color = invulnerableColor;
            else if (hasDirectionalShield) sr.color = shieldedColor;
            else sr.color = normalColor;
        }
    }

    // --- 核心修改区域 ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            int finalDamage = 1; // 基础伤害

            // 检查是否有方向性护盾
            if (hasDirectionalShield)
            {
                // 1. 计算子弹飞来的角度
                Vector2 dir = other.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // 2. 检查是否打在盾上
                float delta = Mathf.DeltaAngle(angle, vulnerableAngle);
                if (Mathf.Abs(delta) > shieldWidth / 2f)
                {
                    // 打在盾上了：格挡
                    // Debug.Log("格挡！");
                    Destroy(other.gameObject);
                    return;
                }
                else
                {
                    // 3. 打进缺口了：双倍伤害！
                    finalDamage = 2;
                    // 这里可以加一个暴击飘字或者特殊的受击音效
                    Debug.Log("<color=red>弱点命中！双倍伤害！</color>");
                }
            }

            // 结算伤害
            TakeDamage(finalDamage);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable && !gameObject.CompareTag("SmallEnemy")) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (scoreManager != null)
        {
            if (gameObject.CompareTag("SmallEnemy")) scoreManager.AddEnemyScore();
            else scoreManager.AddBossScore();
        }
        Destroy(gameObject);
    }

    public int GetCurrentHealth() { return currentHealth; }
}