using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// --- 数据定义 ---
[System.Serializable]
public class MinionGroupConfig
{
    public GameObject prefab;
    public PatternType pattern;
    public int count = 10;
    public float radius = 4f;
    public float spawnDelay = 0.1f;
    public float rotateSpeed = 30f;
}

[System.Serializable]
public class BossPhase
{
    public string phaseName = "Phase";

    [Header("核心机制")]
    public bool isInvulnerable = false;
    public bool enableMovement = false;
    [Tooltip("是否开启方向性护盾 (P3 True)")]
    public bool useDirectionalShield = false;

    [Header("结束条件")]
    public float duration = 0f;
    public bool waitUntilBossDeath = false;

    [Header("攻击配置")]
    public BulletObject danmakuConfig;
    public List<MinionGroupConfig> minionGroups;
}

[RequireComponent(typeof(EnemyHealth))]
public class BossController : MonoBehaviour
{
    [Header("循环阶段配置")]
    [SerializeField] private List<BossPhase> loopPhases;

    [Header("狂暴阶段配置")]
    [SerializeField] private BossPhase enragePhase;
    [SerializeField] private float enrageTimeLimit = 240f;

    [Header("状态监控")]
    [SerializeField] private int minionsAliveCount = 0;
    private float battleTimer = 0f;
    private bool isEnraged = false;

    private EnemyHealth bossHealth;
    private BossMovement bossMovement;
    private Sender danmakuSender;

    private Coroutine currentPhaseCoroutine;
    private Vector3 initialPosition;

    private void Awake()
    {
        bossHealth = GetComponent<EnemyHealth>();
        bossMovement = GetComponent<BossMovement>();
        danmakuSender = GetComponent<Sender>();
        if (danmakuSender == null) danmakuSender = GetComponentInChildren<Sender>();
    }

    private void Start()
    {
        initialPosition = transform.position;

        if (danmakuSender != null) danmakuSender.enabled = false;
        if (bossMovement != null) bossMovement.enabled = false;

        StartCoroutine(GlobalGameTimer());
        StartCoroutine(MainCombatLoop());
    }

    private IEnumerator GlobalGameTimer()
    {
        while (battleTimer < enrageTimeLimit)
        {
            battleTimer += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("!!! BOSS 狂暴 !!!");
        isEnraged = true;
        StopCoroutine("MainCombatLoop");
        if (currentPhaseCoroutine != null) StopCoroutine(currentPhaseCoroutine);
        StartCoroutine(RunPhase(enragePhase));
    }

    private IEnumerator MainCombatLoop()
    {
        yield return new WaitForSeconds(1f);

        int phaseIndex = 0;
        while (!isEnraged)
        {
            if (loopPhases.Count == 0) break;

            BossPhase currentPhase = loopPhases[phaseIndex % loopPhases.Count];
            currentPhaseCoroutine = StartCoroutine(RunPhase(currentPhase));
            yield return currentPhaseCoroutine;

            if (!isEnraged && bossHealth.GetCurrentHealth() > 0)
            {
                // 阶段间隙：停火、无敌、归位
                if (danmakuSender != null) danmakuSender.SetPattern(null);
                if (bossMovement != null) bossMovement.enabled = false;

                bossHealth.SetDirectionalShield(false, 0);
                bossHealth.SetInvulnerable(true); // 间隙无敌

                float transitionTime = 2.0f;
                float elapsed = 0f;
                Vector3 startPos = transform.position;

                while (elapsed < transitionTime)
                {
                    transform.position = Vector3.Lerp(startPos, initialPosition, elapsed / transitionTime);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                transform.position = initialPosition;
            }

            phaseIndex++;
        }
    }

    private IEnumerator RunPhase(BossPhase phase)
    {
        Debug.Log($"<color=cyan>>>> 启动阶段: {phase.phaseName}</color>");

        // 用于管理护盾随机变化的协程
        Coroutine shieldShuffler = null;

        // 1. 设置无敌/护盾
        if (phase.useDirectionalShield)
        {
            // 初始随机
            bossHealth.SetDirectionalShield(true, Random.Range(0f, 360f));
            // 【核心修改】启动定期洗牌协程，每隔几秒换个位置
            shieldShuffler = StartCoroutine(ShuffleShieldRoutine());
        }
        else
        {
            bossHealth.SetDirectionalShield(false, 0);
            bossHealth.SetInvulnerable(phase.isInvulnerable);
        }

        // 2. 移动
        if (bossMovement != null) bossMovement.enabled = phase.enableMovement;

        // 3. 弹幕
        if (danmakuSender != null) danmakuSender.SetPattern(phase.danmakuConfig);

        // 4. 小怪
        minionsAliveCount = 0;
        foreach (var group in phase.minionGroups)
        {
            yield return StartCoroutine(SpawnGroup(group));
        }

        // 5. 等待条件
        if (minionsAliveCount > 0)
        {
            yield return new WaitUntil(() => minionsAliveCount <= 0);
        }
        else if (phase.waitUntilBossDeath)
        {
            yield return new WaitUntil(() => bossHealth.GetCurrentHealth() <= 0);
        }
        else if (phase.duration > 0)
        {
            yield return new WaitForSeconds(phase.duration);
        }

        // 【清理】阶段结束时，记得停止护盾洗牌，否则下一阶段可能还会乱跳
        if (shieldShuffler != null) StopCoroutine(shieldShuffler);
    }

    // 【新增逻辑】每隔 4 秒重新随机一次护盾缺口
    private IEnumerator ShuffleShieldRoutine()
    {
        while (true)
        {
            // 等待时间建议和 BossMovement 的冲锋间隔（3-5秒）差不多
            yield return new WaitForSeconds(4.0f);

            // 重新随机一个角度
            float newAngle = Random.Range(0f, 360f);
            bossHealth.SetDirectionalShield(true, newAngle);

            // 这里可以加一个音效播放代码，提示玩家“弱点变了！”
        }
    }

    // --- 下面保持不变 ---
    private IEnumerator SpawnGroup(MinionGroupConfig group)
    {
        for (int i = 0; i < group.count; i++)
        {
            Vector3 spawnPos = GeometricMath.CalculatePosition(group.pattern, transform.position, i, group.count, group.radius);
            if (group.prefab != null)
            {
                GameObject minionObj = Instantiate(group.prefab, spawnPos, Quaternion.identity);
                RegisterMinion(minionObj, group, spawnPos);
            }
            if (group.spawnDelay > 0) yield return new WaitForSeconds(group.spawnDelay);
        }
    }

    private void RegisterMinion(GameObject minionObj, MinionGroupConfig group, Vector3 spawnPos)
    {
        minionsAliveCount++;
        Enemy enemyScript = minionObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetParentBoss(this);
            if (group.rotateSpeed != 0) enemyScript.ActivateSatelliteMode(this.transform, spawnPos, group.rotateSpeed);
            else enemyScript.ActivateStaticMode();
        }
    }

    public void MinionDied()
    {
        minionsAliveCount--;
        if (minionsAliveCount < 0) minionsAliveCount = 0;
    }
}