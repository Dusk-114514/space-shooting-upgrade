using UnityEngine;

public class Bullet : MonoBehaviour
{
    private ScoreManager scoreManager;
    public int damage = 1; // 伤害值

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        gameObject.layer = LayerMask.NameToLayer("PlayerBullet"); // 设置玩家子弹 Layer
        Destroy(gameObject, 2f); // 2秒自毁
    }

    void Update()
    {
        if (transform.position.y > 6f) Destroy(gameObject); // 超出销毁
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查小怪或 Boss 标签
        if (other.CompareTag("SmallEnemy") || other.CompareTag("Enemy")) // 小怪 "SmallEnemy", Boss "Enemy"
        {
            // 伤害由 EnemyHealth 的 LayerMask 处理, 此处只销毁子弹
            Destroy(gameObject);
        }
    }
}