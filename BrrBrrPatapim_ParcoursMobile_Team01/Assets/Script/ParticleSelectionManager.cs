using UnityEngine;

/// <summary>
/// Gère la sélection, la sauvegarde et le chargement du set de particules actif.
/// Fonctionne comme le SkinsSelectionManager mais pour les effets visuels.
/// </summary>
public class ParticleSelectionManager : MonoBehaviour
{
    public static ParticleSelectionManager Instance;

    private const string EquippedParticleKey = "EQUIPPED_PARTICLE_KEY";
    private string equippedParticleKey;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        equippedParticleKey = PlayerPrefs.GetString(EquippedParticleKey, string.Empty);
        if (!string.IsNullOrEmpty(equippedParticleKey))
        {
            Debug.Log($"[ParticleSelectionManager] Restored equipped set: {equippedParticleKey}");
        }
    }

    /// <summary>
    /// Équipe le set de particules donné et sauvegarde le choix du joueur.
    /// </summary>
    public void EquipParticleSet(ParticleSetScriptableObject particleSet)
    {
        if (particleSet == null)
        {
            Debug.LogWarning("[ParticleSelectionManager] Tried to equip a null particle set.");
            return;
        }

        equippedParticleKey = particleSet.ParticleKey;

        PlayerPrefs.SetString(EquippedParticleKey, equippedParticleKey);
        PlayerPrefs.Save();

        Debug.Log($"[ParticleSelectionManager] Equipped particle set: {equippedParticleKey}");

        // Actualise immédiatement les particules sur le wagon
        var wagon = FindFirstObjectByType<ScriptWagon>();
        if (wagon != null)
            wagon.RefreshParticleSet();
    }

    /// <summary>
    /// Retourne la clé du set de particules actuellement équipé.
    /// </summary>
    public string GetEquippedParticleKey()
    {
        return equippedParticleKey;
    }

    /// <summary>
    /// Réinitialise la sélection (aucun set équipé).
    /// </summary>
    public void ResetParticleSelection()
    {
        PlayerPrefs.DeleteKey(EquippedParticleKey);
        equippedParticleKey = string.Empty;

        Debug.Log("[ParticleSelectionManager] Particle selection reset.");

        var wagon = FindFirstObjectByType<ScriptWagon>();
        if (wagon != null)
            wagon.RefreshParticleSet();
    }
}
