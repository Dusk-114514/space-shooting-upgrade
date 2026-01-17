using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AimedSender))]
public class SuicideMinion : MonoBehaviour
{
    [Header("行为参数")]
    public float entrySpeed = 8f;
    public float preAttackDelay = 1.0f;
    public float shootingDuration = 2.0f;
    public float chargeWindupTime = 2.0f;
    public float chargeSpeed = 18f;

    [Header("视觉效果")]
    [Tooltip("射击时的旋转速度 (度/秒)")]
    public float visualRotateSpeed = 360f; // 【新增】一秒转一圈

    [Header("组件引用")]
    public AimedSender sniper;

    private Vector3 formationPosition;
    private float chargeDelay;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        sniper = GetComponent<AimedSender>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (sniper != null) sniper.enabled = false;
        if (rb != null) rb.isKinematic = true;
    }

    public void Initialize(Vector3 targetPos, float delay)
    {
        formationPosition = targetPos;
        chargeDelay = delay;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        StartCoroutine(BehaviorRoutine());
    }

    private IEnumerator BehaviorRoutine()
    {
        // --- 阶段 1: 入场 ---
        while (Vector3.Distance(transform.position, formationPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, formationPosition, entrySpeed * Time.deltaTime);
            // 入场时也可以稍微转一点，增加动感，或者保持不动
            transform.Rotate(0, 0, 100f * Time.deltaTime);
            yield return null;
        }
        transform.position = formationPosition;

        // --- 阶段 1.5: 战术停顿 ---
        // 停顿的时候归正角度，或者继续慢转？这里演示让它停下来盯着玩家
        transform.rotation = Quaternion.identity;
        yield return new WaitForSeconds(preAttackDelay);

        // --- 阶段 2: 齐射 + 高速旋转 (核心修改) ---
        if (sniper != null) sniper.enabled = true;

        // 【修改开始】不再是死等，而是边转边等
        float shootTimer = 0f;
        while (shootTimer < shootingDuration)
        {
            shootTimer += Time.deltaTime;

            // 绕 Z 轴高速旋转
            transform.Rotate(0, 0, visualRotateSpeed * Time.deltaTime);

            yield return null;
        }
        // 【修改结束】

        if (sniper != null) sniper.enabled = false;

        // --- 阶段 3: 排队等待 ---
        // 等待期间继续旋转，还是减速？这里让它继续转，保持动能
        float waitTimer = 0f;
        while (waitTimer < chargeDelay)
        {
            waitTimer += Time.deltaTime;
            transform.Rotate(0, 0, visualRotateSpeed * 0.5f * Time.deltaTime); // 减速转
            yield return null;
        }

        // --- 阶段 4: 暖机变红 ---
        // 锁定冲锋方向
        Vector3 attackDir = Vector3.down;
        if (player != null)
        {
            attackDir = (player.position - transform.position).normalized;
        }

        // 【关键】此时强制将旋转角度修正为“朝向玩家”，结束之前的自转
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 渐变变红
        float windupTimer = 0f;
        Color startColor = Color.white;
        Color warningColor = new Color(1f, 0.2f, 0.2f, 1f);

        while (windupTimer < chargeWindupTime)
        {
            windupTimer += Time.deltaTime;
            float progress = windupTimer / chargeWindupTime;

            if (sr != null) sr.color = Color.Lerp(startColor, warningColor, progress);

            // 颤抖效果
            float shakeAmount = 0.05f * progress;
            transform.position = formationPosition + (Vector3)(Random.insideUnitCircle * shakeAmount);

            yield return null;
        }

        if (sr != null) sr.color = warningColor;
        transform.position = formationPosition;

        // --- 阶段 5: 冲锋 ---
        float chargeTimer = 0f;
        while (chargeTimer < 4f)
        {
            transform.position += attackDir * chargeSpeed * Time.deltaTime;
            chargeTimer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}