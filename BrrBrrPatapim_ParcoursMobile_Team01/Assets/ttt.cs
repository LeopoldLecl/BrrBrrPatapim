using Unity.Services.RemoteConfig;
using UnityEngine;

using UnityEngine;
using UnityEngine.Advertisements;
 
public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string _androidGameId = "5978493";
    [SerializeField] bool _testMode = true;
    private string _gameId;
 
    void Awake()
    {
        InitializeAds();
    }
 
    public void InitializeAds()
    {

        _gameId = _androidGameId;


        var b = Utilities.CheckForInternetConnection();
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);  
        }
    } 
 
 
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }
 
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}
