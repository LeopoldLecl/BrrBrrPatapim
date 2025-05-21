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
            Debug.LogWarning(" Aucun ScoreScript trouv� sur l'objet avec le tag Player.");
            return;
        }

        switch (type)
        {
            case PowerUpType.Lightning:
                Debug.Log(" �clair ramass� !");
                scoreScript.ActivateLightningBoost();
                break;

            case PowerUpType.Star:
                Debug.Log(" �toile ramass�e !");
                scoreScript.ActivateStarBoost();
                break;
        }

        gameObject.SetActive(false); 

    }
}
