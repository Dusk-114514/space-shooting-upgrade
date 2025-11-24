using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sender : MonoBehaviour
{
    // 当前使用的配置 (可以在 Inspector 看，也可以被代码修改)
    public BulletObject bullet;

    // 运行时状态变量
    private float currentAngle = 0;
    private float currentAngularVelocity = 0;
    private float currentTime = 0;

    private void Start()
    {
        // 如果一开始就有配置，初始化一下
        if (bullet != null)
        {
            ResetState();
        }
    }

    /// <summary>
    /// 【核心新功能】外部调用此方法来切换弹幕配置
    /// </summary>
    /// <param name="newConfig">新的弹幕配置文件</param>
    public void SetPattern(BulletObject newConfig)
    {
        bullet = newConfig;

        // 如果配置不为空，启用发射器并重置状态
        if (bullet != null)
        {
            ResetState();
            this.enabled = true;
        }
        else
        {
            // 如果传入 null，说明这阶段不发射弹幕，禁用脚本节省性能
            this.enabled = false;
        }
    }

    // 重置发射器的计时器和角度，确保新弹幕从头开始
    private void ResetState()
    {
        if (bullet == null) return;
        currentAngle = bullet.InitRotation;
        currentAngularVelocity = bullet.SenderAngularVelocity;
        currentTime = 0;
    }

    private void FixedUpdate()
    {
        if (bullet == null) return;

        // 计算发射器自身的旋转 (处理螺旋弹幕的关键)
        currentAngularVelocity = Mathf.Clamp(currentAngularVelocity + bullet.AngularAcceleration * Time.fixedDeltaTime, -bullet.SenderMaxAngularVelocity, bullet.SenderMaxAngularVelocity);
        currentAngle += currentAngularVelocity * Time.fixedDeltaTime;

        // 角度归一化，防止数值溢出
        if (Mathf.Abs(currentAngle) > 720f)
        {
            currentAngle -= Mathf.Sign(currentAngle) * 360f;
        }

        // 发射计时器
        currentTime += Time.fixedDeltaTime;
        if (currentTime >= bullet.SendInterval)
        {
            currentTime -= bullet.SendInterval;
            SendByCount(bullet.Count, currentAngle);
        }
    }

    private void SendByCount(int count, float angle)
    {
        // 计算起始角度 (如果是偶数发，中间留空；奇数发，中间有一发)
        float temp = count % 2 == 0 ? angle + bullet.LineAngle / 2 : angle;

        for (int i = 0; i < count; ++i)
        {
            // 计算每一发子弹的偏移
            // 这种算法会让子弹左右交替排列 (0, +angle, -angle, +2angle...)
            float finalAngle = temp + Mathf.Pow(-1, i) * Mathf.CeilToInt(i / 2f) * bullet.LineAngle;

            Send(finalAngle);
        }
    }

    private void Send(float angle)
    {
        if (bullet.prefabs == null) return;

        GameObject go = Instantiate(bullet.prefabs);
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.Euler(0, 0, angle);

        // --- 修复逻辑 ---
        // 检查预制体上是否已有 BulletBehavior，没有再添加
        BulletBehavior bh = go.GetComponent<BulletBehavior>();
        if (bh == null)
        {
            bh = go.AddComponent<BulletBehavior>();
        }

        // 【关键修复】你之前忘记调用 InitBullet 了，导致加速度等参数没传进去
        InitBullet(bh);
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