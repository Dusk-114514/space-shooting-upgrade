using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float moveSpeed = 2f; // 移动速度
    public float moveRange = 8f; // 移动范围
    public int health = 5; // BOSS生命值
    private float startX; // 初始X位置
    private int direction = 1; // 移动方向

    void Start()
    {
        startX = transform.position.x;
        Debug.Log("BossMovement initialized. Start X: " + startX + ", Health: " + health);
    }

    void Update()
    {
        // 计算目标位置
        float newX = transform.position.x + (moveSpeed * direction * Time.deltaTime);

        // 限制移动范围
        if (newX > startX + moveRange || newX < startX - moveRange)
        {
            direction *= -1;
            Debug.Log("Boss hit boundary, reversing direction to: " + direction);
        }

        // 更新位置
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Boss triggered with: " + other.gameObject.name);
        if (other.CompareTag("Bullet"))
        {
            health--;
            Debug.Log("Boss health reduced to: " + health);
            Destroy(other.gameObject);
            if (health <= 0)
            {
                Debug.Log("Boss health reached 0, destroying Boss.");
                Destroy(gameObject);
            }
        }
    }
}