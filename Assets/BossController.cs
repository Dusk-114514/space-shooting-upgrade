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
    private List<GameObject> activeRunes = new List<GameObject>();
    // 追踪活跃的神风小怪
    private List<GameObject> activeSuicideMinions = new List<GameObject>();

    private void Awake()
    {
        bossHealth = GetComponent<EnemyHealth>();
        bossMovement = GetComponent<BossMovement>();

        // 【核心修复】改为 GetComponentInChildren，这样即便挂在子物体上也能找到
        normalSender = GetComponentInChildren<Sender>();
        sniperSender = GetComponentInChildren<AimedSender>();

        // 增加两句调试日志，确保找到了
        if (normalSender == null) Debug.LogError("❌ BossController 没找到 Sender 组件！请检查它是否挂在 Boss 或其子物体上。");
        else Debug.Log("✅ BossController 成功连接 Sender。");

        if (sniperSender == null) Debug.LogError("❌ BossController 没找到 AimedSender 组件！请检查挂载。");
        else Debug.Log("✅ BossController 成功连接 AimedSender。");
    }

    private void Start()
    {
        initialPosition = transform.position;

        // 【游戏开始】强制关闭所有发射器，确保干净的开局
        if (normalSender != null)
        {
            normalSender.SetPattern(null);
        }
        if (sniperSender != null)
        {
            sniperSender.SetPattern(null);
        }
        if (bossMovement != null)
        {
            bossMovement.enabled = false;
        }

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

            // --- 阶段间隙 (休息时间) ---
            if (!isEnraged && bossHealth.GetCurrentHealth() > 0)
            {
                // 【核心】间隙期间，强制设为 null，确保 Sender 闭嘴
                if (normalSender != null) normalSender.SetPattern(null);
                if (sniperSender != null) sniperSender.SetPattern(null);
                if (bossMovement != null) bossMovement.enabled = false;

                // 开启间隙无敌
                bossHealth.SetDirectionalShield(false, 0);
                bossHealth.SetInvulnerable(true);

                // 清理上一阶段的残留物
                ClearRunes();

                // Boss 归位动画
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

        // 1. 设置机制 (无敌/护盾)
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

        // 2. 【核心】根据配置设置发射器
        if (normalSender != null)
        {
            normalSender.SetPattern(phase.danmakuConfig);
        }
        if (sniperSender != null)
        {
            sniperSender.SetPattern(phase.sniperConfig);
        }

        // 3. 生成护盾小怪
        minionsAliveCount = 0;
        foreach (var group in phase.minionGroups)
        {
            yield return StartCoroutine(SpawnGroup(group));
        }

        // 4. 启动神风特攻循环 (P1)
        if (phase.spawnSuicideSquad)
        {
            suicideSquadLoop = StartCoroutine(SuicideSquadLoop(phase.suicideSquadCount, phase.suicideSquadInterval));
        }

        // 5. 生成符文 (P2)
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

        // 阶段结束清理
        if (shieldShuffler != null) StopCoroutine(shieldShuffler);
        if (suicideSquadLoop != null) StopCoroutine(suicideSquadLoop);

        ClearRunes();
    }

    // --- 神风特攻队循环逻辑 ---
    private IEnumerator SuicideSquadLoop(int count, float interval)
    {
        // 首次立刻生成
        yield return StartCoroutine(SpawnSuicideSquad(count));

        while (true)
        {
            // 清理空对象
            activeSuicideMinions.RemoveAll(item => item == null);

            // 【防重叠】只要场上还有活着的，就一直等待
            if (activeSuicideMinions.Count > 0)
            {
                yield return null;
                continue;
            }

            // 全死光了，开始倒计时
            yield return new WaitForSeconds(interval);

            // 倒计时结束，生成下一波
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
            // --- 左边小怪 ---
            GameObject lMinion = Instantiate(suicideMinionPrefab, leftSpawn, Quaternion.identity);
            SuicideMinion lScript = lMinion.GetComponent<SuicideMinion>();

            if (lScript != null)
            {
                // 计算位置和延迟
                Vector3 targetPos = new Vector3(-(startX + i * spacing), targetY, 0f);
                float delay = i * 0.4f;
                lScript.Initialize(targetPos, delay);
            }
            activeSuicideMinions.Add(lMinion);

            // --- 右边小怪 ---
            GameObject rMinion = Instantiate(suicideMinionPrefab, rightSpawn, Quaternion.identity);
            SuicideMinion rScript = rMinion.GetComponent<SuicideMinion>();

            if (rScript != null)
            {
                // 计算位置和延迟
                Vector3 targetPos = new Vector3(startX + i * spacing, targetY, 0f);
                float delay = i * 0.4f;
                rScript.Initialize(targetPos, delay);
            }
            activeSuicideMinions.Add(rMinion);

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

    private void ClearRunes()
    {
        foreach (var rune in activeRunes)
        {
            if (rune != null) Destroy(rune);
        }
        activeRunes.Clear();
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