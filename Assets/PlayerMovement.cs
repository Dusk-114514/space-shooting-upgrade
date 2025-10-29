using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 获取Rigidbody2D组件
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on Player!");
        }
    }

    void Update()
    {
        // 获取输入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 计算移动方向和速度
        Vector2 movement = new Vector2(moveX, moveY).normalized * moveSpeed;

        // 应用到Rigidbody2D的velocity
        if (rb != null)
        {
            rb.velocity = new Vector2(movement.x, movement.y); // 只控制水平和垂直速度
        }

        // 边界限制
        Vector2 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -8f, 8f);
        pos.y = Mathf.Clamp(pos.y, -4.5f, 4.5f);
        transform.position = pos;
    }
}