using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private List<AudioSource> audioSources = new List<AudioSource>();
    private Image soundButtonImage;
    private bool isOn = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (!PlayerPrefs.HasKey("Sound"))
                PlayerPrefs.SetInt("Sound", 1);

            isOn = PlayerPrefs.GetInt("Sound") == 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RefreshAudioAndButton();
        ApplySoundState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RefreshAfterSceneLoad());
    }

    private IEnumerator RefreshAfterSceneLoad()
    {
        yield return null; // Wait one frame for objects to initialize
        RefreshAudioAndButton();
        ApplySoundState();
    }

    private void RefreshAudioAndButton()
    {
        audioSources.Clear();
#if UNITY_2023_1_OR_NEWER
        audioSources.AddRange(Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        var toggleButton = Object.FindFirstObjectByType<SoundToggleButton>(FindObjectsInactive.Include);
#else
        // Use Resources to avoid deprecated FindObject(s)OfType in older Unity versions
        audioSources.AddRange(Resources.FindObjectsOfTypeAll<AudioSource>()
            .Where(a => a != null && a.gameObject.scene.IsValid()));
        var toggleButton = Resources.FindObjectsOfTypeAll<SoundToggleButton>()
            .FirstOrDefault(b => b != null && b.gameObject.scene.IsValid());
#endif
        soundButtonImage = toggleButton ? toggleButton.GetComponent<Image>() : null;
    }

    private void ApplySoundState()
    {
        foreach (var audioSource in audioSources)
            audioSource.volume = isOn ? 0.5f: 0.0f;

        if (soundButtonImage)
            soundButtonImage.sprite = isOn ? soundOnSprite : soundOffSprite;
    }

    public void ToggleSound()
    {
        isOn = !isOn;
        PlayerPrefs.SetInt("Sound", isOn ? 1 : 0);
        ApplySoundState();
    }
}