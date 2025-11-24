using UnityEngine;

public class Bullet : MonoBehaviour
{
    private ScoreManager scoreManager;
    public int damage = 1;

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        gameObject.layer = LayerMask.NameToLayer("PlayerBullet");

        // 2秒后自毁 (对于全方位射击，时间销毁是最简单的边界处理方式)
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        // 【已删除】原有的高度限制: if (transform.position.y > 6f) Destroy(gameObject);
        // 现在不需要这一行了，因为我们要允许子弹向任何方向飞行
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SmallEnemy") || other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}