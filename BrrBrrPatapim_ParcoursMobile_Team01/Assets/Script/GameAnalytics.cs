using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;

namespace Script
{
    public class PlayerAnalyticsTracker : MonoBehaviour
    {
        private static PlayerAnalyticsTracker _instance;
        public static PlayerAnalyticsTracker Instance => _instance;

        private float sessionStartTime;
        private float inGameStartTime;
        private float totalInGameTime;

        private bool isInGame; // default false
        private bool analyticsInitialized; // default false

        private const string SessionCountKey = "SessionCount";
        private const string TotalPlaytimeKey = "TotalPlaytime";
        private const string TotalInGameTimeKey = "TotalInGameTime";
        private const string LastSessionDatesKey = "LastSessionDates"; // stores recent session dates (for week counting)

        private async void Awake()
        {
            _instance = this; // register singleton-like access
            DontDestroyOnLoad(gameObject);
            try
            {
                await UnityServices.InitializeAsync();
                analyticsInitialized = true;
                StartSession();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void StartSession()
        {
            sessionStartTime = Time.realtimeSinceStartup;
            totalInGameTime = 0f;

            int sessionCount = PlayerPrefs.GetInt(SessionCountKey, 0) + 1;
            PlayerPrefs.SetInt(SessionCountKey, sessionCount);

            string sessionDates = PlayerPrefs.GetString(LastSessionDatesKey, string.Empty);
            List<DateTime> dates = DeserializeDates(sessionDates);
            dates.Add(DateTime.UtcNow);
            dates.RemoveAll(d => (DateTime.UtcNow - d).TotalDays > 7); // keep only last 7 days
            PlayerPrefs.SetString(LastSessionDatesKey, SerializeDates(dates));

            PlayerPrefs.Save();
        }

        private void OnApplicationQuit()
        {
            EndSession();
        }

        private void EndSession()
        {
            float sessionDuration = Time.realtimeSinceStartup - sessionStartTime;
            float totalPlaytime = PlayerPrefs.GetFloat(TotalPlaytimeKey, 0f) + sessionDuration;
            float totalInGame = PlayerPrefs.GetFloat(TotalInGameTimeKey, 0f) + totalInGameTime;

            PlayerPrefs.SetFloat(TotalPlaytimeKey, totalPlaytime);
            PlayerPrefs.SetFloat(TotalInGameTimeKey, totalInGame);
            PlayerPrefs.Save();

            if (analyticsInitialized)
            {
                // Send a simple event name (payload not supported by current RecordEvent overloads)
                AnalyticsService.Instance.RecordEvent("session_end");
                AnalyticsService.Instance.Flush();
            }
        }

        // Called when the player enters or leaves actual gameplay
        public void SetInGameState(bool playing)
        {
            if (playing && !isInGame)
            {
                inGameStartTime = Time.realtimeSinceStartup;
                isInGame = true;

                if (analyticsInitialized)
                {
                    AnalyticsService.Instance.RecordEvent("gameplay_started");
                }
            }
            else if (!playing && isInGame)
            {
                totalInGameTime += Time.realtimeSinceStartup - inGameStartTime;
                isInGame = false;

                if (analyticsInitialized)
                {
                    AnalyticsService.Instance.RecordEvent("gameplay_ended");
                }
            }
        }

        // Static helpers for other scripts
        public static void RecordEventSafe(string eventName)
        {
            try
            {
                if (_instance != null && _instance.analyticsInitialized && !string.IsNullOrEmpty(eventName))
                {
                    AnalyticsService.Instance.RecordEvent(eventName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Analytics RecordEventSafe failed: {ex.Message}");
            }
        }

        public static void SetPlaying(bool playing)
        {
            _instance?.SetInGameState(playing);
        }

        // Public getters
        public float GetAverageSessionTime()
        {
            int sessionCount = PlayerPrefs.GetInt(SessionCountKey, 1);
            float totalPlaytime = PlayerPrefs.GetFloat(TotalPlaytimeKey, 0f);
            return sessionCount > 0 ? totalPlaytime / sessionCount : 0f;
        }

        public float GetAverageInGameTime()
        {
            int sessionCount = PlayerPrefs.GetInt(SessionCountKey, 1);
            float totalInGame = PlayerPrefs.GetFloat(TotalInGameTimeKey, 0f);
            return sessionCount > 0 ? totalInGame / sessionCount : 0f;
        }

        public int GetWeeklySessionCount()
        {
            string sessionDates = PlayerPrefs.GetString(LastSessionDatesKey, string.Empty);
            List<DateTime> dates = DeserializeDates(sessionDates);
            return dates.Count;
        }

        // Serialization helpers
        private string SerializeDates(List<DateTime> dates)
        {
            return string.Join("|", dates.ConvertAll(d => d.ToString("o")));
        }

        private List<DateTime> DeserializeDates(string data)
        {
            var result = new List<DateTime>();
            if (string.IsNullOrEmpty(data)) return result;

            string[] parts = data.Split('|');
            foreach (var p in parts)
            {
                if (DateTime.TryParse(p, out DateTime d))
                    result.Add(d);
            }
            return result;
        }
    }
}
