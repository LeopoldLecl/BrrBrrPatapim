using UnityEngine;

[CreateAssetMenu(fileName = "ParticleSet", menuName = "Scriptable Objects/Particle Set")]
public class ParticleSetScriptableObject : ScriptableObject
{
    [Header("Particle Set Info")]
    [SerializeField] private string particleKey;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Particle Prefabs")]
    [SerializeField] private GameObject[] particlePrefabs;

    public string ParticleKey => particleKey;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject[] ParticlePrefabs => particlePrefabs;
}
