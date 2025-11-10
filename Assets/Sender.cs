using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sender : MonoBehaviour
{
    public BulletObject bullet;
    float currentAngle = 0;
    float currentAngularVelocity = 0;
    float currentTime = 0;
    private void Awake()
    {
        currentAngle = bullet.InitRotation;
        currentAngularVelocity = bullet.SenderAngularVelocity;
    }
    private void FixedUpdate()
    {
        currentAngularVelocity = Mathf.Clamp(currentAngularVelocity + bullet.AngularAcceleration * Time.fixedDeltaTime, -bullet.SenderMaxAngularVelocity, bullet.SenderMaxAngularVelocity);
        currentAngle += currentAngularVelocity * Time.fixedDeltaTime;
        if (Mathf.Abs(currentAngle) > 720f)
        {
            currentAngle -= Mathf.Sign(currentAngle) * 360f;
        }
        currentTime += Time.fixedDeltaTime;
        if (currentTime >= bullet.SendInterval)
        {
            currentTime -= bullet.SendInterval;

            SendByCount(bullet.Count, currentAngle);
        }
    }

    private void SendByCount(int count,float angle)
    {
        float temp = count % 2 == 0 ? angle + bullet.LineAngle / 2 : angle;

        for (int i =0; i < count; ++i)
        {
            temp += Mathf.Pow(-1, i) * i * bullet.LineAngle;

            Send(temp);
        }
    }
    private void Send(float angle)
    {
        GameObject go = Instantiate(bullet.prefabs);
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.Euler(0, 0, angle);
        var bh = go.AddComponent<BulletBehavior>();
        bh.LinearVelocity = bullet.LinearVelocity;
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