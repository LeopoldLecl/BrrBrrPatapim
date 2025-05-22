using System;
using UnityEngine;
using static GameplayEnums;
using NaughtyAttributes;
using TMPro;

public class Portal : MonoBehaviour
{
    [OnValueChanged("ChangeMaterialAndValues")]
    [SerializeField] private PortalType portalType;

    [SerializeField] private GameObject portalPlane;

    [Header("Materials")]
    [SerializeField] private Material RedPortalMaterial;
    [SerializeField] private Material GreenPortalMaterial;
    [SerializeField] private Material AdPortalMaterial;

    [Header("UI")]
    [SerializeField] private TextMeshPro portalValue;

    [Header("Portal Value Range")]
    [SerializeField] private int minValue;
    [SerializeField] private int maxValue;

    [Header("Portal Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip redSound;
    [SerializeField] private AudioClip greenSound;
    [SerializeField] private AudioClip adSound;

    private int value;
    public int Value => value;

    private void Start()
    {
        RandomizePortalType();
        ChangeMaterialAndValues();
    }

    private void RandomizePortalType()
    {
        PortalType[] weightedTypes = new PortalType[]
        {
            PortalType.RED, PortalType.RED, PortalType.RED, PortalType.RED,
            PortalType.RED, PortalType.RED, PortalType.RED, PortalType.RED,
            PortalType.GREEN, PortalType.GREEN, PortalType.GREEN, PortalType.GREEN,
            PortalType.GREEN, PortalType.GREEN, PortalType.GREEN, PortalType.GREEN,
            PortalType.AD
        };
        portalType = weightedTypes[UnityEngine.Random.Range(0, weightedTypes.Length)];
    }

    private void OnTriggerEnter(Collider other)
    {
        //  Ignore everything except tagged "Player"
        if (!other.CompareTag("Player")) return;

        // Play corresponding sound
        if (audioSource != null)
        {
            switch (portalType)
            {
                case PortalType.GREEN:
                    if (greenSound) audioSource.PlayOneShot(greenSound);
                    break;
                case PortalType.RED:
                    if (redSound) audioSource.PlayOneShot(redSound);
                    break;
                case PortalType.AD:
                    if (adSound) audioSource.PlayOneShot(adSound);
                    break;
            }
        }

        other.GetComponent<ScriptWagon>()?.OnPortalTouched(portalType, Value);
    }

    private void ChangeMaterialAndValues()
    {
        switch (portalType)
        {
            case PortalType.GREEN:
                portalPlane.GetComponent<Renderer>().material = GreenPortalMaterial;
                break;
            case PortalType.RED:
                portalPlane.GetComponent<Renderer>().material = RedPortalMaterial;
                break;
            case PortalType.AD:
                portalPlane.GetComponent<Renderer>().material = AdPortalMaterial;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        CalculatePortalValue();
    }

    private void CalculatePortalValue()
    {
        value = UnityEngine.Random.Range(minValue, maxValue);
        string sign;

        switch (portalType)
        {
            case PortalType.GREEN:
                sign = "+";
                break;
            case PortalType.RED:
                sign = "-";
                break;
            case PortalType.AD:
                sign = "+";
                value *= 5;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        portalValue.text = $"{sign}{value}";
    }
}
