import { storage } from "@/src/utils/storage";

import { addDays, daysBetween, todayKey } from "./date";
import type { Habit, HabitCategory, HabitState, HabitStatus } from "./types";

const HABITS_KEY = "habits.v1";
const ONBOARDING_KEY = "onboarding.done";

export const HABIT_COLORS = [
  "#FF4081",
  "#E040FB",
  "#536DFE",
  "#00B8D4",
  "#FFAB40",
  "#FF5252",
  "#64DD17",
  "#FFD600",
];

export const HABIT_CATEGORIES: { key: HabitCategory; label: string; icon: string }[] = [
  { key: "health", label: "Health", icon: "heart-pulse" },
  { key: "fitness", label: "Fitness", icon: "dumbbell" },
  { key: "mind", label: "Mind", icon: "meditation" },
  { key: "learn", label: "Learn", icon: "book-open-variant" },
  { key: "social", label: "Social", icon: "account-group" },
  { key: "other", label: "Other", icon: "star-four-points" },
];

export const HABIT_ICONS = [
  "run",
  "dumbbell",
  "meditation",
  "book-open-variant",
  "water",
  "sleep",
  "food-apple",
  "music",
  "lightbulb-on",
  "heart-pulse",
  "yoga",
  "pencil",
  "code-tags",
  "broom",
  "leaf",
  "account-group",
];

function uid(): string {
  return Math.random().toString(36).slice(2) + Date.now().toString(36);
}

export async function loadHabits(): Promise<Habit[]> {
  const raw = (await storage.getItem<string>(HABITS_KEY, "")) ?? "";
  if (!raw) return [];
  try {
    const parsed = JSON.parse(raw) as Habit[];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export async function saveHabits(habits: Habit[]): Promise<void> {
  await storage.setItem(HABITS_KEY, JSON.stringify(habits));
}

export async function isOnboardingDone(): Promise<boolean> {
  return (await storage.getItem<boolean>(ONBOARDING_KEY, false)) === true;
}

export async function markOnboardingDone(): Promise<void> {
  await storage.setItem(ONBOARDING_KEY, true);
}

export async function resetAll(): Promise<void> {
  await storage.removeItem(HABITS_KEY);
  await storage.removeItem(ONBOARDING_KEY);
}

export function createHabit(input: {
  name: string;
  category: HabitCategory;
  color: string;
  icon: string;
  reminderTime?: string | null;
}): Habit {
  return {
    id: uid(),
    name: input.name.trim(),
    category: input.category,
    color: input.color,
    icon: input.icon,
    reminderTime: input.reminderTime ?? null,
    createdAt: todayKey(),
    completions: {},
    bestStreak: 0,
  };
}

// Compute streak/status from completions.
// Streak counts consecutive completed days ending at today (or yesterday if
// today not yet done). Missed days = consecutive missed days before today.
export function computeState(habit: Habit, today: string = todayKey()): HabitState {
  const doneToday = !!habit.completions[today];
  const totalDone = Object.keys(habit.completions).length;

  if (totalDone === 0) {
    return {
      status: "fresh",
      currentStreak: 0,
      missedDays: 0,
      totalDone: 0,
      doneToday: false,
    };
  }

  // Streak: walk back from today (if done) or yesterday otherwise.
  let cursor = doneToday ? today : addDays(today, -1);
  let streak = 0;
  while (habit.completions[cursor]) {
    streak++;
    cursor = addDays(cursor, -1);
  }

  // Missed days BEFORE today (not including today).
  let missed = 0;
  if (!doneToday) {
    let m = addDays(today, -1);
    // Don't count days before the habit was created.
    const createdLimit = habit.createdAt;
    while (!habit.completions[m] && m >= createdLimit) {
      missed++;
      m = addDays(m, -1);
      if (missed > 365) break;
    }
  }

  let status: HabitStatus;
  if (doneToday) status = "today_done";
  else if (missed === 0) status = "ready";
  else if (missed >= 3) status = "burned";
  else status = "fire_warning";

  return {
    status,
    currentStreak: streak,
    missedDays: missed,
    totalDone,
    doneToday,
  };
}

export function toggleToday(habit: Habit, today: string = todayKey()): Habit {
  const completions = { ...habit.completions };
  if (completions[today]) {
    delete completions[today];
  } else {
    completions[today] = true;
  }
  const next = { ...habit, completions };
  const state = computeState(next, today);
  return {
    ...next,
    bestStreak: Math.max(habit.bestStreak, state.currentStreak),
  };
}

// Build the visible tile strip for a habit lane. Returns an array of recent
// past tiles + today + a few future tiles, with their tile state.
export type TileKind = "done" | "missed" | "today" | "future" | "milestone";

export interface Tile {
  dateKey: string;
  kind: TileKind;
  isMilestone: boolean; // 7-day multiple from start of current streak
}

export function buildTiles(
  habit: Habit,
  pastCount: number,
  futureCount: number,
  today: string = todayKey(),
): Tile[] {
  const tiles: Tile[] = [];
  // Past tiles
  for (let i = pastCount; i >= 1; i--) {
    const d = addDays(today, -i);
    const done = !!habit.completions[d];
    // Don't show "missed" for dates before habit was created.
    const before = daysBetween(habit.createdAt, d) < 0;
    tiles.push({
      dateKey: d,
      kind: before ? "future" : done ? "done" : "missed",
      isMilestone: false,
    });
  }
  // Today
  tiles.push({
    dateKey: today,
    kind: habit.completions[today] ? "done" : "today",
    isMilestone: false,
  });
  // Future
  for (let i = 1; i <= futureCount; i++) {
    tiles.push({
      dateKey: addDays(today, i),
      kind: "future",
      isMilestone: false,
    });
  }

  // Mark milestones: every 7th done day from the very first completion.
  const completedKeys = Object.keys(habit.completions).sort();
  if (completedKeys.length) {
    const ordered = new Set(
      completedKeys.filter((_, idx) => (idx + 1) % 7 === 0),
    );
    for (const t of tiles) {
      if (ordered.has(t.dateKey)) t.isMilestone = true;
    }
  }

  return tiles;
}

export function hasMonthMaster(habit: Habit): boolean {
  // Earned if best streak >= 30.
  return habit.bestStreak >= 30;
}
