using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using Script;

namespace Script
{
    public class CanvasManager : MonoBehaviour
    {
        public UnityEvent onStartGame;

        [Header("UI")]
        [SerializeField] private GameObject menuUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject postGameUI;
        [SerializeField] private GameObject bonusWheelUI;

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

            // Analytics: mark gameplay started
            PlayerAnalyticsTracker.SetPlaying(true);
            PlayerAnalyticsTracker.RecordEventSafe("ui_start_game");

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

            Debug.Log("Fin de jeu - Switch caméra");

            // Analytics: mark gameplay ended
            PlayerAnalyticsTracker.SetPlaying(false);
            PlayerAnalyticsTracker.RecordEventSafe("ui_end_game");

            // Activer la cam�ra de fin avec priorit� plus haute
            if (endGameCamera != null)
            {
                endGameCamera.Priority = 100;
            }
            else
            {
                Debug.LogWarning("EndGameCamera non assignée !");
            }
        }

        public void OpenWheel()
        {
            bonusWheelUI.SetActive(true);
        }
        
        public void RestartGame()
        {
            PlayerAnalyticsTracker.RecordEventSafe("ui_restart");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
