using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public UnityEvent onStartGame;

    [Header("UI")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject postGameUI;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera gameplayCamera;
    [SerializeField] private CinemachineCamera endGameCamera;
    
    [SerializeField] TextMeshProUGUI highscoreText;

    private void Start()
    {
        UpdateHighscoreText();
    }

    private void UpdateHighscoreText()
    {
        int highscore = PlayerPrefs.GetInt("highscore", 0);
        if (highscoreText != null)
        {
            highscoreText.text = $"HighScore : {highscore}";
        }
    }
    
    public void ResetPlayerStatsAndReboot()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void StartGame()
    {
        menuUI.SetActive(false);
        gameUI.SetActive(true);

        onStartGame.Invoke();
        PortalsManager.Instance.StartGame();

        // S'assurer que la cam�ra de jeu est active au d�marrage
        if (gameplayCamera != null && endGameCamera != null)
        {
            gameplayCamera.Priority = 12;
            endGameCamera.Priority = 0;
        }
    }

    public void EndGame()
    {
        menuUI.SetActive(false);
        gameUI.SetActive(false);
        postGameUI.SetActive(true);

        Debug.Log("Fin de jeu - Switch cam�ra");

        // Activer la cam�ra de fin avec priorit� plus haute
        if (endGameCamera != null)
        {
            endGameCamera.Priority = 100;
        }
        else
        {
            Debug.LogWarning("EndGameCamera non assign�e !");
        }
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
