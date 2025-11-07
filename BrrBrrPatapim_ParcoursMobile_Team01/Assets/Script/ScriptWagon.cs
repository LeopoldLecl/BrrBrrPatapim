using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using static GameplayEnums;

public class ScriptWagon : MonoBehaviour
{
    public static ScriptWagon Instance;

    [Header("Wagon Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float boostSpeed = 35f;
    [SerializeField] private float turnSpeed = 3f;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private int wagonsCount = 10;
    [SerializeField] private float firstWagonSpacing = 3f;
    [SerializeField] private KeyCode rotateButton = KeyCode.Space;


    //[SerializeField] private InputActionReference _click;
    
    
    
    [Header("Audio & FX")]
    [SerializeField] private AudioSource screamSource;
    [SerializeField] private AudioClip screamSound;
    [SerializeField] private float screamThreshold = 1f;
    [SerializeField] private ParticleSystem skidParticles;
    [SerializeField] private List<ParticleSystem> extraParticles;

    private List<Transform> _wagonsList = new List<Transform>();
    private List<Vector3> _positionHistory = new List<Vector3>();
    private Tween _currentRotationTween;
    private float _currentTargetRotationX;
    private ParticleSystem _boostParticleSystem;

    private float _descentTimer;
    private bool _isDescending;
    private bool _isBoosting;
    private bool _hasGameStarted;

    private SkinActivator _skinActivator;

    // --- Particle Set Runtime ---
    private GameObject _activeParticleSet;
    private List<ParticleSystem> _activeParticleSystems = new List<ParticleSystem>();

    public bool HasGameStarted
    {
        get => _hasGameStarted;
        set => _hasGameStarted = value;
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SpawnWagons(wagonsCount);
        _boostParticleSystem = GetComponentInChildren<ParticleSystem>();
        _skinActivator = FindFirstObjectByType<SkinActivator>();
        RefreshParticleSet();
    }

    private void Update()
    {
        _positionHistory.Insert(0, transform.position);
        int requiredHistory = Mathf.CeilToInt(_wagonsList.Count * spacing * 10);
        if (_positionHistory.Count > requiredHistory)
            _positionHistory.RemoveRange(requiredHistory, _positionHistory.Count - requiredHistory);

        UpdateWagonsPositions();
        if (!HasGameStarted) return;

        float targetRotationX = (Input.touchCount > 0
#if UNITY_EDITOR || UNITY_STANDALONE
          || Input.GetKey(rotateButton)
          //||_click.action.IsPressed()
#endif
        ) ? 45f : -45f;

        if (!_isBoosting)
        {
            if (Mathf.Abs(targetRotationX - _currentTargetRotationX) > 0.001f)
            {
                RotateToX(targetRotationX);
                _currentTargetRotationX = targetRotationX;
                PlayCurrentParticleSet();
            }
        }

        transform.Translate(speed * Time.deltaTime * Vector3.back);

        if (!_isBoosting)
        {
            if (targetRotationX < 0f)
            {
                _descentTimer += Time.deltaTime;
                if (_descentTimer >= screamThreshold && !_isDescending)
                {
                    _isDescending = true;
                    if (screamSource && screamSound && !screamSource.isPlaying)
                        screamSource.PlayOneShot(screamSound);
                }
            }
            else
            {
                _descentTimer = 0f;
                _isDescending = false;
            }
        }
    }

    private void RotateToX(float targetX)
    {
        if (_currentRotationTween != null && _currentRotationTween.IsActive())
            _currentRotationTween.Kill();

        Vector3 targetRotation = new Vector3(targetX, transform.localEulerAngles.y, transform.localEulerAngles.z);
        _currentRotationTween = transform.DOLocalRotate(targetRotation, turnSpeed).SetEase(Ease.OutQuad);
    }

    private void SpawnWagons(int count)
    {
        foreach (Transform wagon in _wagonsList)
        {
            if (wagon != null) Destroy(wagon.gameObject);
        }

        _wagonsList.Clear();
        Vector3 spawnPos = transform.position - transform.forward * firstWagonSpacing;

        for (int i = 0; i < count; i++)
        {
            GameObject wagonObj = Instantiate(prefab, spawnPos, transform.rotation);
            _wagonsList.Add(wagonObj.transform);
            spawnPos -= transform.forward * spacing;
        }
    }

    private void UpdateWagonsPositions()
    {
        for (int i = 0; i < _wagonsList.Count; i++)
        {
            if (_wagonsList[i] == null) continue;

            int posIndex = Mathf.Min(Mathf.RoundToInt((i + 1) * spacing * 10), _positionHistory.Count - 1);
            if (posIndex < 0 || posIndex >= _positionHistory.Count) continue;

            _wagonsList[i].position = Vector3.Lerp(_wagonsList[i].position,
                                                   _positionHistory[posIndex],
                                                   followSpeed * Time.deltaTime);

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

    // --- PARTICLE SET SYSTEM ---
    public void RefreshParticleSet()
    {
        // Nettoyage de l�ancien set
        if (_activeParticleSet != null)
            Destroy(_activeParticleSet);

        _activeParticleSystems.Clear();

        var manager = ParticleSelectionManager.Instance;
        if (manager == null) return;

        string key = manager.GetEquippedParticleKey();
        if (string.IsNullOrEmpty(key)) return;

        var allSets = Resources.LoadAll<ParticleSetScriptableObject>("");
        foreach (var set in allSets)
        {
            if (set.ParticleKey == key)
            {
                foreach (var prefab in set.ParticlePrefabs)
                {
                    if (prefab == null) continue;
                    var go = Instantiate(prefab, transform);
                    _activeParticleSystems.AddRange(go.GetComponentsInChildren<ParticleSystem>(true));
                    _activeParticleSet = go;
                }

                Debug.Log($"[ScriptWagon] Loaded particle set: {key}");
                return;
            }
        }

        Debug.LogWarning($"[ScriptWagon] No particle set found for key '{key}'");
    }

    private void PlayCurrentParticleSet()
    {
        if (_activeParticleSystems != null && _activeParticleSystems.Count > 0)
        {
            foreach (var ps in _activeParticleSystems)
                ps?.Play();
        }
        else
        {
            PlayDefaultParticles();
        }
    }

    private void PlayDefaultParticles()
    {
        if (extraParticles != null)
        {
            foreach (var ps in extraParticles)
                ps?.Play();
        }

        if (skidParticles != null)
            skidParticles.Play();
    }

    // --- BOOST / PORTALS ---
    public void OnPortalTouched(PortalType portal, int value)
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

    private IEnumerator BoostToY(float targetY, float direction)
    {
        _isBoosting = true;
        float originalSpeed = speed;
        float originalTurnSpeed = turnSpeed;
        float originalFollowSpeed = followSpeed;

        speed = boostSpeed;
        turnSpeed = 0f;
        followSpeed += 500f;
        _boostParticleSystem?.Play();

        float boostAngle = direction > 0 ? 60f : -60f;
        RotateToX(boostAngle);

        while ((direction > 0 && transform.position.y < targetY) ||
               (direction < 0 && transform.position.y > targetY))
        {
            transform.Translate(boostSpeed * Time.deltaTime * direction * Vector3.up, Space.World);
            yield return null;
        }

        Vector3 pos = transform.position;
        pos.y = targetY;
        transform.position = pos;

        speed = originalSpeed;
        turnSpeed = originalTurnSpeed;
        followSpeed = originalFollowSpeed;
        _isBoosting = false;
        _boostParticleSystem?.Stop();
        RotateToX(Input.touchCount > 0 ? 45f : -45f);
    }
}
