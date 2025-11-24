using UnityEngine;

public class DownShooterEnemy : MonoBehaviour
{
    public GameObject enemyBulletPrefab; // 敌方子弹预制体
    public float shootInterval = 3f; // 射击间隔
    public float bulletSpeed = 4f; // 子弹速度
    private float nextShootTime;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.down * 2f; // 垂直下落
        nextShootTime = Time.time + shootInterval;
        gameObject.tag = "SmallEnemy"; // 设置小怪标签
    }

    void Update()
    {
        if (Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootInterval;
        }
    }

    void Shoot()
    {
        if (enemyBulletPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, -0.5f, 0);
            GameObject bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().velocity = Vector2.down * -bulletSpeed;
        }
    }

    // EnemyHealth 已处理击杀
}