using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    [Header("战斗参数")]
    // 1. 集成点：在这里定义伤害，不再依赖 EnemyDamage
    public int damage = 1;

    [Header("运动参数")]
    public float LinearVelocity = 0f;
    public float Acceleration = 0f;
    public float AngularVelocity = 0f;
    public float AngularAcceleration = 0f;
    public float MaxVelocity = 10f;
    public float LifeTime = 5f;

    private void FixedUpdate()
    {
        // 运动逻辑保持不变
        LinearVelocity += Mathf.Clamp(Acceleration * Time.fixedDeltaTime, -MaxVelocity, MaxVelocity);
        AngularVelocity += AngularAcceleration * Time.fixedDeltaTime;
        transform.Translate(LinearVelocity * Vector2.right * Time.fixedDeltaTime, Space.Self);
        transform.rotation *= Quaternion.Euler(0, 0, AngularVelocity * Time.fixedDeltaTime);

        LifeTime -= Time.fixedDeltaTime;
        if (LifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}