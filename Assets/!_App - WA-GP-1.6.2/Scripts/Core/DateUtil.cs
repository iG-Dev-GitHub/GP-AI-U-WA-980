using System;
using System.Globalization;

namespace HabitCross.Core
{
    /// <summary>
    /// Calendar-date helpers. Like the reference web app, we work with local
    /// calendar dates encoded as <c>YYYY-MM-DD</c> strings to avoid timezone
    /// bugs across storage round-trips. Mirrors <c>src/lib/date.ts</c>.
    /// </summary>
    public static class DateUtil
    {
        /// <summary>Returns the YYYY-MM-DD key for the given (local) date.</summary>
        public static string TodayKey(DateTime? date = null)
        {
            DateTime d = date ?? DateTime.Now;
            return $"{d.Year:D4}-{d.Month:D2}-{d.Day:D2}";
        }

        /// <summary>Parses a YYYY-MM-DD key into a local <see cref="DateTime"/>.</summary>
        public static DateTime Parse(string key)
        {
            // Robust manual parse so the format never depends on the current culture.
            string[] parts = key.Split('-');
            int y = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int m = int.Parse(parts[1], CultureInfo.InvariantCulture);
            int d = int.Parse(parts[2], CultureInfo.InvariantCulture);
            return new DateTime(y, m, d);
        }

        /// <summary>Returns the key offset by <paramref name="delta"/> days.</summary>
        public static string AddDays(string key, int delta)
        {
            return TodayKey(Parse(key).AddDays(delta));
        }

        /// <summary>Whole-day difference (to - from). Negative if from is later.</summary>
        public static int DaysBetween(string from, string to)
        {
            return (int)Math.Round((Parse(to) - Parse(from)).TotalDays);
        }

        /// <summary>Long human date, e.g. "Mon, Jun 9".</summary>
        public static string FormatLongDate(string key)
        {
            return Parse(key).ToString("ddd, MMM d", CultureInfo.CurrentCulture);
        }

        /// <summary>Full human date, e.g. "Monday, June 9".</summary>
        public static string FormatFullDate(string key)
        {
            return Parse(key).ToString("dddd, MMMM d", CultureInfo.CurrentCulture);
        }

        /// <summary>Short, locale-aware date used for "since" labels.</summary>
        public static string FormatShortDate(string key)
        {
            return Parse(key).ToString("d", CultureInfo.CurrentCulture);
        }
    }
}
