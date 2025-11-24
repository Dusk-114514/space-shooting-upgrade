using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameTimer : MonoBehaviour
{
    [Header("基础计时器 UI")]
    public Text timerText;
    public float totalTime = 240f;
    public float alertTime = 30f;
    public Color normalColor = Color.white;
    public Color alertColor = Color.red;
    public float timerFlashInterval = 0.2f;

    [Header("终末宣告 UI设置")]
    [Tooltip("拖入专门用于显示终末宣告的 Text 组件")]
    public Text endgameMessageText;
    [TextArea(2, 3)]
    [Tooltip("终末宣告的内容")]
    public string endgameContent = "Your life flickereth like a dying flame.\nEmbrace thine end.";
    [Tooltip("打字机速度（每个字母间隔秒数）")]
    public float typingSpeed = 0.08f;
    [Tooltip("宣告出现后的闪烁间隔")]
    public float messageFlashInterval = 0.5f;

    private float currentTime;
    private bool isEnded = false;
    private bool isAlertState = false;

    void Start()
    {
        currentTime = totalTime;

        // 初始化计时器文本
        if (timerText != null)
        {
            timerText.color = normalColor;
        }

        // 初始化终末宣告文本（一开始隐藏，且内容为空）
        if (endgameMessageText != null)
        {
            endgameMessageText.text = "";
            endgameMessageText.gameObject.SetActive(false);
            // 确保颜色是红色，增加压迫感
            endgameMessageText.color = alertColor;
        }
    }

    void Update()
    {
        if (isEnded) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            EndTimer();
        }

        UpdateTimerUI();

        // 进入最后30秒警戒状态
        if (!isAlertState && currentTime <= alertTime && currentTime > 0)
        {
            isAlertState = true;
            if (timerText != null)
            {
                timerText.color = alertColor;
                timerText.fontStyle = FontStyle.Bold;
            }
        }

        // 警戒状态下的计时器简单缩放呼吸效果
        if (isAlertState && timerText != null)
        {
            float scale = 1f + Mathf.PingPong(Time.time * 2f, 0.1f);
            timerText.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void EndTimer()
    {
        isEnded = true;
        UpdateTimerUI();

        // 1. 计时器归零后，自身开始快速闪烁
        if (timerText != null)
        {
            timerText.transform.localScale = Vector3.one; // 重置缩放
            StartCoroutine(FlashTextRoutine(timerText, timerFlashInterval));
        }

        // 2. 启动终末宣告流程
        if (endgameMessageText != null)
        {
            StartCoroutine(TypewriterAndFlashRoutine());
        }

        Debug.LogWarning("时间到！迎接终末！");
        // 这里可以通知 BossController 强制狂暴
    }

    // 通用闪烁协程
    IEnumerator FlashTextRoutine(Text targetText, float interval)
    {
        if (targetText == null) yield break;
        while (true)
        {
            targetText.enabled = !targetText.enabled;
            yield return new WaitForSeconds(interval);
        }
    }

    // 打字机 + 后续闪烁流程协程
    IEnumerator TypewriterAndFlashRoutine()
    {
        endgameMessageText.text = "";
        endgameMessageText.gameObject.SetActive(true); // 激活物体

        // --- 打字机效果 ---
        foreach (char letter in endgameContent.ToCharArray())
        {
            endgameMessageText.text += letter;
            // 简单的音效触发点：可以在这里播放打字机音效
            yield return new WaitForSeconds(typingSpeed);
        }

        // 等待一小会儿，增强仪式感
        yield return new WaitForSeconds(0.5f);

        // --- 开始持续闪烁 ---
        // 复用上面的通用闪烁协程，但使用不同的间隔
        StartCoroutine(FlashTextRoutine(endgameMessageText, messageFlashInterval));
    }
}