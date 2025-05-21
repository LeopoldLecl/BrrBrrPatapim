using UnityEngine;
using TMPro;

public class ScoreScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private TextMeshProUGUI scoreText;
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

    [Header("Bonus")]
    public bool isInSpace = false;

    private float bonusMultiplier = 1f;
    private float bonusDuration = 0f;
    private float starBonusMultiplier = 1f;

    private int displayedScore = 0;
    private int actualScore = 0;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private float soundTimer = 0f;
    private float previousPlayerY;

    void Start()
    {
        if (player == null || scoreText == null)
        {
            Debug.LogError("ScoreScript: player or scoreText not assigned.");
            enabled = false;
            return;
        }

        originalScale = scoreText.rectTransform.localScale;
        originalRotation = scoreText.rectTransform.localRotation;
        previousPlayerY = player.position.y;
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

        // Smooth score interpolation
        if (displayedScore < actualScore)
        {
            displayedScore = Mathf.CeilToInt(Mathf.Lerp(displayedScore, actualScore, Time.deltaTime * 10));
            scoreText.text = displayedScore.ToString();
        }

        // Lerp back to original scale and rotation
        scoreText.rectTransform.localScale = Vector3.Lerp(scoreText.rectTransform.localScale, originalScale, Time.deltaTime * scaleSpeed);
        scoreText.rectTransform.localRotation = Quaternion.Lerp(scoreText.rectTransform.localRotation, originalRotation, Time.deltaTime * rotationLerpSpeed);

        soundTimer -= Time.deltaTime;
    }

    void OnScoreUpdate()
    {
        // Scale effect
        scoreText.rectTransform.localScale = punchScale;

        // Random shake rotation on Z
        float randomAngle = Random.Range(-rotationShakeAngle, rotationShakeAngle);
        scoreText.rectTransform.localRotation = Quaternion.Euler(0f, 0f, randomAngle);

        // Play sound
        if (audioSource != null && scorePopSFX != null && soundTimer <= 0f)
        {
            audioSource.PlayOneShot(scorePopSFX);
            soundTimer = soundCooldown;
        }
    }

    private void UpdateBonusTimers()
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
        if (!isInSpace) return;

        bonusMultiplier = 2f;
        bonusDuration = 5f;
    }

    public void ActivateStarBoost()
    {
        starBonusMultiplier += 0.10f;
    }

    public int GetScore() => actualScore;
}
