using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EndGameScores : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI leaderboardText;
    
    [SerializeField] private List<string> playerNames = new List<string>();
    public List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
    

    public void GenerateLeaderboard(int playerScore)
    {
        leaderboard.Clear();

        // Ensure we have at least 9 fake names
        List<string> fakeNames = new List<string>(playerNames);
        while (fakeNames.Count < 9)
            fakeNames.Add("Bot" + (fakeNames.Count + 1));

        // Generate 4 lower and 5 higher fake scores around the player's score
        List<int> fakeScores = new List<int>();
        int baseScore = playerScore;
        for (int i = 4; i > 0; i--)
            fakeScores.Add(baseScore - Random.Range(500, 5000) * i);
        for (int i = 1; i <= 5; i++)
            fakeScores.Add(baseScore + Random.Range(500, 5000) * i);

        // Shuffle fake names for randomness
        for (int i = fakeNames.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = fakeNames[i];
            fakeNames[i] = fakeNames[j];
            fakeNames[j] = temp;
        }

        // Add 4 lower scores
        for (int i = 0; i < 4; i++)
            leaderboard.Add(new LeaderboardEntry(fakeNames[i], fakeScores[i]));

        // Add actual player in the middle
        leaderboard.Add(new LeaderboardEntry("You", playerScore));

        // Add 5 higher scores
        for (int i = 4; i < 9; i++)
            leaderboard.Add(new LeaderboardEntry(fakeNames[i], fakeScores[i]));

        // Optionally, sort descending
        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));
        
        DisplayLeaderboard();
    }
    
    public void DisplayLeaderboard()
    {
        if (leaderboardText == null) return;

        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < leaderboard.Count; i++)
        {
            var entry = leaderboard[i];
            string name = entry.playerName == "You" ? "<color=red>You</color>" : entry.playerName;
            sb.AppendLine($"{i + 1}. {name}  {entry.score}");
        }

        leaderboardText.text = sb.ToString();
    }
}

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;

    public LeaderboardEntry(string name, int score)
    {
        playerName = name;
        this.score = score;
    }
}
