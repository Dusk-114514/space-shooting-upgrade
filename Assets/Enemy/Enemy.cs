using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum EnemyState { Idle, Satellite, StaticFloating }

    [Header("状态")]
    private EnemyState currentState = EnemyState.Idle;
    private BossController parentBoss;

    // --- 卫星模式变量 ---
    private Transform orbitCenter;
    private float orbitRadius;
    private float currentAngle;
    private float orbitSpeed;

    // --- 静态浮动模式变量 ---
    private Vector3 initialPosition;
    private float floatSpeed = 2f;
    private float floatAmount = 0.5f;

    private void Start()
    {
        // 记录初始位置，防止逻辑报错
        initialPosition = transform.position;
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Satellite:
                HandleSatelliteMovement();
                break;

            case EnemyState.StaticFloating:
                HandleFloatingMovement();
                break;
        }
    }

    // --- 1. 卫星旋转逻辑 (原生数学) ---
    private void HandleSatelliteMovement()
    {
        if (orbitCenter == null) return;

        // 增加角度
        currentAngle += orbitSpeed * Time.deltaTime;
        currentAngle %= 360f; // 保持在 0-360 度

        // 极坐标转笛卡尔坐标
        float rad = currentAngle * Mathf.Deg2Rad;
        float x = orbitCenter.position.x + orbitRadius * Mathf.Cos(rad);
        float y = orbitCenter.position.y + orbitRadius * Mathf.Sin(rad);

        transform.position = new Vector3(x, y, 0);
    }

    // --- 2. 原地浮动逻辑 (原生数学) ---
    private void HandleFloatingMovement()
    {
        // 利用 Sin 函数制造上下起伏
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
    }

    // --- 公共接口 ---

    public void SetParentBoss(BossController boss)
    {
        parentBoss = boss;
    }

    /// <summary>
    /// 激活卫星模式：绕着目标转
    /// </summary>
    public void ActivateSatelliteMode(Transform target, Vector3 startPos, float speed = 45f)
    {
        currentState = EnemyState.Satellite;
        orbitCenter = target;
        orbitSpeed = speed;

        // 计算当前的初始角度和半径，保证平滑开始
        Vector3 offset = startPos - target.position;
        orbitRadius = offset.magnitude;
        currentAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 激活静态模式：原地上下浮动
    /// </summary>
    public void ActivateStaticMode()
    {
        currentState = EnemyState.StaticFloating;
        initialPosition = transform.position; // 记录当前位置作为浮动中心
    }

    private void OnDestroy()
    {
        if (parentBoss != null)
        {
            parentBoss.MinionDied();
        }
    }
}