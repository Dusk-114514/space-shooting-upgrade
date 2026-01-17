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
    public float basicBulletSpeed = 10f;
    public float shotgunBulletSpeed = 10f;
    public float laserBulletSpeed = 15f;

    [Header("Weapon Settings")]
    public float shotgunSpreadAngle = 30f;
    public int shotgunBulletCount = 5;

    private int weaponLevel = 1;
    private float nextFireTime;
    public float fireRate = 0.5f;

    private Camera mainCam;

    void Start()
    {
        if (firePoint == null) firePoint = transform;
        mainCam = Camera.main;
        UpdateFireRate();
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
                        if (rb != null) rb.velocity = bullet.transform.up * shotgunBulletSpeed;
                    }
                }
                break;

            case 3: // Laser
                if (laserBulletPrefab != null)
                {
                    GameObject bullet = Instantiate(laserBulletPrefab, firePoint.position, firePoint.rotation);
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.velocity = bullet.transform.up * laserBulletSpeed;
                }
                break;
        }
        nextFireTime = Time.time + fireRate;
    }

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

    // 【核心修改】增加 breakLimit 参数，允许突破射速下限
    public void UpgradeFireRate(float decrement, bool breakLimit = false)
    {
        // 如果 breakLimit 为 true，下限极低 (0.01f)，否则保持正常下限
        float minBasic = breakLimit ? 0.01f : 0.1f;
        float minShotgun = breakLimit ? 0.01f : 0.1f;
        float minLaser = breakLimit ? 0.01f : 0.05f;

        basicFireRate = Mathf.Max(minBasic, basicFireRate - decrement);
        shotgunFireRate = Mathf.Max(minShotgun, shotgunFireRate - decrement);
        laserFireRate = Mathf.Max(minLaser, laserFireRate - decrement);

        UpdateFireRate();
        Debug.Log($"射速升级！BreakLimit: {breakLimit}, 当前射速间隔: {fireRate}");
    }
}