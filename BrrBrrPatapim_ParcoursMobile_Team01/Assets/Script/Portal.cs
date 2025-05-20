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
    
    [SerializeField] private Material RedPortalMaterial;
    [SerializeField] private Material GreenPortalMaterial;
    [SerializeField] private Material AdPortalMaterial;
    
    [SerializeField] TextMeshPro portalValue;

    [SerializeField] private int minValue;
    [SerializeField] private int maxValue;
    
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
            PortalType.GREEN, PortalType.GREEN, PortalType.GREEN, PortalType.GREEN,
            PortalType.AD, PortalType.AD // Assuming AD is your "blue" portal
        };
        portalType = weightedTypes[UnityEngine.Random.Range(0, weightedTypes.Length)];
    }
    
    private void OnTriggerEnter(Collider other)
    {
        switch(portalType) 
        {
            case PortalType.GREEN:
                Debug.Log("Green portal entered");
                break;
            case PortalType.RED:
                Debug.Log("Red portal entered");
                break;
            case PortalType.AD:
                Debug.Log("Ad portal entered");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ChangeMaterialAndValues()
    {
        switch(portalType) 
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
        int value = UnityEngine.Random.Range(minValue, maxValue);
        string sign;

        switch(portalType)
        {
            case PortalType.GREEN:
                sign = "+";
                break;
            case PortalType.RED:
                sign = "-";
                break;
            case PortalType.AD:
                sign = "+";
                value *= 2;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Concatenate sign and value
        portalValue.text = $"{sign}{value}";
    }
}
