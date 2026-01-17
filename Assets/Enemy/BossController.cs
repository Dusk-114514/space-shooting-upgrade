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
    public bool useDirectionalShield = false;

    [Header("P1 特殊机制 (神风特攻)")]
    public bool spawnSuicideSquad = false;
    public int suicideSquadCount = 4;       // 单侧数量
    public float suicideSquadInterval = 3f; // 上一波死光后，等待多久刷下一波

    [Header("P2 特殊机制 (符文)")]
    [Tooltip("定义符文生成坐标，列表为空则不生成")]
    public List<Vector3> runePositions;

    [Header("结束条件")]
    public float duration = 0f;
    public bool waitUntilBossDeath = false;

    [Header("攻击配置")]
    [Tooltip("普通/螺旋弹幕 (None则停火)")]
    public BulletObject danmakuConfig;
    [Tooltip("自机狙 (None则停火)")]
    public BulletObject sniperConfig;

    [Header("小怪配置")]
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

    [Header("通用预制体")]
    [SerializeField] private GameObject runePrefab;
    [SerializeField] private GameObject suicideMinionPrefab;

    [Header("状态监控")]
    [SerializeField] private int minionsAliveCount = 0;
    private float battleTimer = 0f;
    private bool isEnraged = false;

    private EnemyHealth bossHealth;
    private BossMovement bossMovement;

    // 发射器引用
    private Sender normalSender;
    private AimedSender sniperSender;

    private Coroutine currentPhaseCoroutine;
    private Vector3 initialPosition;

    // --- 【修改点1】添加所有生成物的追踪列表，用于阶段切换时的清理 ---
    private List<GameObject> activeRunes = new List<GameObject>();
    private List<GameObject> activeSuicideMinions = new List<GameObject>();
    private List<GameObject> activeStandardMinions = new List<GameObject>(); // 新增：追踪普通小怪

    private void Awake()
    {
        bossHealth = GetComponent<EnemyHealth>();
        bossMovement = GetComponent<BossMovement>();

        normalSender = GetComponentInChildren<Sender>();
        sniperSender = GetComponentInChildren<AimedSender>();

        if (normalSender == null) Debug.LogError("❌ BossController 没找到 Sender 组件！请检查它是否挂在 Boss 或其子物体上。");
        else Debug.Log("✅ BossController 成功连接 Sender。");

        if (sniperSender == null) Debug.LogError("❌ BossController 没找到 AimedSender 组件！请检查挂载。");
        else Debug.Log("✅ BossController 成功连接 AimedSender。");
    }

    private void Start()
    {
        initialPosition = transform.position;

        if (normalSender != null) normalSender.SetPattern(null);
        if (sniperSender != null) sniperSender.SetPattern(null);
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
        if (currentPhaseCoroutine != null)
        {
            StopCoroutine(currentPhaseCoroutine);
        }
        // 狂暴前也清理一次战场
        CleanupAllEntities();
        StartCoroutine(RunPhase(enragePhase));
    }

    private IEnumerator MainCombatLoop()
    {
        // 进场缓冲
        yield return new WaitForSeconds(1f);

        int phaseIndex = 0;
        while (!isEnraged)
        {
            if (loopPhases.Count == 0) break;

            BossPhase currentPhase = loopPhases[phaseIndex % loopPhases.Count];
            currentPhaseCoroutine = StartCoroutine(RunPhase(currentPhase));
            yield return currentPhaseCoroutine;

            // --- 阶段间隙 (Transition) ---
            if (!isEnraged && bossHealth.GetCurrentHealth() > 0)
            {
                // 1. 关闭攻击和移动
                if (normalSender != null) normalSender.SetPattern(null);
                if (sniperSender != null) sniperSender.SetPattern(null);
                if (bossMovement != null) bossMovement.enabled = false;

                // 2. 开启间隙无敌
                bossHealth.SetDirectionalShield(false, 0);
                bossHealth.SetInvulnerable(true);

                // 3. 【修改点2】彻底清理上一阶段的所有残留物 (小怪、自爆怪、符文)
                CleanupAllEntities();

                // 4. Boss 归位动画
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

        Coroutine shieldShuffler = null;
        Coroutine suicideSquadLoop = null;

        // 1. 设置机制
        if (phase.useDirectionalShield)
        {
            bossHealth.SetDirectionalShield(true, Random.Range(0f, 360f));
            shieldShuffler = StartCoroutine(ShuffleShieldRoutine());
        }
        else
        {
            bossHealth.SetDirectionalShield(false, 0);
            bossHealth.SetInvulnerable(phase.isInvulnerable);
        }

        if (bossMovement != null)
        {
            bossMovement.enabled = phase.enableMovement;
        }

        // 2. 设置发射器
        if (normalSender != null) normalSender.SetPattern(phase.danmakuConfig);
        if (sniperSender != null) sniperSender.SetPattern(phase.sniperConfig);

        // 3. 生成普通小怪
        minionsAliveCount = 0;
        foreach (var group in phase.minionGroups)
        {
            yield return StartCoroutine(SpawnGroup(group));
        }

        // 4. 启动神风特攻循环
        if (phase.spawnSuicideSquad)
        {
            suicideSquadLoop = StartCoroutine(SuicideSquadLoop(phase.suicideSquadCount, phase.suicideSquadInterval));
        }

        // 5. 生成符文
        if (phase.runePositions != null && phase.runePositions.Count > 0)
        {
            SpawnRunes(phase.runePositions);
        }

        // 6. 等待结束条件
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

        // 阶段自然结束时的清理
        if (shieldShuffler != null) StopCoroutine(shieldShuffler);
        if (suicideSquadLoop != null) StopCoroutine(suicideSquadLoop);

        // 注意：实体清理工作交给了 MainCombatLoop 中的 Transition 部分统一处理
    }

    // --- 神风特攻队逻辑 ---
    private IEnumerator SuicideSquadLoop(int count, float interval)
    {
        yield return StartCoroutine(SpawnSuicideSquad(count));

        while (true)
        {
            // 清理 list 中的空引用
            activeSuicideMinions.RemoveAll(item => item == null);

            if (activeSuicideMinions.Count > 0)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(interval);
            yield return StartCoroutine(SpawnSuicideSquad(count));
        }
    }

    private IEnumerator SpawnSuicideSquad(int countPerSide)
    {
        if (suicideMinionPrefab == null) yield break;

        Vector3 leftSpawn = new Vector3(-12f, -8f, 0f);
        Vector3 rightSpawn = new Vector3(12f, -8f, 0f);

        float startX = 1.5f;
        float spacing = 1.5f;
        float targetY = 3.5f;

        for (int i = 0; i < countPerSide; i++)
        {
            // --- 左边 ---
            GameObject lMinion = Instantiate(suicideMinionPrefab, leftSpawn, Quaternion.identity);
            SuicideMinion lScript = lMinion.GetComponent<SuicideMinion>();
            if (lScript != null)
            {
                Vector3 targetPos = new Vector3(-(startX + i * spacing), targetY, 0f);
                lScript.Initialize(targetPos, i * 0.4f);
            }
            activeSuicideMinions.Add(lMinion); // 注册追踪

            // --- 右边 ---
            GameObject rMinion = Instantiate(suicideMinionPrefab, rightSpawn, Quaternion.identity);
            SuicideMinion rScript = rMinion.GetComponent<SuicideMinion>();
            if (rScript != null)
            {
                Vector3 targetPos = new Vector3(startX + i * spacing, targetY, 0f);
                rScript.Initialize(targetPos, i * 0.4f);
            }
            activeSuicideMinions.Add(rMinion); // 注册追踪

            yield return new WaitForSeconds(0.2f);
        }
    }

    // --- 辅助方法 ---

    private void SpawnRunes(List<Vector3> positions)
    {
        if (runePrefab == null) return;
        foreach (Vector3 pos in positions)
        {
            GameObject rune = Instantiate(runePrefab, pos, Quaternion.identity);
            activeRunes.Add(rune);
        }
    }

    // 【修改点3】统一清理所有实体的方法
    private void CleanupAllEntities()
    {
        // 1. 清理符文
        foreach (var rune in activeRunes) { if (rune != null) Destroy(rune); }
        activeRunes.Clear();

        // 2. 清理自爆小怪
        foreach (var minion in activeSuicideMinions) { if (minion != null) Destroy(minion); }
        activeSuicideMinions.Clear();

        // 3. 清理普通小怪
        foreach (var minion in activeStandardMinions) { if (minion != null) Destroy(minion); }
        activeStandardMinions.Clear();

        // 4. 重置计数器，防止逻辑卡死
        minionsAliveCount = 0;
    }

    private IEnumerator ShuffleShieldRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(4.0f);
            float newAngle = Random.Range(0f, 360f);
            bossHealth.SetDirectionalShield(true, newAngle);
        }
    }

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

        // 【修改点4】将生成的普通小怪也加入列表
        activeStandardMinions.Add(minionObj);

        Enemy enemyScript = minionObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetParentBoss(this);
            if (group.rotateSpeed != 0)
            {
                enemyScript.ActivateSatelliteMode(this.transform, spawnPos, group.rotateSpeed);
            }
            else
            {
                enemyScript.ActivateStaticMode();
            }
        }
    }

    public void MinionDied()
    {
        minionsAliveCount--;
        if (minionsAliveCount < 0) minionsAliveCount = 0;
    }
}