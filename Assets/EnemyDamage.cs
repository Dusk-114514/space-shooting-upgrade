using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("伤害设置")]
    public int damage = 1;

    [Header("行为设置")]
    [Tooltip("勾选此项：撞到玩家后销毁自己（适用于子弹）\n不勾选：撞到玩家后不销毁（适用于Boss/小怪本体）")]
    public bool destroyOnImpact = true; // 默认为 true，方便子弹使用

    // 防止重复伤害的锁
    private bool hasHit = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other.gameObject);
    }

    private void TryDealDamage(GameObject target)
    {
        if (hasHit && destroyOnImpact) return; // 如果是消耗品且已命中，就不再触发

        if (target.CompareTag("Player"))
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // 如果是子弹，标记为已命中
                if (destroyOnImpact) hasHit = true;

                // 造成伤害
                playerHealth.TakeDamage(damage);

                // 【核心修复】根据开关决定是否销毁自己
                if (destroyOnImpact)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}