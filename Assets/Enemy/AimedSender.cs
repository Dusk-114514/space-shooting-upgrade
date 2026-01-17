using UnityEngine;

public class AimedSender : MonoBehaviour
{
    public BulletObject bullet;

    private float currentTime = 0;
    private Transform playerTransform;

    private void OnEnable()
    {
        FindPlayer();
        currentTime = 0;
    }

    private void FixedUpdate()
    {
        if (bullet == null) return;

        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        currentTime += Time.fixedDeltaTime;
        if (currentTime >= bullet.SendInterval)
        {
            currentTime -= bullet.SendInterval;
            ShootAtPlayer();
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    public void SetPattern(BulletObject newConfig)
    {
        bullet = newConfig;
        if (bullet != null)
        {
            this.enabled = true;
            FindPlayer();
        }
        else
        {
            this.enabled = false;
        }
    }

    private void ShootAtPlayer()
    {
        if (playerTransform == null) return;

        Vector2 direction = playerTransform.position - transform.position;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        int count = bullet.Count;
        float angleStep = bullet.LineAngle;
        float startAngle = baseAngle - (angleStep * (count - 1)) / 2f;

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            CreateBullet(currentAngle);
        }
    }

    private void CreateBullet(float angle)
    {
        if (bullet.prefabs == null) return;

        // 旋转子弹 (不减90度，因为子弹朝右飞)
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject go = Instantiate(bullet.prefabs, transform.position, rotation);

        // 1. 设置运动
        BulletBehavior bh = go.GetComponent<BulletBehavior>();
        if (bh == null) bh = go.AddComponent<BulletBehavior>();

        bh.LinearVelocity = bullet.LinearVelocity;
        bh.Acceleration = bullet.Acceleration;
        bh.LifeTime = bullet.LifeCycle;
        bh.MaxVelocity = bullet.maxVelocity;
        bh.AngularVelocity = 0;
        bh.AngularAcceleration = 0;

        // 2. 【新增】设置伤害
        EnemyDamage ed = go.GetComponent<EnemyDamage>();
        if (ed != null)
        {
            ed.damage = bullet.damage;
        }
    }
}