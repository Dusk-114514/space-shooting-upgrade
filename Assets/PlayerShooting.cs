using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject basicBulletPrefab;
    public GameObject shotgunBulletPrefab;
    public GameObject laserBulletPrefab;
    public Transform firePoint;

    [Header("Fire Rates")]
    public float basicFireRate = 0.5f;
    public float shotgunFireRate = 0.8f;
    public float laserFireRate = 0.1f;

    [Header("Bullet Speeds")]
    public float basicBulletSpeed = 10f;   // 基础子弹速度
    public float shotgunBulletSpeed = 10f; // 散弹速度
    public float laserBulletSpeed = 15f;   // 激光速度

    [Header("Weapon Settings")]
    public float shotgunSpreadAngle = 30f;
    public int shotgunBulletCount = 5;

    // 内部状态
    private int weaponLevel = 1;
    private float nextFireTime;
    public float fireRate = 0.5f;

    private Camera mainCam;

    void Start()
    {
        if (firePoint == null) firePoint = transform;
        mainCam = Camera.main;
        UpdateFireRate();
        Debug.Log("PlayerShooting initialized. Weapon Level: " + weaponLevel);
    }

    void Update()
    {
        AimAtMouse();

        if (Input.GetMouseButton(0) && Time.time > nextFireTime)
        {
            Shoot();
        }
    }

    void AimAtMouse()
    {
        if (mainCam == null) return;
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - firePoint.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
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
                    // 修改：使用变量 basicBulletSpeed
                    if (rb != null) rb.velocity = bullet.transform.up * basicBulletSpeed;
                }
                break;

            case 2: // Shotgun
                if (shotgunBulletPrefab != null)
                {
                    float startAngle = -shotgunSpreadAngle / 2f;
                    float angleStep = shotgunSpreadAngle / (shotgunBulletCount - 1);

                    for (int i = 0; i < shotgunBulletCount; i++)
                    {
                        float currentSpread = startAngle + (i * angleStep);
                        Quaternion spreadRotation = firePoint.rotation * Quaternion.Euler(0, 0, currentSpread);

                        GameObject bullet = Instantiate(shotgunBulletPrefab, firePoint.position, spreadRotation);
                        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                        // 修改：使用变量 shotgunBulletSpeed
                        if (rb != null) rb.velocity = bullet.transform.up * shotgunBulletSpeed;
                    }
                }
                break;

            case 3: // Laser
                if (laserBulletPrefab != null)
                {
                    GameObject bullet = Instantiate(laserBulletPrefab, firePoint.position, firePoint.rotation);
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    // 修改：使用变量 laserBulletSpeed
                    if (rb != null) rb.velocity = bullet.transform.up * laserBulletSpeed;
                }
                break;
        }
        nextFireTime = Time.time + fireRate;
    }

    // --- 辅助方法 ---
    void UpdateFireRate()
    {
        switch (weaponLevel)
        {
            case 1: fireRate = basicFireRate; break;
            case 2: fireRate = shotgunFireRate; break;
            case 3: fireRate = laserFireRate; break;
        }
    }

    public void SetWeapon(int level)
    {
        weaponLevel = level;
        UpdateFireRate();
        nextFireTime = Time.time;
    }

    // 你也可以扩展这个方法来升级子弹速度
    public void UpgradeFireRate(float decrement)
    {
        basicFireRate = Mathf.Max(0.1f, basicFireRate - decrement);
        shotgunFireRate = Mathf.Max(0.1f, shotgunFireRate - decrement);
        laserFireRate = Mathf.Max(0.05f, laserFireRate - decrement);
        UpdateFireRate();
        nextFireTime = Time.time;
    }
}