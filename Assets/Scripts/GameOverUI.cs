using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;
    public Text winnerText;
    public Button restartButton;

    void Start()
    {
        panel.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);
    }

    public void Show(string winnerName)
    {
        winnerText.text = $"{winnerName} WINS!";
        panel.SetActive(true);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

