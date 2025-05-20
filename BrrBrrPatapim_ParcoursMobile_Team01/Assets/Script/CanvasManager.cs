using System;
using UnityEngine;
using UnityEngine.Events;

public class CanvasManager : MonoBehaviour
{
    public UnityEvent onStartGame;
    
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject postGameUI;
    

    public void StartGame()
    {
        menuUI.SetActive(false);
        gameUI.SetActive(true);
        
        onStartGame.Invoke();
        PortalsManager.Instance.StartGame();
    }
}
