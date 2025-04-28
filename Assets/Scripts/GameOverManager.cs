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
        Time.timeScale = 1f; // �O�̂��߁A�Q�[���X�^�[�g���ɒʏ푬�x�֖߂�
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // ���S�ɔ�\��
        }
        else
        {
            Debug.LogWarning("GameOverManager: gameOverPanel ���ݒ肳��Ă��܂���I");
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
            Debug.LogWarning("GameOverManager: ShowGameOver ���Ă΂�܂������Apanel �� null �ł�");
        }
    }

    public void ReloadScene()
    {
        Time.timeScale = 1f; // �O�̂��߁A�Q�[���X�^�[�g���ɒʏ푬�x�֖߂�
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
