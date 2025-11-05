using System;
using Script;
using UnityEngine;
using UnityEngine.Advertisements;

// A persistent, call-from-anywhere Ads service for Unity Ads (Advertisement Legacy SDK)
// Drop this script anywhere under Assets (recommended: Assets/Script/AdService.cs).
// Static API:
//   AdService.InitializeAds();
//   AdService.LoadRewarded();
//   AdService.ShowRewarded();
//   AdService.LoadInterstitial();
//   AdService.ShowInterstitial();
// Events:
//   AdService.OnRewardedCompleted, AdService.OnRewardedSkipped, AdService.OnInterstitialClosed, AdService.OnDebugLog
[DefaultExecutionOrder(-1000)]
public class AdService : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    // Placements (match your Unity Dashboard)
    [Header("Placements")]
    [SerializeField] private string interstitialPlacementId = "Interstitial_Android";
    [SerializeField] private string rewardedPlacementId = "Rewarded_Android";

    // Game IDs per platform (set from the Unity Dashboard)
    [Header("Game IDs")]
    [SerializeField] private string androidGameId = "3003911"; // Replace with your real Android Game ID
    [SerializeField] private string iOSGameId = "0000000";     // Replace with your real iOS Game ID

    [Header("Options")]
    [SerializeField] private bool testMode = true;

    [Header("Logging")]
    [SerializeField] private LogVerbosity logVerbosity = LogVerbosity.Minimal;
    private enum LogVerbosity { Silent = 0, ErrorsOnly = 1, Minimal = 2, Verbose = 3 }

    // Singleton
    private static AdService _instance;
    public static AdService Instance
    {
        get
        {
            if (_instance == null) EnsureInstance();
            return _instance;
        }
    }

    // Public events
    public static event Action<string> OnDebugLog;
    public static event Action OnRewardedCompleted;
    public static event Action OnRewardedSkipped;
    public static event Action OnInterstitialClosed;

    // State
    private string _gameId;
    private bool _rewardedLoaded;
    private bool _interstitialLoaded;
    private bool _pendingShowRewarded;
    private bool _pendingShowInterstitial;

    // Reset static singleton when Domain Reload is disabled (Editor Enter Play Mode Options)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _instance = null;

    // Persist across scenes
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private static void EnsureInstance()
    {
#if UNITY_2023_1_OR_NEWER
        var existing = FindFirstObjectByType<AdService>();
#else
        var existing = FindObjectOfType<AdService>();
#endif
        if (existing != null)
        {
            _instance = existing;
            return;
        }
        var go = new GameObject("AdService");
        _instance = go.AddComponent<AdService>();
        DontDestroyOnLoad(go);
    }

    // ---------- Static API ----------
    public static void InitializeAds() => Instance.Initialize();
    public static void LoadRewarded() => Instance.LoadRewardedAd();
    public static void ShowRewarded() => Instance.ShowRewardedAd();
    public static void LoadInterstitial() => Instance.LoadInterstitialAd();
    public static void ShowInterstitial() => Instance.ShowInterstitialAd();
    public static void SetTestMode(bool enabled) { Instance.testMode = enabled; }

    // ---------- Instance implementation ----------
    public void Initialize()
    {
#if UNITY_ANDROID
        _gameId = androidGameId;
#elif UNITY_IOS
        _gameId = iOSGameId;
#else
        _gameId = androidGameId; // Fallback for Editor/other
#endif
        if (!Advertisement.isSupported)
        {
            LogError("Ads not supported");
            return;
        }
        if (Advertisement.isInitialized)
        {
            // too chatty for minimal; skip
            return;
        }
        DebugLog($"Init Ads (gameId={_gameId}, test={testMode})");
        Advertisement.Initialize(_gameId, testMode, this);
    }
 
    public void LoadRewardedAd()
    {
        if (!Advertisement.isInitialized)
        {
            DebugLog("Init on LoadRewarded");
            Initialize();
        }
        DebugLog($"Load rwd: {rewardedPlacementId}");
        Advertisement.Load(rewardedPlacementId, this);
    }

    public void ShowRewardedAd()
    {
        if (_rewardedLoaded)
        {
            DebugLog($"Show rwd next: {rewardedPlacementId}");
            _ = StartCoroutine(ShowAdNextFrame(rewardedPlacementId));
            return;
        }
        DebugLog("Rwd not loaded; will auto-show");
        _pendingShowRewarded = true;
        LoadRewardedAd();
    }

    public void LoadInterstitialAd()
    {
        if (!Advertisement.isInitialized)
        {
            DebugLog("Init on LoadInter");
            Initialize();
        }
        DebugLog($"Load inter: {interstitialPlacementId}");
        Advertisement.Load(interstitialPlacementId, this);
    }

    public void ShowInterstitialAd()
    {
        if (_interstitialLoaded)
        {
            DebugLog($"Show inter next: {interstitialPlacementId}");
            _ = StartCoroutine(ShowAdNextFrame(interstitialPlacementId));
            return;
        }
        DebugLog("Inter not loaded; will auto-show");
        _pendingShowInterstitial = true;
        LoadInterstitialAd();
    }

    // ---------- IUnityAdsInitializationListener ----------
    public void OnInitializationComplete()
    {
        // Keep quiet unless verbose
        DebugLog("Init complete");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        LogError($"InitFail: {error} {message}");
    }

    // ---------- IUnityAdsLoadListener ----------
    public void OnUnityAdsAdLoaded(string placementId)
    {
        DebugLog($"Loaded: {placementId}");
        PlayerAnalyticsTracker.RecordEventSafe($"ad_loaded_{placementId}");
        if (placementId == rewardedPlacementId)
        {
            _rewardedLoaded = true;
            if (_pendingShowRewarded)
            {
                _pendingShowRewarded = false;
                ShowRewardedAd();
            }
        }
        else if (placementId == interstitialPlacementId)
        {
            _interstitialLoaded = true;
            if (_pendingShowInterstitial)
            {
                _pendingShowInterstitial = false;
                ShowInterstitialAd();
            }
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        LogError($"LoadFail: {placementId} {error} {message}");
        PlayerAnalyticsTracker.RecordEventSafe($"ad_load_fail_{placementId}");
        if (placementId == rewardedPlacementId) _rewardedLoaded = false;
        if (placementId == interstitialPlacementId) _interstitialLoaded = false;
    }

    // ---------- IUnityAdsShowListener ----------
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        LogError($"ShowFail: {placementId} {error} {message}");
        PlayerAnalyticsTracker.RecordEventSafe($"ad_show_fail_{placementId}");
        if (placementId == rewardedPlacementId) _rewardedLoaded = false;
        if (placementId == interstitialPlacementId) _interstitialLoaded = false;
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        DebugLog($"Show start: {placementId}");
        PlayerAnalyticsTracker.RecordEventSafe($"ad_show_start_{placementId}");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        DebugLog($"Click: {placementId}");
        PlayerAnalyticsTracker.RecordEventSafe($"ad_click_{placementId}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == rewardedPlacementId)
        {
            _rewardedLoaded = false;
            LogEvent($"Rewarded: {showCompletionState}");
            PlayerAnalyticsTracker.RecordEventSafe($"ad_rewarded_{showCompletionState}");
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
                OnRewardedCompleted?.Invoke();
            else if (showCompletionState == UnityAdsShowCompletionState.SKIPPED)
                OnRewardedSkipped?.Invoke();
        }
        else if (placementId == interstitialPlacementId)
        {
            _interstitialLoaded = false;
            LogEvent($"Inter: {showCompletionState}");
            PlayerAnalyticsTracker.RecordEventSafe($"ad_interstitial_{showCompletionState}");
            OnInterstitialClosed?.Invoke();
        }
    }

    // Defer actual ad show to the next frame so any UI OnClick teardown doesn't destroy the placeholder's canvas
    private System.Collections.IEnumerator ShowAdNextFrame(string placementId)
    {
        yield return new WaitForEndOfFrame();
        try
        {
            Advertisement.Show(placementId, this);
        }
        catch (Exception ex)
        {
            LogError($"ShowEx: {ex.Message}");
        }
    }

    // Auto-initialize on play (safe: no-op if already initialized)
    private void Start()
    {
        Initialize();
    }

    // ---------- Logging helpers ----------
    private void DebugLog(string msg)
    {
        // Info-level; only emits in Verbose mode
        if (logVerbosity == LogVerbosity.Verbose) Emit(msg);
    }

    private void LogEvent(string msg)
    {
        // Events in Minimal and above
        if (logVerbosity >= LogVerbosity.Minimal) Emit(msg);
    }

    private void LogError(string msg)
    {
        // Errors in ErrorsOnly and above
        if (logVerbosity >= LogVerbosity.ErrorsOnly) Emit(msg);
    }

    private void Emit(string msg)
    {
        OnDebugLog?.Invoke(msg);
        Debug.Log("[Ads] " + msg);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}