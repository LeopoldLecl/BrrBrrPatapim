using UnityEngine;

public class ScorePowerUp : MonoBehaviour
{
    public enum PowerUpType { Lightning, Star }
    [SerializeField] private PowerUpType type;

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie que le tag est "Player"
        if (!other.CompareTag("Player")) return;

        // Essaye de récupérer le ScoreScript dans l'objet ou ses enfants
        ScoreScript scoreScript = other.GetComponentInChildren<ScoreScript>();
        if (scoreScript == null)
        {
            Debug.LogWarning(" Aucun ScoreScript trouvé sur l'objet avec le tag Player.");
            return;
        }

        switch (type)
        {
            case PowerUpType.Lightning:
                Debug.Log(" Éclair ramassé !");
                scoreScript.ActivateLightningBoost();
                break;

            case PowerUpType.Star:
                Debug.Log(" Étoile ramassée !");
                scoreScript.ActivateStarBoost();
                break;
        }

        gameObject.SetActive(false); 

    }
}
