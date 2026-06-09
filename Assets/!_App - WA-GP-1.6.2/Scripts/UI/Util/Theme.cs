using UnityEngine;

namespace HabitCross.UI.Util
{
    /// <summary>
    /// Central design palette ported from <c>design_guidelines.json</c>. Kept in
    /// code so the visual language never depends on string-keyed asset lookups.
    /// </summary>
    public static class Theme
    {
        public static readonly Color PrimaryBackground = Hex("#80D8FF");
        public static readonly Color ScreenBackground  = Hex("#E1F5FE");
        public static readonly Color AddBackground     = Hex("#F5FBFF");
        public static readonly Color Surface           = Hex("#FFFFFF");
        public static readonly Color SuccessTile       = Hex("#64DD17");
        public static readonly Color MissedTile        = Hex("#FFCDD2");
        public static readonly Color FutureTile        = Hex("#ECEFF1");
        public static readonly Color MilestoneGold     = Hex("#FFD600");
        public static readonly Color TextPrimary       = Hex("#263238");
        public static readonly Color TextSecondary     = Hex("#546E7A");
        public static readonly Color TextMuted         = Hex("#90A4AE");
        public static readonly Color Action            = Hex("#2962FF");
        public static readonly Color ActionDeep        = Hex("#0D47A1");
        public static readonly Color Shadow            = new Color(0f, 0f, 0f, 0.08f);
        public static readonly Color ShadowStrong      = new Color(0f, 0f, 0f, 0.18f);
        public static readonly Color Danger            = Hex("#E53935");
        public static readonly Color DangerDeep        = Hex("#B71C1C");

        /// <summary>Parses "#RRGGBB" or "#RRGGBBAA". Returns magenta on failure.</summary>
        public static Color Hex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.magenta;
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.magenta;
        }

        /// <summary>Lightens a hex lane color by mixing with white (matches shadeLane()).</summary>
        public static Color ShadeLane(string hex)
        {
            var c = Hex(hex);
            const float t = 0.78f;
            return new Color(
                c.r + (1f - c.r) * t,
                c.g + (1f - c.g) * t,
                c.b + (1f - c.b) * t,
                1f);
        }

        public static Color HeatColor(float pct)
        {
            if (pct <= 0f) return Hex("#ECEFF1");
            if (pct < 0.34f) return Hex("#C5E1A5");
            if (pct < 0.67f) return Hex("#9CCC65");
            if (pct < 1f) return Hex("#7CB342");
            return Hex("#558B2F");
        }
    }
}
