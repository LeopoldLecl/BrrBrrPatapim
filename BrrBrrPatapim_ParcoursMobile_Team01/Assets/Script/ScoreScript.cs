using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class ScoreScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI starMultiplierText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scorePopSFX;

    [Header("Juice Settings")]
    [SerializeField] private float heightMultiplier = 10f;
    [SerializeField] private int scoreMultiplier = 1;
    [SerializeField] private Vector3 punchScale = new Vector3(1.3f, 1.3f, 1f);
    [SerializeField] private float scaleSpeed = 6f;
    [SerializeField] private float rotationShakeAngle = 20f;
    [SerializeField] private float rotationLerpSpeed = 6f;
    [SerializeField] private float soundCooldown = 0.2f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lightningColor = Color.red;
    [SerializeField] private float colorLerpSpeed = 6f;

    [Header("Star Juice")]
    [SerializeField] private Vector3 starPunchScale = new Vector3(1.5f, 1.5f, 1f);
    [SerializeField] private float starScaleSpeed = 8f;
    [SerializeField] private float starRotationShakeAngle = 15f;
    [SerializeField] private float starRotationLerpSpeed = 8f;
    [SerializeField] private GameObject starEffect;
    [SerializeField] private GameObject lightningEffect;

    [Header("Bonus")]
    public bool isInSpace = false;

    private float bonusMultiplier = 1f;
    private float bonusDuration = 0f;
    private float starBonusMultiplier = 1f;

    private int displayedScore = 0;
    private int actualScore = 0;
    private float soundTimer = 0f;
    private float previousPlayerY;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Vector3 starOriginalScale;
    private Quaternion starOriginalRotation;
    
    public int DisplayedScore => displayedScore;

    void Start()
    {
        if (player == null || scoreText == null || starMultiplierText == null)
        {
            Debug.LogError("ScoreScript: References not assigned.");
            enabled = false;
            return;
        }

        originalScale = scoreText.rectTransform.localScale;
        originalRotation = scoreText.rectTransform.localRotation;

        starOriginalScale = starMultiplierText.rectTransform.localScale;
        starOriginalRotation = starMultiplierText.rectTransform.localRotation;

        previousPlayerY = player.position.y;
        UpdateStarMultiplierDisplay(); // Init
    }

    void Update()
    {
        UpdateBonusTimers();

        float currentY = player.position.y;
        float deltaY = currentY - previousPlayerY;

        if (deltaY > 0f)
        {
            int gained = Mathf.FloorToInt(deltaY * heightMultiplier) * scoreMultiplier;
            if (gained > 0)
            {
                float finalMultiplier = bonusMultiplier * starBonusMultiplier;
                actualScore += Mathf.FloorToInt(gained * finalMultiplier);
                OnScoreUpdate();
            }
        }

        previousPlayerY = currentY;

        if (displayedScore < actualScore)
        {
            displayedScore = Mathf.CeilToInt(Mathf.Lerp(displayedScore, actualScore, Time.deltaTime * 10));
            scoreText.text = displayedScore.ToString();
        }

        // Lerp color
        scoreText.color = Color.Lerp(scoreText.color,
                                     bonusMultiplier > 1f ? lightningColor : normalColor,
                                     Time.deltaTime * colorLerpSpeed);

        // Lerp scale/rotation back
        scoreText.rectTransform.localScale = Vector3.Lerp(scoreText.rectTransform.localScale, originalScale, Time.deltaTime * scaleSpeed);
        scoreText.rectTransform.localRotation = Quaternion.Lerp(scoreText.rectTransform.localRotation, originalRotation, Time.deltaTime * rotationLerpSpeed);

        starMultiplierText.rectTransform.localScale = Vector3.Lerp(starMultiplierText.rectTransform.localScale, starOriginalScale, Time.deltaTime * starScaleSpeed);
        starMultiplierText.rectTransform.localRotation = Quaternion.Lerp(starMultiplierText.rectTransform.localRotation, starOriginalRotation, Time.deltaTime * starRotationLerpSpeed);

        soundTimer -= Time.deltaTime;
    }

    void OnScoreUpdate()
    {
        // Scale + shake
        scoreText.rectTransform.localScale = punchScale;

        float randomAngle = Random.Range(-rotationShakeAngle, rotationShakeAngle);
        scoreText.rectTransform.localRotation = Quaternion.Euler(0f, 0f, randomAngle);

        if (audioSource != null && scorePopSFX != null && soundTimer <= 0f)
        {
            audioSource.PlayOneShot(scorePopSFX);
            soundTimer = soundCooldown;
        }
    }

    void UpdateBonusTimers()
    {
        if (bonusDuration > 0f)
        {
            bonusDuration -= Time.deltaTime;
            if (bonusDuration <= 0f)
            {
                bonusMultiplier = 1f;
            }
        }
    }

    public void ActivateLightningBoost()
    {
        if (!isInSpace)
        {
            Debug.Log("Boost ignor� : pas en espace.");
            return;
        }

        bonusMultiplier = 2f;
        bonusDuration = 5f;
        Debug.Log("Boost �clair activ� !");
        
        GameObject thisEffect = Instantiate(this.lightningEffect);
        thisEffect.transform.position = player.position;
        thisEffect.GetComponent<ParticleSystem>().Play();
    }

    public void ActivateStarBoost()
    {
        starBonusMultiplier += 0.10f;
        Debug.Log($"Boost �toile  x{starBonusMultiplier:F2}");

        UpdateStarMultiplierDisplay();

        // Juicy feedback
        starMultiplierText.rectTransform.localScale = starPunchScale;
        float randomAngle = Random.Range(-starRotationShakeAngle, starRotationShakeAngle);
        starMultiplierText.rectTransform.localRotation = Quaternion.Euler(0f, 0f, randomAngle);
        
        GameObject thisEffect = Instantiate(this.starEffect);
        thisEffect.transform.position = player.position;
        thisEffect.GetComponent<ParticleSystem>().Play();
    }

    void UpdateStarMultiplierDisplay()
    {
        starMultiplierText.text = $"x {starBonusMultiplier:F2}";
    }

    public int GetScore() => actualScore;
}
