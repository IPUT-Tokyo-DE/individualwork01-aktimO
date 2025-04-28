using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Time.timeScale = 1f; // 念のため、ゲームスタート時に通常速度へ戻す
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // 安全に非表示
        }
        else
        {
            Debug.LogWarning("GameOverManager: gameOverPanel が設定されていません！");
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("GameOverManager: ShowGameOver が呼ばれましたが、panel が null です");
        }
    }

    public void ReloadScene()
    {
        Time.timeScale = 1f; // 念のため、ゲームスタート時に通常速度へ戻す
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
