using UnityEngine;
using UnityEngine.Events;

public class DeathZoneFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Offset & Vitesse")]
    [SerializeField] private float verticalOffset = -20f;
    [SerializeField] private float catchUpSpeed = 3f;
    [SerializeField] private float overshootSpeed = 5.25f;

    [Header("Dépassement")]
    [SerializeField] private float maxOvershoot = 100f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private UnityEvent OnGameOver;


    private bool isAllowedToRise = false;

    private float currentTargetY;
    private float maxReachedPlayerY;

    private bool isShifting = false;
    private float shiftTargetY;
    [SerializeField] private float shiftSpeed = 3f;

    public void AlloweToRise()
    {
        isAllowedToRise = true;
    }

    public void DisallowToRise()
    {
        isAllowedToRise = false;
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
        if (isAllowedToRise)
        {
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

            transform.position = new Vector3(transform.position.x, currentTargetY, transform.position.z);
        }

        if (isShifting)
        {
            float newY = Mathf.Lerp(transform.position.y, shiftTargetY, Time.deltaTime * shiftSpeed);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            currentTargetY = transform.position.y;

            if (Mathf.Abs(transform.position.y - shiftTargetY) < 0.01f)
            {
                transform.position = new Vector3(transform.position.x, shiftTargetY, transform.position.z);
                currentTargetY = shiftTargetY;
                isShifting = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioSource.Play();
            OnGameOver.Invoke();
        }
    }

    public void ShiftZone(float delta)
    {
        shiftTargetY = transform.position.y + delta;
        isShifting = true;
        maxReachedPlayerY = Mathf.Max(maxReachedPlayerY, shiftTargetY - verticalOffset - maxOvershoot);
    }
}
