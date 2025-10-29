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