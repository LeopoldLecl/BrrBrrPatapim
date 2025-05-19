using UnityEngine;

public class RollerCoasterScript : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] float touchForce = 1f;
    [SerializeField] float rotationSpeed = 5f;

    Rigidbody rb;
    float previousY;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        previousY = transform.position.y;
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x + speed * 0.01f, transform.position.y, transform.position.z);

        if (Input.touchCount > 0)
        {
            ElevateWagon();
        }

        UpdateRotation();
    }

    void UpdateRotation()
    {
        float currentY = transform.position.y;
        float deltaY = currentY - previousY;

        float targetZ = 0f;
        if (deltaY > 0.001f)
        {
            targetZ = 60;
        }
        else if (deltaY < -0.001f)
        {
            targetZ = -60f;
        }

        Quaternion targetRotation = Quaternion.Euler(targetZ, -90f,0f );
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        previousY = currentY;
    }

    public void ElevateWagon()
    {
        rb.AddForce(Vector3.up * 0.1f * touchForce, ForceMode.Impulse);
        Debug.Log("Elevating wagon");
    }
}
