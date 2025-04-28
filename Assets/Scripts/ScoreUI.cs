using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI scoreText;

    public void UpdateScoreText(int score)
    {
        scoreText.text = $"�X�R�A: {score}";
    }
}
