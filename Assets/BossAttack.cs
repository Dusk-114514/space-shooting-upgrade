using UnityEngine;

public class BossAttack : MonoBehaviour
{
    public GameObject bulletPrefab; // 子弹预制件
    public float attackInterval = 2f; // 攻击间隔（秒）
    private float lastAttackTime; // 上次攻击时间

    void Start()
    {
        Debug.Log("BossAttack initialized. Bullet Prefab assigned: " + (bulletPrefab != null));
    }

    void Update()
    {
        if (Time.time - lastAttackTime >= attackInterval)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        if (bulletPrefab != null)
        {
            Debug.Log("Boss attacking!");
            Vector3 spawnPosition = transform.position + new Vector3(0, -1f, 0); // 子弹从Boss下方生成
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -5f); // 向下移动
        }
        else
        {
            Debug.LogError("Bullet prefab not assigned!");
        }
    }
}