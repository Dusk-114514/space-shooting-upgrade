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
    public float rotateSpeed = 30f;
    public float spawnDelay = 0.1f;
}

[System.Serializable]
public class BossPhase
{
    public string phaseName = "Spell Card";

    [Header("阶段设置")]
    [Tooltip("此阶段 Boss 是否无敌？")]
    public bool isInvulnerablePhase = true;

    [Header("弹幕配置")]
    [Tooltip("拖入此阶段 Boss 要发射的弹幕配置文件 (BulletObject)")]
    public BulletObject danmakuConfig; // 【新增】这里存放这一波的弹幕配置

    [Header("小怪配置")]
    public List<MinionGroupConfig> minionGroups;
}

// --- 控制器类 ---
[RequireComponent(typeof(EnemyHealth))]
public class BossController : MonoBehaviour
{
    [Header("战斗流程配置")]
    [SerializeField] private List<BossPhase> allPhases;

    [Header("状态监控")]
    [SerializeField] private int minionsAliveCount = 0;
    private bool waitingForMinionsClear = false;

    // 组件引用
    private EnemyHealth bossHealth;
    private BossMovement bossMovement;
    private Sender danmakuSender; // 【新增】发射器引用

    private void Awake()
    {
        bossHealth = GetComponent<EnemyHealth>();
        bossMovement = GetComponent<BossMovement>();

        // 【新增】自动查找 Boss 身上的 Sender 组件
        // 如果 Boss 身上没挂，尝试在子物体里找（以防你把发射点做成了子物体）
        danmakuSender = GetComponent<Sender>();
        if (danmakuSender == null)
        {
            danmakuSender = GetComponentInChildren<Sender>();
        }
    }

    private void Start()
    {
        // 游戏开始时先关闭发射器，防止乱射
        if (danmakuSender != null) danmakuSender.enabled = false;

        if (allPhases == null || allPhases.Count == 0)
        {
            Debug.LogWarning("Boss 没有配置任何阶段 (Phases)！");
            return;
        }
        StartCoroutine(CombatLoop());
    }

    private IEnumerator CombatLoop()
    {
        yield return new WaitForSeconds(1f);

        if (bossMovement != null)
        {
            Debug.Log("Boss 入场完毕，激活移动 AI！");
            bossMovement.enabled = true;
        }

        // 遍历所有阶段
        for (int i = 0; i < allPhases.Count; i++)
        {
            BossPhase currentPhase = allPhases[i];
            Debug.Log($"<color=cyan>BOSS 进入阶段 {i + 1}: {currentPhase.phaseName}</color>");

            // 1. 设置无敌
            if (currentPhase.isInvulnerablePhase)
            {
                bossHealth.SetInvulnerable(true);
            }

            // 2. 【核心集成】设置并启动弹幕
            if (danmakuSender != null)
            {
                // 将当前阶段的配置传给 Sender，Sender 会自动重置并开始工作
                danmakuSender.SetPattern(currentPhase.danmakuConfig);
            }

            // 3. 生成小怪
            minionsAliveCount = 0;
            foreach (var group in currentPhase.minionGroups)
            {
                yield return StartCoroutine(SpawnGroup(group));
            }

            // 4. 等待清空 (此时 Boss 一边放弹幕，一边无敌)
            if (minionsAliveCount > 0)
            {
                waitingForMinionsClear = true;
                yield return new WaitUntil(() => minionsAliveCount <= 0);
                waitingForMinionsClear = false;
            }

            // 5. 阶段结束：护盾破碎
            Debug.Log("阶段完成！Boss 护盾失效！停止射击并虚弱 5 秒...");
            bossHealth.SetInvulnerable(false);

            // 【核心集成】破防后，停止发射弹幕 (传入 null 即可关闭 Sender)
            if (danmakuSender != null)
            {
                danmakuSender.SetPattern(null);
            }

            yield return new WaitForSeconds(5f);
        }

        Debug.Log("所有阶段结束！");
        // 如果你想在所有阶段结束后 Boss 发狂射击，可以在这里加一行：
        // if (danmakuSender != null) danmakuSender.SetPattern(finalPattern);
    }

    private IEnumerator SpawnGroup(MinionGroupConfig group)
    {
        for (int i = 0; i < group.count; i++)
        {
            Vector3 spawnPos = GeometricMath.CalculatePosition(
                group.pattern,
                transform.position,
                i,
                group.count,
                group.radius
            );

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