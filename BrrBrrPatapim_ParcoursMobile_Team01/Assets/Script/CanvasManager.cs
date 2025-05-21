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

    public void StartGame()
    {
        menuUI.SetActive(false);
        gameUI.SetActive(true);

        onStartGame.Invoke();
        PortalsManager.Instance.StartGame();

        // S'assurer que la caméra de jeu est active au démarrage
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

        // Activer la caméra de fin avec priorité plus haute
        if (endGameCamera != null)
        {
            endGameCamera.Priority = 100;
        }
        else
        {
            Debug.LogWarning("EndGameCamera non assignée !");
        }
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
