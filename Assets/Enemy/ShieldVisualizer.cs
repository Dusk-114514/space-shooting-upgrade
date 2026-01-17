using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShieldVisualizer : MonoBehaviour
{
    [Header("设置")]
    public EnemyHealth targetHealth; // 监听的目标 (Boss)
    public float radius = 3.5f;      // 护盾显示的半径 (要比 Boss 稍微大一点)
    public int segments = 60;        // 圆环的精细度 (段数越多越圆)
    public float lineWidth = 0.2f;   // 线条宽度

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        // 初始化 LineRenderer 的一些基础属性
        line.useWorldSpace = true;   // 使用世界坐标，方便跟随 Boss
        line.loop = false;           // 不闭合 (因为我们要画缺口)
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = 0;
    }

    private void LateUpdate()
    {
        // 如果没有目标，或者目标没有开启方向盾，就隐藏线条
        if (targetHealth == null || !targetHealth.hasDirectionalShield)
        {
            line.enabled = false;
            return;
        }

        // 开启线条并绘制
        line.enabled = true;
        DrawShieldArc();
    }

    private void DrawShieldArc()
    {
        // 读取 Boss 的护盾数据
        float weakAngle = targetHealth.vulnerableAngle; // 弱点中心角度
        float gapWidth = targetHealth.shieldWidth;      // 缺口宽度 (例如 90度)

        // 我们要画的是 "护盾" (阻挡区)，所以是 360度 - 缺口宽度
        // 护盾的起始角度 = 弱点角度 + 半个缺口宽
        float startAngle = weakAngle + (gapWidth / 2f);
        float shieldLength = 360f - gapWidth; // 护盾总共覆盖多少度

        // 计算每一段的角度增量
        float angleStep = shieldLength / segments;

        line.positionCount = segments + 1;

        Vector3 centerPos = targetHealth.transform.position;

        for (int i = 0; i <= segments; i++)
        {
            // 当前点的角度 (度 -> 弧度)
            float currentDeg = startAngle + (angleStep * i);
            float rad = currentDeg * Mathf.Deg2Rad;

            // 极坐标转笛卡尔坐标: x = r * cos(θ), y = r * sin(θ)
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;

            // 设置点的位置 (Boss中心 + 偏移)
            line.SetPosition(i, centerPos + new Vector3(x, y, 0));
        }
    }
}