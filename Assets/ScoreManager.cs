using UnityEngine;
using UnityEngine.UI; // 使用旧版UI

public class ScoreManager : MonoBehaviour
{
    public Text scoreText; // 得分文本，Inspector中赋值
    public int score = 0; // 改为public，以便其他脚本访问

    void Start()
    {
        Debug.Log("ScoreManager initialized. ScoreText assigned: " + (scoreText != null));
        UpdateScoreText();
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score increased to: " + score + ", Updating ScoreText...");
        UpdateScoreText();
    }

    // 新增: Boss 专用加分方法 (50分)
    public void AddBossScore()
    {
        AddScore(50);
        Debug.Log("Boss defeated! +50 points");
    }

    // 新增: 小怪专用加分方法 (10分)
    public void AddEnemyScore()
    {
        AddScore(10);
        Debug.Log("Enemy defeated! +10 points");
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
            Debug.Log("ScoreText set to: " + scoreText.text + ", IsActive: " + scoreText.gameObject.activeInHierarchy);
        }
        else
        {
            Debug.LogError("ScoreText is not assigned in Inspector!");
        }
    }
}