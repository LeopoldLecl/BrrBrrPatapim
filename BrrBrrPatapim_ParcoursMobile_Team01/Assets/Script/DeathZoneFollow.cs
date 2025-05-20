using UnityEngine;

public class DeathZoneFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Offset & Vitesse")]
    [SerializeField] private float verticalOffset = -20f;
    [SerializeField] private float catchUpSpeed = 3f;
    [SerializeField] private float overshootSpeed = 5.25f;

    [Header("D�passement")]
    [SerializeField] private float maxOvershoot = 100f;

    private float currentTargetY;
    private float maxReachedPlayerY;

    private bool isShifting = false;
    private float shiftTargetY;
    [SerializeField] private float shiftSpeed = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip deathZoneLoopClip;
    [SerializeField] private AudioSource audioSource;
    
    private bool hasGameStarted = false;
    
    public bool HasGameStarted
    {
        get => hasGameStarted;
        set => hasGameStarted = value;
    }


    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player reference not assigned.");
            enabled = false;
            return;
        }

        maxReachedPlayerY = player.position.y;
        currentTargetY = player.position.y + verticalOffset;
        transform.position = new Vector3(transform.position.x, currentTargetY, transform.position.z);
    }

    void Update()
    {
        if (!hasGameStarted) return;
        
        float playerY = player.position.y;

        if (playerY > maxReachedPlayerY)
        {
            maxReachedPlayerY = playerY;
        }

        float overshootTargetY = maxReachedPlayerY + verticalOffset + maxOvershoot;

        currentTargetY = Mathf.MoveTowards(transform.position.y, overshootTargetY, overshootSpeed * Time.deltaTime);

        if (transform.position.y < playerY + verticalOffset)
        {
            currentTargetY = Mathf.MoveTowards(transform.position.y, playerY + verticalOffset, catchUpSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(player.transform.position.x, currentTargetY, transform.position.z);

        if (isShifting)
        {
            float newY = Mathf.Lerp(transform.position.y, shiftTargetY, Time.deltaTime * shiftSpeed);
            transform.position = new Vector3(player.transform.position.x, newY, transform.position.z);
            currentTargetY = transform.position.y;

            if (Mathf.Abs(transform.position.y - shiftTargetY) < 0.01f)
            {
                transform.position = new Vector3(player.transform.position.x, shiftTargetY, transform.position.z);
                currentTargetY = shiftTargetY;
                isShifting = false;
            }
        }


    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null && deathZoneLoopClip != null)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = deathZoneLoopClip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }



    public void ShiftZone(float delta)
    {
        shiftTargetY = transform.position.y + delta;
        isShifting = true;
        maxReachedPlayerY = Mathf.Max(maxReachedPlayerY, shiftTargetY - verticalOffset - maxOvershoot);
    }
}
