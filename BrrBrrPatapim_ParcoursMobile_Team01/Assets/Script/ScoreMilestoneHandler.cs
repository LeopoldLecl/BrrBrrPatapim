using UnityEngine;

public class ScoreMilestoneHandler : MonoBehaviour
{
    [System.Serializable]
    public class ScoreMilestone
    {
        public int requiredScore = 300000;
        public GameObject toDisable;
        public GameObject toEnable;
        [HideInInspector] public bool triggered = false;
    }

    [Header("References")]
    [SerializeField] private ScoreScript scoreScript;

    [Header("Milestones")]
    [SerializeField] private ScoreMilestone[] milestones;

    void Update()
    {
        int currentScore = GetScore();

        foreach (var milestone in milestones)
        {
            if (!milestone.triggered && currentScore >= milestone.requiredScore)
            {
                if (milestone.toDisable != null)
                    milestone.toDisable.SetActive(false);

                if (milestone.toEnable != null)
                    milestone.toEnable.SetActive(true);

                milestone.triggered = true;
            }
        }
    }

    private int GetScore()
    {
        var scoreField = typeof(ScoreScript).GetField("actualScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return scoreField != null ? (int)scoreField.GetValue(scoreScript) : 0;
    }
}
