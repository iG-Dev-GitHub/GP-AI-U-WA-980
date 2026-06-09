using System;
using System.Collections.Generic;

namespace HabitCross.Core
{
    /// <summary>
    /// Pure streak/tile game logic. A faithful port of <c>src/lib/habits.ts</c>
    /// from the reference web app, including the lane character placement rules
    /// from <c>HabitLane.tsx</c>.
    /// </summary>
    public static class HabitLogic
    {
        private static string UidImpl()
        {
            // Random + time component, like the web app's Math.random()+Date.now().
            return Guid.NewGuid().ToString("N").Substring(0, 16) +
                   DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
        }

        public static Habit CreateHabit(string name, HabitCategory category, string color,
            string icon, string reminderTime, string today = null)
        {
            today ??= DateUtil.TodayKey();
            return new Habit
            {
                id = UidImpl(),
                name = (name ?? string.Empty).Trim(),
                category = HabitCategoryUtil.ToToken(category),
                color = color,
                icon = icon,
                reminderTime = string.IsNullOrEmpty(reminderTime) ? null : reminderTime,
                createdAt = today,
                completions = new List<string>(),
                bestStreak = 0,
            };
        }

        /// <summary>
        /// Computes streak/status from completions. Streak counts consecutive
        /// completed days ending at today (or yesterday if today isn't done yet).
        /// Missed days = consecutive missed days before today.
        /// </summary>
        public static HabitState ComputeState(Habit habit, string today = null)
        {
            today ??= DateUtil.TodayKey();
            bool doneToday = habit.IsDone(today);
            int totalDone = habit.CompletionCount;

            if (totalDone == 0)
            {
                return new HabitState
                {
                    status = HabitStatus.Fresh,
                    currentStreak = 0,
                    missedDays = 0,
                    totalDone = 0,
                    doneToday = false,
                };
            }

            // Streak: walk back from today (if done) or yesterday otherwise.
            string cursor = doneToday ? today : DateUtil.AddDays(today, -1);
            int streak = 0;
            while (habit.IsDone(cursor))
            {
                streak++;
                cursor = DateUtil.AddDays(cursor, -1);
            }

            // Missed days BEFORE today (not including today).
            int missed = 0;
            if (!doneToday)
            {
                string m = DateUtil.AddDays(today, -1);
                string createdLimit = habit.createdAt;
                while (!habit.IsDone(m) && string.CompareOrdinal(m, createdLimit) >= 0)
                {
                    missed++;
                    m = DateUtil.AddDays(m, -1);
                    if (missed > 365) break;
                }
            }

            HabitStatus status;
            if (doneToday) status = HabitStatus.TodayDone;
            else if (missed == 0) status = HabitStatus.Ready;
            else if (missed >= 3) status = HabitStatus.Burned;
            else status = HabitStatus.FireWarning;

            return new HabitState
            {
                status = status,
                currentStreak = streak,
                missedDays = missed,
                totalDone = totalDone,
                doneToday = doneToday,
            };
        }

        /// <summary>Toggles today's completion and updates the best streak.</summary>
        public static void ToggleToday(Habit habit, string today = null)
        {
            today ??= DateUtil.TodayKey();
            bool done = habit.IsDone(today);
            habit.SetDone(today, !done);
            var state = ComputeState(habit, today);
            habit.bestStreak = Math.Max(habit.bestStreak, state.currentStreak);
        }

        /// <summary>
        /// Builds the visible tile strip: recent past tiles + today + a few future
        /// tiles, each with its computed kind and milestone flag.
        /// </summary>
        public static List<Tile> BuildTiles(Habit habit, int pastCount, int futureCount, string today = null)
        {
            today ??= DateUtil.TodayKey();
            var tiles = new List<Tile>(pastCount + 1 + futureCount);

            for (int i = pastCount; i >= 1; i--)
            {
                string d = DateUtil.AddDays(today, -i);
                bool done = habit.IsDone(d);
                bool before = DateUtil.DaysBetween(habit.createdAt, d) < 0;
                tiles.Add(new Tile
                {
                    dateKey = d,
                    kind = before ? TileKind.Future : (done ? TileKind.Done : TileKind.Missed),
                    isMilestone = false,
                });
            }

            tiles.Add(new Tile
            {
                dateKey = today,
                kind = habit.IsDone(today) ? TileKind.Done : TileKind.Today,
                isMilestone = false,
            });

            for (int i = 1; i <= futureCount; i++)
            {
                tiles.Add(new Tile
                {
                    dateKey = DateUtil.AddDays(today, i),
                    kind = TileKind.Future,
                    isMilestone = false,
                });
            }

            // Mark milestones: every 7th done day from the very first completion.
            var completedKeys = habit.SortedCompletions();
            if (completedKeys.Count > 0)
            {
                var ordered = new HashSet<string>();
                for (int idx = 0; idx < completedKeys.Count; idx++)
                {
                    if ((idx + 1) % 7 == 0) ordered.Add(completedKeys[idx]);
                }
                for (int i = 0; i < tiles.Count; i++)
                {
                    if (ordered.Contains(tiles[i].dateKey))
                    {
                        var t = tiles[i];
                        t.isMilestone = true;
                        tiles[i] = t;
                    }
                }
            }

            return tiles;
        }

        /// <summary>Earned at best streak >= 30.</summary>
        public static bool HasMonthMaster(Habit habit) => habit.bestStreak >= 30;

        /// <summary>
        /// Index of the tile the runner stands on, mirroring HabitLane.tsx.
        /// <paramref name="pastCount"/> is the index of today's tile.
        /// </summary>
        public static int CharacterIndex(IReadOnlyList<Tile> tiles, HabitState state, int pastCount)
        {
            int todayIdx = pastCount;
            int characterIdx = todayIdx;
            if (state.doneToday)
            {
                characterIdx = todayIdx;
            }
            else if (state.status == HabitStatus.FireWarning || state.status == HabitStatus.Burned)
            {
                int i = todayIdx - 1;
                while (i > 0 && tiles[i].kind == TileKind.Missed) i--;
                characterIdx = i;
            }
            return characterIdx;
        }
    }
}
