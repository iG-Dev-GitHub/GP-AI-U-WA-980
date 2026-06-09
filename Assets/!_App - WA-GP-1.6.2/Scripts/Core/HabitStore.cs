using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HabitCross.Core
{
    [Serializable]
    internal class HabitListWrapper
    {
        public List<Habit> habits = new List<Habit>();
    }

    /// <summary>
    /// Offline-first persistence, replacing the web app's AsyncStorage. Data is
    /// stored as JSON in <see cref="Application.persistentDataPath"/>. A tiny
    /// PlayerPrefs flag tracks onboarding completion. No string-keyed asset
    /// loading is involved, so the build stays obfuscation-safe.
    /// </summary>
    public static class HabitStore
    {
        private const string OnboardingKey = "habitcross.onboarding.done";
        private const string FileName = "habits.v1.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static List<Habit> LoadHabits()
        {
            try
            {
                if (!File.Exists(FilePath)) return new List<Habit>();
                string raw = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(raw)) return new List<Habit>();
                var wrapper = JsonUtility.FromJson<HabitListWrapper>(raw);
                var list = wrapper?.habits ?? new List<Habit>();
                foreach (var h in list) h.Invalidate();
                return list;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HabitStore] load failed: {e.Message}");
                return new List<Habit>();
            }
        }

        public static void SaveHabits(List<Habit> habits)
        {
            try
            {
                var wrapper = new HabitListWrapper { habits = habits ?? new List<Habit>() };
                string json = JsonUtility.ToJson(wrapper);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HabitStore] save failed: {e.Message}");
            }
        }

        public static bool IsOnboardingDone() => PlayerPrefs.GetInt(OnboardingKey, 0) == 1;

        public static void MarkOnboardingDone()
        {
            PlayerPrefs.SetInt(OnboardingKey, 1);
            PlayerPrefs.Save();
        }

        public static void ResetAll()
        {
            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HabitStore] reset failed: {e.Message}");
            }
            PlayerPrefs.DeleteKey(OnboardingKey);
            PlayerPrefs.Save();
        }
    }
}
