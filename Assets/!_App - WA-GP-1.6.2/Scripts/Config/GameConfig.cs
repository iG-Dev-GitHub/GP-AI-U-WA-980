using System;
using System.Collections.Generic;
using UnityEngine;

namespace HabitCross.Config
{
    [Serializable]
    public class CategoryDef
    {
        public string key;   // token: health / fitness / ...
        public string label; // display label
        public string icon;  // icon key (see Glyphs)
    }

    /// <summary>
    /// All design data (palette, habit colors, categories, icon set) in one
    /// strongly-typed ScriptableObject. This replaces any need for the Resources
    /// folder or string-keyed lookups: the app receives this via a serialized
    /// reference, and falls back to <see cref="CreateDefault"/> if none is wired.
    /// Mirrors constants from <c>src/lib/habits.ts</c> and <c>design_guidelines.json</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Habit Cross/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Habit palette (hex)")]
        public string[] habitColors =
        {
            "#FF4081", "#E040FB", "#536DFE", "#00B8D4",
            "#FFAB40", "#FF5252", "#64DD17", "#FFD600",
        };

        [Header("Categories")]
        public CategoryDef[] categories =
        {
            new CategoryDef { key = "health",  label = "Health",  icon = "heart-pulse" },
            new CategoryDef { key = "fitness", label = "Fitness", icon = "dumbbell" },
            new CategoryDef { key = "mind",    label = "Mind",    icon = "meditation" },
            new CategoryDef { key = "learn",   label = "Learn",   icon = "book-open-variant" },
            new CategoryDef { key = "social",  label = "Social",  icon = "account-group" },
            new CategoryDef { key = "other",   label = "Other",   icon = "star-four-points" },
        };

        [Header("Selectable habit icons")]
        public string[] icons =
        {
            "run", "dumbbell", "meditation", "book-open-variant", "water", "sleep",
            "food-apple", "music", "lightbulb-on", "heart-pulse", "yoga", "pencil",
            "code-tags", "broom", "leaf", "account-group",
        };

        /// <summary>Builds an in-memory default identical to the asset's defaults.</summary>
        public static GameConfig CreateDefault()
        {
            return CreateInstance<GameConfig>();
        }

        public string ColorForIndex(int index)
        {
            if (habitColors == null || habitColors.Length == 0) return "#FF4081";
            return habitColors[((index % habitColors.Length) + habitColors.Length) % habitColors.Length];
        }

        public CategoryDef CategoryByKey(string key)
        {
            if (categories != null)
            {
                foreach (var c in categories)
                    if (c.key == key) return c;
            }
            return categories != null && categories.Length > 0 ? categories[0] : null;
        }
    }
}
