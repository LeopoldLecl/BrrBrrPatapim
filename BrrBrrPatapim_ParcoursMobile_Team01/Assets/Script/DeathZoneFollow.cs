using UnityEngine;

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

    private float currentTargetY;
    private float maxReachedPlayerY;

    private bool isShifting = false;
    private float shiftTargetY;
    [SerializeField] private float shiftSpeed = 3f; 


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
        float playerY = player.position.y;

        if (playerY > maxReachedPlayerY)
        {
            maxReachedPlayerY = playerY;
        }

        // Calcule la cible à dépasser (ex : si offset = -5, overshoot = 3, alors on vise Y = playerY - 2)
        float overshootTargetY = maxReachedPlayerY + verticalOffset + maxOvershoot;

        // Move vers cette cible (on dépasse le joueur à terme)
        currentTargetY = Mathf.MoveTowards(transform.position.y, overshootTargetY, overshootSpeed * Time.deltaTime);

        // Si on est en dessous du joueur (et donc à la traîne), on accélère pour rattraper
        if (transform.position.y < playerY + verticalOffset)
        {
            currentTargetY = Mathf.MoveTowards(transform.position.y, playerY + verticalOffset, catchUpSpeed * Time.deltaTime);
        }

        // Applique la position
        transform.position = new Vector3(transform.position.x, currentTargetY, transform.position.z);

        // Smooth shift vers une nouvelle hauteur définie par ShiftZone()
        if (isShifting)
        {
            float newY = Mathf.Lerp(transform.position.y, shiftTargetY, Time.deltaTime * shiftSpeed);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            currentTargetY = transform.position.y;

            // Arrêt du lerp quand suffisamment proche
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
            Debug.Log("Player entered the death zone.");
        }
    }

    public void ShiftZone(float delta)
    {
        shiftTargetY = transform.position.y + delta;
        isShifting = true;
        maxReachedPlayerY = Mathf.Max(maxReachedPlayerY, shiftTargetY - verticalOffset - maxOvershoot);
    }
}
