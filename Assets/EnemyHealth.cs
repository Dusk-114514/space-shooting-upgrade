using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5; // Boss最大生命值
    private int currentHealth;
    private ScoreManager scoreManager;
    [SerializeField] private LayerMask playerBulletLayerMask; // 玩家子弹的 LayerMask
    [SerializeField] private LayerMask enemyBulletLayerMask;  // 敌人子弹的 LayerMask

    void Start()
    {
        currentHealth = maxHealth;
        scoreManager = FindObjectOfType<ScoreManager>();
        Debug.Log("Enemy health initialized to: " + currentHealth);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 判断是否属于玩家子弹层
        if (IsLayerInLayerMask(other.gameObject.layer, playerBulletLayerMask))
        {
            //TakeDamage(1);  // 扣除 Boss 的生命值
            Destroy(other.gameObject);  // 销毁玩家子弹
            Debug.Log("Player bullet hit Boss, health reduced");
        }
        // 判断是否属于敌人子弹层
        /*else if (IsLayerInLayerMask(other.gameObject.layer, enemyBulletLayerMask))
        {
            Debug.Log("Boss ignored self bullet");
        }*/
    }

    // 判断某层是否包含在 LayerMask 中
    private bool IsLayerInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
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