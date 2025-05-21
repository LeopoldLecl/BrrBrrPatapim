using UnityEngine;

public class ScorePowerUp : MonoBehaviour
{
    public enum PowerUpType { Lightning, Star }
    [SerializeField] private PowerUpType type;

    private void OnTriggerEnter(Collider other)
    {
        // Essaye de trouver le ScoreScript sur le joueur
        ScoreScript scoreScript = other.GetComponentInChildren<ScoreScript>();
        if (scoreScript == null) return;

        switch (type)
        {
            case PowerUpType.Lightning:
                scoreScript.ActivateLightningBoost();
                break;
            case PowerUpType.Star:
                scoreScript.ActivateStarBoost();
                break;
        }

        Destroy(gameObject); // Supprime le power-up une fois ramassé
    }
}
