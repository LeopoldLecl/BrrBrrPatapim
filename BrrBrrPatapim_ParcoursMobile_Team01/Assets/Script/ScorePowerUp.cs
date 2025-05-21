using UnityEngine;

public class ScorePowerUp : MonoBehaviour
{
    public enum PowerUpType { Lightning, Star }
    [SerializeField] private PowerUpType type;

    private void OnTriggerEnter(Collider other)
    {
        // V�rifie que le tag est "Player"
        if (!other.CompareTag("Player")) return;

        // Essaye de r�cup�rer le ScoreScript dans l'objet ou ses enfants
        ScoreScript scoreScript = other.GetComponentInChildren<ScoreScript>();
        if (scoreScript == null)
        {
            return;
        }

        switch (type)
        {
            case PowerUpType.Lightning:
                scoreScript.ActivateLightningBoost();
                break;

            case PowerUpType.Star:
                scoreScript.ActivateStarBoost();
                break;
        }

        gameObject.SetActive(false); 

    }
}
