using UnityEngine;

public class HitPoint : MonoBehaviour
{
    private PlayerHealth playerHealth;

    void Start()
    {
        // 获取父物体（玩家）身上的血量组件
        playerHealth = GetComponentInParent<PlayerHealth>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 尝试获取撞到的物体是不是子弹 (有没有 BulletBehavior)
        BulletBehavior bullet = other.GetComponent<BulletBehavior>();

        if (bullet != null)
        {
            // 找到了子弹脚本！
            if (playerHealth != null)
            {
                // 2. 集成点：读取子弹的 damage，传递给 PlayerHealth
                Debug.Log($"<color=red>判定点命中！伤害: {bullet.damage}</color>");
                playerHealth.TakeDamage(bullet.damage);

                // 3. 销毁子弹
                Destroy(other.gameObject);
            }
        }
        // 兼容旧逻辑：如果有些子弹只有 Tag 没有脚本
        else if (other.CompareTag("EnemyBullet"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                Destroy(other.gameObject);
            }
        }
    }

    // 辅助显示判定范围 (Scene窗口可见)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float r = GetComponent<CircleCollider2D>() ? GetComponent<CircleCollider2D>().radius : 0.05f;
        Gizmos.DrawWireSphere(transform.position, r);
    }
}