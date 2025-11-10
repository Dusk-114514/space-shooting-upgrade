using UnityEngine;
using System.Collections;

public class BossMovement : MonoBehaviour
{
    public float chargeSpeed = 6f; // 突刺速度 (控制冲刺时间 = 距离 / 速度)
    public float minChargeInterval = 3f; // 最小突刺间隔 (秒, 随机间隔下限)
    public float maxChargeInterval = 5f; // 最大突刺间隔 (秒, 随机间隔上限)
    public float stayDuration = 1f; // 停留时间 (秒)
    public float returnDuration = 1f; // 退回时间 (秒)
    public float smoothTime = 0.5f; // 平滑时间 (秒, Lerp 曲线时间, 控制加速/减速平滑度)
    private Vector3 originalPosition; // 原位 (固定中心上部)
    private float nextChargeTime; // 下次突刺时间
    private GameObject player; // 玩家对象引用
    private Coroutine chargeCoroutine; // 当前突刺协程
    private Vector3 chargeTarget; // 固定突刺目标位置 (捕获玩家位置)

    void Start()
    {
        originalPosition = new Vector3(0f, 4.5f, 0f); // 固定中心上部 (X=0, Y=4.5f)
        transform.position = originalPosition;
        player = GameObject.FindGameObjectWithTag("Player"); // 假设 Player Tag = "Player"
        nextChargeTime = Time.time + Random.Range(minChargeInterval, maxChargeInterval); // 随机首次突刺时间
        Debug.Log("BossMovement initialized. Fixed Position: " + originalPosition + ", Next Charge in " + (nextChargeTime - Time.time) + "s");
    }

    void Update()
    {
        // 只检查突刺触发
        if (Time.time >= nextChargeTime && chargeCoroutine == null && player != null)
        {
            if (Random.value < 0.3f) // 30% 概率触发
            {
                chargeTarget = player.transform.position; // 立即捕获玩家当前精确位置 (X,Y)
                chargeCoroutine = StartCoroutine(ChargeCoroutine());
                nextChargeTime = Time.time + Random.Range(minChargeInterval, maxChargeInterval); // 随机下一间隔
                Debug.Log("Boss Charge triggered to target: " + chargeTarget + "! Next in " + (nextChargeTime - Time.time) + "s");
            }
        }
    }

    IEnumerator ChargeCoroutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = chargeTarget; // 直接使用捕获的玩家位置 (X,Y, 无调整)
        // 阶段1: 冲刺 (动态时间 = 距离 / chargeSpeed)
        float distance = Vector3.Distance(startPos, targetPos);
        float dynamicRushDuration = distance / chargeSpeed; // 基于速度计算时间 (e.g., 10 units / 6f = 1.67s)
        float rushTime = 0f;
        while (rushTime < dynamicRushDuration)
        {
            rushTime += Time.deltaTime;
            float t = rushTime / dynamicRushDuration; // 0 to 1
            t = Mathf.SmoothStep(0f, 1f, t); // 平滑加速
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        // 阶段2: 停留 (stayDuration 秒)
        yield return new WaitForSeconds(stayDuration);
        Debug.Log("Boss Charge: Reached target, staying for " + stayDuration + "s");
        // 阶段3: 平滑退回原位 (returnDuration 秒)
        float returnTime = 0f;
        while (returnTime < returnDuration)
        {
            returnTime += Time.deltaTime;
            float t = returnTime / returnDuration; // 0 to 1
            t = Mathf.SmoothStep(0f, 1f, t); // 平滑退回
            transform.position = Vector3.Lerp(transform.position, originalPosition, t);
            yield return null;
        }
        transform.position = originalPosition; // 强制回原位
        chargeCoroutine = null;
        Debug.Log("Boss Charge completed, returned to original position");
    }
}