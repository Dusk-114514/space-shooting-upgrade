using UnityEngine;
using UnityEngine.UI; // 必须引用 UI

public class BossHealthBar : MonoBehaviour
{
    [Header("UI 组件")]
    public Slider hpSlider; // 拖入你的 Slider
    public GameObject healthBarContainer; // 整个血条物体（用于隐藏/显示）

    [Header("目标")]
    public BossController bossController; // 场景里的 Boss

    private void Start()
    {
        // 1. 如果没有手动拖拽 Boss，自动去场景里找
        if (bossController == null)
        {
            bossController = FindObjectOfType<BossController>();
        }

        // 2. 初始化检查
        if (bossController != null)
        {
            // 获取 Boss 身上的血量组件
            EnemyHealth bossHealth = bossController.GetComponent<EnemyHealth>();
            if (bossHealth != null)
            {
                // 【订阅事件】当 Boss 血量变化时，执行 UpdateUI 方法
                bossHealth.OnHealthChanged += UpdateUI;

                // 确保一开始是显示的
                if (healthBarContainer != null) healthBarContainer.SetActive(true);
            }
        }
        else
        {
            // 如果场景里没有 Boss，隐藏血条
            if (healthBarContainer != null) healthBarContainer.SetActive(false);
        }
    }

    // 当 Boss 掉血时会自动调用这个方法
    private void UpdateUI(int current, int max)
    {
        if (hpSlider != null)
        {
            // 计算百分比 (0.0 ~ 1.0)
            float percent = (float)current / max;
            hpSlider.value = percent;
        }

        // 如果 Boss 死了 (血量<=0)，隐藏血条
        if (current <= 0 && healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }
    }

    // 脚本销毁时记得取消订阅，防止报错
    private void OnDestroy()
    {
        if (bossController != null)
        {
            EnemyHealth bossHealth = bossController.GetComponent<EnemyHealth>();
            if (bossHealth != null)
            {
                bossHealth.OnHealthChanged -= UpdateUI;
            }
        }
    }
}