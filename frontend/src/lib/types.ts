export type HabitCategory =
  | "health"
  | "fitness"
  | "mind"
  | "learn"
  | "social"
  | "other";

export interface Habit {
  id: string;
  name: string;
  category: HabitCategory;
  color: string; // lane color
  icon: string; // MaterialCommunityIcons name
  reminderTime?: string | null; // HH:mm, kept for future use
  createdAt: string; // ISO date YYYY-MM-DD
  completions: Record<string, true>; // map of YYYY-MM-DD -> true
  bestStreak: number;
}

export type HabitStatus =
  | "fresh" // brand new, never completed
  | "today_done" // completed today
  | "ready" // not done today, no missed days yet
  | "fire_warning" // 1-2 days missed
  | "burned"; // 3+ days missed, streak reset

export interface HabitState {
  status: HabitStatus;
  currentStreak: number;
  missedDays: number; // consecutive missed days before today
  totalDone: number;
  doneToday: boolean;
}
