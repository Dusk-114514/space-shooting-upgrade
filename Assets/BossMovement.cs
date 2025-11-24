using UnityEngine;
using System.Collections;

public class BossMovement : MonoBehaviour
{
    [Header("冲锋设置")]
    public float chargeSpeed = 6f;
    public float minChargeInterval = 3f;
    public float maxChargeInterval = 5f;
    public float stayDuration = 1f;
    public float returnDuration = 1f;

    // 原位 (Boss 每次冲锋后返回的位置)
    // 我们可以让它默认为空，在启用脚本时自动记录当前位置
    private Vector3 originalPosition;

    private float nextChargeTime;
    private GameObject player;
    private Coroutine chargeCoroutine;
    private Vector3 chargeTarget;

    void Awake()
    {
        // 默认禁用自己，等待 BossController 唤醒
        // 这样可以避免在 Boss 入场(DiveDown)时意外触发冲锋
        this.enabled = false;
    }

    // 当脚本被 BossController 启用时调用 (OnEnable)
    void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // 【关键修改】不再强制瞬移，而是把当前位置（入场后的位置）记为“原点”
        // 这样 Boss 就会从 DiveDown 结束的地方开始战斗
        originalPosition = transform.position;

        // 重置下一次冲锋时间
        nextChargeTime = Time.time + Random.Range(minChargeInterval, maxChargeInterval);

        Debug.Log($"Boss 技能模块已激活。原点设定为: {originalPosition}");
    }

    void Update()
    {
        // 只有当脚本启用时，Update 才会运行
        if (Time.time >= nextChargeTime && chargeCoroutine == null && player != null)
        {
            if (Random.value < 0.3f)
            {
                chargeTarget = player.transform.position;
                chargeCoroutine = StartCoroutine(ChargeCoroutine());
                nextChargeTime = Time.time + Random.Range(minChargeInterval, maxChargeInterval);
                Debug.Log("Boss 释放冲锋技能！");
            }
        }
    }

    IEnumerator ChargeCoroutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = chargeTarget;

        // 1. 冲刺
        float distance = Vector3.Distance(startPos, targetPos);
        float dynamicRushDuration = distance / chargeSpeed;
        float rushTime = 0f;

        while (rushTime < dynamicRushDuration)
        {
            rushTime += Time.deltaTime;
            float t = rushTime / dynamicRushDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 2. 停留
        yield return new WaitForSeconds(stayDuration);

        // 3. 返回原位
        float returnTime = 0f;
        while (returnTime < returnDuration)
        {
            returnTime += Time.deltaTime;
            float t = returnTime / returnDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(transform.position, originalPosition, t);
            yield return null;
        }

        transform.position = originalPosition;
        chargeCoroutine = null;
    }
}