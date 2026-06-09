using System.Collections.Generic;

namespace HabitCross.UI.Util
{
    /// <summary>
    /// Maps the reference app's MaterialCommunityIcons names to display glyphs.
    /// Lookups are pure in-code (no Resources / string asset paths), so the build
    /// stays obfuscation-safe. Unknown keys fall back to a neutral dot.
    /// </summary>
    public static class Glyphs
    {
        public const string Fallback = "•";

        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            // Habit / category icons
            { "run", "🏃" },
            { "dumbbell", "🏋" },
            { "meditation", "🧘" },
            { "book-open-variant", "📖" },
            { "water", "💧" },
            { "sleep", "😴" },
            { "food-apple", "🍎" },
            { "music", "🎵" },
            { "lightbulb-on", "💡" },
            { "heart-pulse", "❤" },
            { "yoga", "🧘" },
            { "pencil", "✏" },
            { "code-tags", "💻" },
            { "broom", "🧹" },
            { "leaf", "🍃" },
            { "account-group", "👥" },
            { "star-four-points", "✦" },

            // UI accent icons
            { "fire", "🔥" },
            { "trophy", "🏆" },
            { "star", "⭐" },
            { "medal", "🏅" },
            { "medal-outline", "🏅" },
            { "check", "✓" },
            { "check-bold", "✓" },
            { "check-circle", "✓" },
            { "plus", "+" },
            { "close", "✕" },
            { "arrow-left", "←" },
            { "arrow-right-bold", "→" },
            { "play-circle", "▶" },
            { "road-variant", "🛣" },
            { "chart-bar", "📊" },
            { "cog", "⚙" },
            { "bell", "🔔" },
            { "database", "🗄" },
            { "information", "ℹ" },
            { "trash-can", "🗑" },
            { "alert", "⚠" },
            { "walk", "🚶" },
            { "palette", "🎨" },
            { "run-fast", "🏃" },
        };

        public static string For(string key)
        {
            if (!string.IsNullOrEmpty(key) && Map.TryGetValue(key, out var g)) return g;
            return Fallback;
        }
    }
}
