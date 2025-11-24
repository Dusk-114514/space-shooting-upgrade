using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject basicBulletPrefab; // 基本子弹预制体
    public GameObject shotgunBulletPrefab; // 霰弹枪子弹预制体
    public GameObject laserBulletPrefab; // 激光子弹预制体
    public Transform firePoint; // 发射点
    public float basicFireRate = 0.5f; // 基本射速 (可升级)
    public float shotgunFireRate = 0.8f; // 霰弹枪射速 (可升级)
    public float laserFireRate = 0.1f; // 激光射速 (可升级)
    public float shotgunSpreadAngle = 30f; // 霰弹枪扩散角
    public int shotgunBulletCount = 5; // 霰弹枪子弹数量
    private int weaponLevel = 1; // 1=Basic, 2=Shotgun, 3=Laser
    private float nextFireTime;
    public float fireRate = 0.5f; // 统一射速，用于外部读取

    void Start()
    {
        if (firePoint == null) firePoint = transform;
        UpdateFireRate(); // 初始化射速
        Debug.Log("PlayerShooting initialized. Weapon Level: " + weaponLevel + ", Fire Rate: " + fireRate);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time > nextFireTime)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        switch (weaponLevel)
        {
            case 1: // Basic
                if (basicBulletPrefab != null)
                {
                    GameObject bullet = Instantiate(basicBulletPrefab, firePoint.position, firePoint.rotation);
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.velocity = Vector2.up * 10f;
                    Debug.Log("Basic bullet fired");
                }
                break;
            case 2: // Shotgun
                if (shotgunBulletPrefab != null)
                {
                    for (int i = 0; i < shotgunBulletCount; i++)
                    {
                        float angle = -shotgunSpreadAngle / 2 + (i * shotgunSpreadAngle / (shotgunBulletCount - 1));
                        Quaternion spreadRotation = Quaternion.AngleAxis(angle, Vector3.forward) * firePoint.rotation;
                        GameObject bullet = Instantiate(shotgunBulletPrefab, firePoint.position, spreadRotation);
                        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                        if (rb != null) rb.velocity = spreadRotation * Vector2.up * 10f;
                    }
                    Debug.Log("Shotgun bullets fired");
                }
                break;
            case 3: // Laser
                if (laserBulletPrefab != null)
                {
                    GameObject bullet = Instantiate(laserBulletPrefab, firePoint.position, firePoint.rotation);
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.velocity = Vector2.up * 10f;
                    Debug.Log("Laser bullet fired");
                }
                break;
        }
        nextFireTime = Time.time + fireRate; // 射击后立即重置时间
    }

    void UpdateFireRate()
    {
        switch (weaponLevel)
        {
            case 1:
                fireRate = basicFireRate;
                break;
            case 2:
                fireRate = shotgunFireRate;
                break;
            case 3:
                fireRate = laserFireRate;
                break;
        }
        Debug.Log("Fire rate updated to: " + fireRate + " for weapon level " + weaponLevel);
    }

    public void SetWeapon(int level)
    {
        weaponLevel = level;
        UpdateFireRate();
        nextFireTime = Time.time; // 重置计时器, 允许立即射击
        Debug.Log("Weapon level set to: " + level + ", Fire Rate: " + fireRate + ", Ready to fire immediately");
    }

    public void UpgradeFireRate(float decrement)
    {
        // 全局升级所有武器射速 (每级减少 decrement)
        basicFireRate = Mathf.Max(0.1f, basicFireRate - decrement);
        shotgunFireRate = Mathf.Max(0.1f, shotgunFireRate - decrement);
        laserFireRate = Mathf.Max(0.05f, laserFireRate - decrement);
        UpdateFireRate(); // 更新当前武器火速
        nextFireTime = Time.time; // 重置计时器
        Debug.Log("All weapons fire rate upgraded by " + decrement + ", Current: " + fireRate);
    }
}