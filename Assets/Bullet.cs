using UnityEngine;

public class Bullet : MonoBehaviour
{
    private ScoreManager scoreManager;

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        Destroy(gameObject, 2f); // 添加2秒销毁作为备用，防止遗漏
    }

    void Update()
    {
        // 如果子弹超出屏幕上边界，销毁
        if (transform.position.y > 6f) // 调整范围以匹配你的屏幕
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) // 假设Boss有"Enemy"标签
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1); // 每次击中减少1点生命
            }
        }
    }
}