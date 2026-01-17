using UnityEngine;

public enum PatternType
{
    Circle,     // 圆形
    Spiral,     // 螺旋
    Linear,     // 线性/横排
    RandomRing  // 随机环
}

public static class GeometricMath
{
    public static Vector3 CalculatePosition(PatternType type, Vector3 center, int index, int totalCount, float radius, float angleOffset = 0f)
    {
        switch (type)
        {
            case PatternType.Circle:
                return GetCirclePos(center, index, totalCount, radius, angleOffset);

            case PatternType.Spiral:
                float spiralRadius = radius + (index * 0.2f);
                return GetCirclePos(center, index, totalCount, spiralRadius, angleOffset + (index * 10f));

            case PatternType.Linear:
                float length = radius * 2;
                float step = length / (totalCount > 1 ? totalCount - 1 : 1);
                float x = -radius + (step * index);
                return center + new Vector3(x, 0, 0);

            case PatternType.RandomRing:
                float randomAngle = Random.Range(0f, 360f);
                return GetCirclePos(center, 0, 1, radius, randomAngle);

            default:
                return center;
        }
    }

    private static Vector3 GetCirclePos(Vector3 center, int index, int total, float radius, float startAngle)
    {
        float segment = 360f / total;
        float angleDeg = startAngle + (segment * index);
        float angleRad = angleDeg * Mathf.Deg2Rad;

        float x = center.x + radius * Mathf.Cos(angleRad);
        float y = center.y + radius * Mathf.Sin(angleRad);

        return new Vector3(x, y, 0);
    }
}