using UnityEngine;

public class Rune : MonoBehaviour
{
    [Header("设置")]
    public float requiredTime = 5f; // 需要停留的时间
    public float fireRateBonus = 0.1f; // 减少多少射击间隔
    public Color normalColor = new Color(1f, 1f, 1f, 0.3f); // 未激活颜色 (半透明白)
    public Color activeColor = new Color(0f, 1f, 0f, 0.6f); // 激活中颜色 (绿色)
    public Color completeColor = Color.cyan; // 完成颜色

    private float timer = 0f;
    private bool isPlayerInside = false;
    private SpriteRenderer sr;
    private bool isCompleted = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = normalColor;
    }

    void Update()
    {
        if (isCompleted) return;

        if (isPlayerInside)
        {
            // 累加时间
            timer += Time.deltaTime;

            // 视觉反馈：根据进度插值颜色
            float progress = timer / requiredTime;
            if (sr != null)
            {
                sr.color = Color.Lerp(normalColor, activeColor, progress);
            }

            // 检查是否完成
            if (timer >= requiredTime)
            {
                CompleteRune();
            }
        }
        else
        {
            // 如果玩家离开，进度缓慢衰退，或者直接重置？这里设为重置，增加难度
            if (timer > 0)
            {
                timer -= Time.deltaTime * 2f; // 离开时进度快速回退
                if (timer < 0) timer = 0;

                // 更新回退的颜色
                float progress = timer / requiredTime;
                if (sr != null) sr.color = Color.Lerp(normalColor, activeColor, progress);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    void CompleteRune()
    {
        isCompleted = true;
        if (sr != null) sr.color = completeColor;

        // 获取玩家并升级
        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            // 调用升级，参数 true 表示【突破上限】
            playerShooting.UpgradeFireRate(fireRateBonus, true);
        }

        Debug.Log("符文激活！射速提升！");

        // 播放个音效或者特效...

        // 销毁符文
        Destroy(gameObject, 0.5f);
    }
}