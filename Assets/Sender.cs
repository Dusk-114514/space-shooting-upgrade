using UnityEngine;

public class Sender : MonoBehaviour
{
    public BulletObject bullet;

    private float currentAngle = 0;
    private float currentAngularVelocity = 0;
    private float currentTime = 0;

    private void Awake()
    {
        // 初始自我禁用，防止Inspector配置残留导致开局乱射
        this.enabled = false;
    }

    public void SetPattern(BulletObject newConfig)
    {
        bullet = newConfig;

        if (bullet != null)
        {
            this.enabled = true;
            ResetState();
        }
        else
        {
            this.enabled = false;
            currentTime = 0;
        }
    }

    private void ResetState()
    {
        if (bullet == null) return;
        currentAngle = bullet.InitRotation;
        currentAngularVelocity = bullet.SenderAngularVelocity;
        currentTime = bullet.SendInterval;
    }

    private void FixedUpdate()
    {
        if (bullet == null) return;

        currentAngularVelocity = Mathf.Clamp(
            currentAngularVelocity + bullet.AngularAcceleration * Time.fixedDeltaTime,
            -bullet.SenderMaxAngularVelocity,
            bullet.SenderMaxAngularVelocity
        );

        currentAngle += currentAngularVelocity * Time.fixedDeltaTime;

        if (Mathf.Abs(currentAngle) > 720f) currentAngle -= Mathf.Sign(currentAngle) * 360f;

        currentTime += Time.fixedDeltaTime;
        if (currentTime >= bullet.SendInterval)
        {
            currentTime -= bullet.SendInterval;
            SendByCount(bullet.Count, currentAngle);
        }
    }

    private void SendByCount(int count, float angle)
    {
        if (count <= 0) return;
        float temp = count % 2 == 0 ? angle + bullet.LineAngle / 2 : angle;

        for (int i = 0; i < count; ++i)
        {
            float finalAngle = temp + Mathf.Pow(-1, i) * Mathf.CeilToInt(i / 2f) * bullet.LineAngle;
            Send(finalAngle);
        }
    }

    private void Send(float angle)
    {
        if (bullet.prefabs == null) return;

        GameObject go = Instantiate(bullet.prefabs, transform.position, Quaternion.Euler(0, 0, angle));

        // 1. 设置运动参数
        BulletBehavior bh = go.GetComponent<BulletBehavior>();
        if (bh == null) bh = go.AddComponent<BulletBehavior>();
        InitBullet(bh);

        // 2. 【新增】设置伤害参数
        EnemyDamage ed = go.GetComponent<EnemyDamage>();
        if (ed != null)
        {
            ed.damage = bullet.damage;
        }
    }

    private void InitBullet(BulletBehavior bh)
    {
        bh.LinearVelocity = bullet.LinearVelocity;
        bh.Acceleration = bullet.Acceleration;
        bh.AngularAcceleration = bullet.AngularAcceleration;
        bh.AngularVelocity = bullet.AngularVelocity;
        bh.LifeTime = bullet.LifeCycle;
        bh.MaxVelocity = bullet.maxVelocity;
    }
}