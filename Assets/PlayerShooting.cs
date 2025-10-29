using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab; // 子弹预制体，Inspector中赋值
    public Transform firePoint;     // 发射点，Inspector中赋值
    public float bulletSpeed = 10f; // 子弹速度
    public float fireRate = 0.5f;   // 每秒发射频率
    private float nextFireTime;     // 下次可以射击的时间

    void Update()
    {
        // 按空格键发射子弹
        if (Input.GetKey(KeyCode.Space) && Time.time > nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // 实例化子弹
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.up * bulletSpeed; // 子弹向上移动
    }
}