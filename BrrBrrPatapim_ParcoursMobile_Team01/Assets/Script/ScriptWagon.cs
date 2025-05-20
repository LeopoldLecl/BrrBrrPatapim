using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ScriptWagon : MonoBehaviour
{
    [FormerlySerializedAs("_prefab")] [SerializeField] private GameObject prefab;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 3f;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private int wagonsCount = 10; // Speed of rotation
    [SerializeField] private KeyCode rotateButton = KeyCode.Space; // Button to hold for rotation
    [SerializeField] private float firstWagonSpacing = 3f;
    
    [Header("Events")]
    public UnityEvent onScreenTouched;
    public UnityEvent onScreenReleased;

    private List<Transform> _wagonsList = new List<Transform>();
    private List<Vector3> _positionHistory = new List<Vector3>();
    private Tween _currentRotationTween;
    private float _currentTargetRotationX = 0f; // Track the current target rotation

    private void Start()
    {
        SpawnWagons(wagonsCount);
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        
        // Handle rotation based on touch input
        float targetRotationX = Input.touchCount > 0 ? 45f : -45f;

        // Only create new tween when target rotation changes
        if (targetRotationX != _currentTargetRotationX)
        {
            RotateToX(targetRotationX);
            _currentTargetRotationX = targetRotationX;
        }

        // Move locomotive forward
        transform.Translate((Vector3.forward * -1) * speed * Time.deltaTime);

        // Record position for wagons to follow
        _positionHistory.Insert(0, transform.position);

        // Limit history size
        int requiredHistory = Mathf.CeilToInt(_wagonsList.Count * spacing * 10);
        if (_positionHistory.Count > requiredHistory)
        {
            _positionHistory.RemoveRange(requiredHistory, _positionHistory.Count - requiredHistory);
        }

        UpdateWagonsPositions();
    }

    private void RotateToX(float targetX)
    {
        // Kill previous rotation tween if active
        if (_currentRotationTween != null && _currentRotationTween.IsActive())
        {
            _currentRotationTween.Kill();
        }
        
        // Create new rotation tween
        Vector3 targetRotation = new Vector3(targetX, transform.localEulerAngles.y, transform.localEulerAngles.z);
        _currentRotationTween = transform.DOLocalRotate(targetRotation, turnSpeed).SetEase(Ease.OutQuad);
    }

    private void SpawnWagons(int count)
    {
        // Clear existing wagons
        foreach (Transform wagon in _wagonsList)
        {
            if (wagon != null)
            {
                Destroy(wagon.gameObject);
            }
        }

        _wagonsList.Clear();

        // Spawn first wagon with custom spacing
        Vector3 spawnPos = transform.position - transform.forward * firstWagonSpacing;

        for (int i = 0; i < count; i++)
        {
            GameObject wagonObj = Instantiate(prefab, spawnPos, transform.rotation);
            _wagonsList.Add(wagonObj.transform);

            // Use regular spacing for subsequent wagons
            spawnPos -= transform.forward * spacing;
        }
    }

    private void UpdateWagonsPositions()
    {
        for (int i = 0; i < _wagonsList.Count; i++)
        {
            if (_wagonsList[i] == null) continue;

            // Calculate history index based on wagon position
            int posIndex = Mathf.Min(Mathf.RoundToInt((i + 1) * spacing * 10), _positionHistory.Count - 1);

            if (posIndex < 0 || posIndex >= _positionHistory.Count) continue;

            // Move wagon toward target position
            _wagonsList[i].position = Vector3.Lerp(_wagonsList[i].position,
                                                 _positionHistory[posIndex],
                                                 followSpeed * Time.deltaTime);

            // Rotate wagon to face movement direction
            if (posIndex > 0)
            {
                Vector3 direction = _positionHistory[posIndex - 1] - _positionHistory[posIndex];
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    _wagonsList[i].rotation = Quaternion.Slerp(_wagonsList[i].rotation,
                                                             targetRotation,
                                                             followSpeed * Time.deltaTime);
                }
            }
        }
    }
    
    public void zbeub()
    {
        Debug.Log("zbeub");
    }
}