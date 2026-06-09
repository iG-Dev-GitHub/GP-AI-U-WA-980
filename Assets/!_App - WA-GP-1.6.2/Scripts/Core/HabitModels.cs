using System;
using System.Collections.Generic;

namespace HabitCross.Core
{
    /// <summary>Habit category, mirrors the union type in <c>src/lib/types.ts</c>.</summary>
    public enum HabitCategory
    {
        Health,
        Fitness,
        Mind,
        Learn,
        Social,
        Other
    }

    /// <summary>Derived per-habit status used to drive lane visuals.</summary>
    public enum HabitStatus
    {
        Fresh,        // brand new, never completed
        TodayDone,    // completed today
        Ready,        // not done today, no missed days yet
        FireWarning,  // 1-2 days missed
        Burned        // 3+ days missed, streak reset
    }

    /// <summary>The kind of a single road tile.</summary>
    public enum TileKind
    {
        Done,
        Missed,
        Today,
        Future,
        Milestone
    }

    /// <summary>
    /// Persistent habit record. Serializable with <see cref="UnityEngine.JsonUtility"/>.
    /// Completions are stored as a sorted list of YYYY-MM-DD keys (JsonUtility cannot
    /// serialize dictionaries); membership lookups use a lazily built hash set.
    /// </summary>
    [Serializable]
    public class Habit
    {
        public string id;
        public string name;
        public string category;     // HabitCategory serialized as its lowercase token
        public string color;        // hex lane color, e.g. "#FF4081"
        public string icon;         // glyph/icon key
        public string reminderTime; // "HH:mm" or empty
        public string createdAt;    // YYYY-MM-DD
        public List<string> completions = new List<string>();
        public int bestStreak;

        [NonSerialized] private HashSet<string> _set;

        /// <summary>True if the habit was completed on the given date key.</summary>
        public bool IsDone(string dateKey)
        {
            EnsureSet();
            return _set.Contains(dateKey);
        }

        public int CompletionCount
        {
            get { EnsureSet(); return _set.Count; }
        }

        public void SetDone(string dateKey, bool done)
        {
            EnsureSet();
            if (done)
            {
                if (_set.Add(dateKey)) completions.Add(dateKey);
            }
            else if (_set.Remove(dateKey))
            {
                completions.Remove(dateKey);
            }
        }

        /// <summary>Completion keys in ascending order (stable for milestone math).</summary>
        public List<string> SortedCompletions()
        {
            var copy = new List<string>(completions);
            copy.Sort(StringComparer.Ordinal);
            return copy;
        }

        /// <summary>Rebuilds the membership cache after deserialization or edits.</summary>
        public void Invalidate() => _set = null;

        private void EnsureSet()
        {
            if (_set != null) return;
            _set = new HashSet<string>();
            if (completions == null) completions = new List<string>();
            foreach (var k in completions) _set.Add(k);
        }

        public HabitCategory CategoryEnum => HabitCategoryUtil.FromToken(category);
    }

    /// <summary>Snapshot of a habit's computed gameplay state.</summary>
    public struct HabitState
    {
        public HabitStatus status;
        public int currentStreak;
        public int missedDays;
        public int totalDone;
        public bool doneToday;
    }

    /// <summary>A single tile on a habit's road strip.</summary>
    public struct Tile
    {
        public string dateKey;
        public TileKind kind;
        public bool isMilestone;
    }

    /// <summary>Token conversion for <see cref="HabitCategory"/>.</summary>
    public static class HabitCategoryUtil
    {
        public static string ToToken(HabitCategory c) => c.ToString().ToLowerInvariant();

        public static HabitCategory FromToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return HabitCategory.Other;
            switch (token.ToLowerInvariant())
            {
                case "health": return HabitCategory.Health;
                case "fitness": return HabitCategory.Fitness;
                case "mind": return HabitCategory.Mind;
                case "learn": return HabitCategory.Learn;
                case "social": return HabitCategory.Social;
                default: return HabitCategory.Other;
            }
        }
    }
}
