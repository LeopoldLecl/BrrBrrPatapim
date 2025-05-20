using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static GameplayEnums;

public class ScriptWagon : MonoBehaviour
{
    public static ScriptWagon Instance;
    
    [FormerlySerializedAs("_prefab")] [SerializeField] private GameObject prefab;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float boostSpeed = 35f; // Adjust as needed
    [SerializeField] private float turnSpeed = 3f;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private int wagonsCount = 10; // Speed of rotation
    [SerializeField] private KeyCode rotateButton = KeyCode.Space; // Button to hold for rotation
    [SerializeField] private float firstWagonSpacing = 3f;

    private List<Transform> _wagonsList = new List<Transform>();
    private List<Vector3> _positionHistory = new List<Vector3>();
    private Tween _currentRotationTween;
    private float _currentTargetRotationX = 0f; // Track the current target rotation
    
    private ParticleSystem _boostParticleSystem;
    
    private bool _isBoosting = false;
    private bool _hasGameStarted = false;
    
    public bool HasGameStarted
    {
        get => _hasGameStarted;
        set => _hasGameStarted = value;
    }

    private void Start()
    {
        SpawnWagons(wagonsCount);
        _boostParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        _positionHistory.Insert(0, transform.position);

        // Limit history size
        int requiredHistory = Mathf.CeilToInt(_wagonsList.Count * spacing * 10);
        if (_positionHistory.Count > requiredHistory)
        {
            _positionHistory.RemoveRange(requiredHistory, _positionHistory.Count - requiredHistory);
        }

        UpdateWagonsPositions();
        if (!HasGameStarted) return;
        
        // Handle rotation based on touch input
        float targetRotationX = Input.touchCount > 0 ? 45f : -45f;

        if (!_isBoosting)
        {
            if (targetRotationX != _currentTargetRotationX)
            {
                RotateToX(targetRotationX);
                _currentTargetRotationX = targetRotationX;
            }
        }


        // Move locomotive forward
        transform.Translate((Vector3.forward * -1) * speed * Time.deltaTime);

        // Record position for wagons to follow
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
    
    
    public void OnPortalTouched(GameplayEnums.PortalType portal, int value)
    {
        if (_isBoosting) return;

        float targetY = transform.position.y;
        float direction = 0f;

        switch (portal)
        {
            case PortalType.GREEN:
            case PortalType.AD:
                direction = 1f;
                targetY += value;
                break;
            case PortalType.RED:
                direction = -1f;
                targetY -= value;
                break;
        }

        if (direction != 0f)
        {
            StartCoroutine(BoostToY(targetY, direction));
        }
    }

    private System.Collections.IEnumerator BoostToY(float targetY, float direction)
    {
        _isBoosting = true;
        float originalSpeed = speed;
        float originalTurnSpeed = turnSpeed;
        speed = boostSpeed;
        float originalFollowSpeed = followSpeed;
        followSpeed += 500f;
        turnSpeed = 0f; // Prevent rotation
        _boostParticleSystem?.Play();

        // Set angle for boost (e.g., 60 up, -60 down)
        float boostAngle = direction > 0 ? 60f : -60f;
        RotateToX(boostAngle);

        while ((direction > 0 && transform.position.y < targetY) ||
               (direction < 0 && transform.position.y > targetY))
        {
            // Move in the boost direction
            transform.Translate(Vector3.up * direction * boostSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        // Snap to target Y
        Vector3 pos = transform.position;
        pos.y = targetY;
        transform.position = pos;

        // Restore controls
        speed = originalSpeed;
        turnSpeed = originalTurnSpeed;
        _isBoosting = false;
        followSpeed = originalFollowSpeed;
        _boostParticleSystem?.Stop();
        RotateToX(Input.touchCount > 0 ? 45f : -45f); // Restore angle
    }
}