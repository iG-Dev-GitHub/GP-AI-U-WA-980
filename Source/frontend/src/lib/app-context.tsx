import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";

import { ensureAssets, type AssetPack } from "@/src/lib/assets";
import {
  HABIT_COLORS,
  createHabit,
  isOnboardingDone,
  loadHabits,
  markOnboardingDone,
  resetAll,
  saveHabits,
  toggleToday,
} from "@/src/lib/habits";
import type { Habit, HabitCategory } from "@/src/lib/types";

interface AppCtx {
  habits: Habit[];
  assets: AssetPack | null;
  assetsLoading: boolean;
  assetsStatus: string;
  onboardingDone: boolean | null;
  addHabit(input: {
    name: string;
    category: HabitCategory;
    color: string;
    icon: string;
    reminderTime?: string | null;
  }): Promise<Habit>;
  toggleHabitToday(id: string): Promise<void>;
  deleteHabit(id: string): Promise<void>;
  finishOnboarding(): Promise<void>;
  resetEverything(): Promise<void>;
}

const Ctx = createContext<AppCtx | null>(null);

export function AppProvider({ children }: { children: React.ReactNode }) {
  const [habits, setHabits] = useState<Habit[]>([]);
  const [assets, setAssets] = useState<AssetPack | null>(null);
  const [assetsLoading, setAssetsLoading] = useState(true);
  const [assetsStatus, setAssetsStatus] = useState("Loading…");
  const [onboardingDone, setOnboardingDone] = useState<boolean | null>(null);

  // Initial load
  useEffect(() => {
    let mounted = true;
    (async () => {
      const [h, ob] = await Promise.all([loadHabits(), isOnboardingDone()]);
      if (!mounted) return;
      setHabits(h);
      setOnboardingDone(ob);
    })();
    return () => {
      mounted = false;
    };
  }, []);

  // Asset fetch/generation (fire-and-forget, won't block UI)
  useEffect(() => {
    let mounted = true;
    (async () => {
      try {
        const pack = await ensureAssets((s) => mounted && setAssetsStatus(s));
        if (!mounted) return;
        setAssets(pack);
      } finally {
        if (mounted) setAssetsLoading(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, []);

  const persist = useCallback(async (next: Habit[]) => {
    setHabits(next);
    await saveHabits(next);
  }, []);

  const addHabit: AppCtx["addHabit"] = useCallback(
    async (input) => {
      const color = input.color || HABIT_COLORS[habits.length % HABIT_COLORS.length];
      const habit = createHabit({ ...input, color });
      await persist([...habits, habit]);
      return habit;
    },
    [habits, persist],
  );

  const toggleHabitToday: AppCtx["toggleHabitToday"] = useCallback(
    async (id) => {
      const next = habits.map((h) => (h.id === id ? toggleToday(h) : h));
      await persist(next);
    },
    [habits, persist],
  );

  const deleteHabit: AppCtx["deleteHabit"] = useCallback(
    async (id) => {
      await persist(habits.filter((h) => h.id !== id));
    },
    [habits, persist],
  );

  const finishOnboarding = useCallback(async () => {
    await markOnboardingDone();
    setOnboardingDone(true);
  }, []);

  const resetEverything = useCallback(async () => {
    await resetAll();
    setHabits([]);
    setOnboardingDone(false);
  }, []);

  const value = useMemo<AppCtx>(
    () => ({
      habits,
      assets,
      assetsLoading,
      assetsStatus,
      onboardingDone,
      addHabit,
      toggleHabitToday,
      deleteHabit,
      finishOnboarding,
      resetEverything,
    }),
    [
      habits,
      assets,
      assetsLoading,
      assetsStatus,
      onboardingDone,
      addHabit,
      toggleHabitToday,
      deleteHabit,
      finishOnboarding,
      resetEverything,
    ],
  );

  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}

export function useApp(): AppCtx {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error("useApp must be used within AppProvider");
  return ctx;
}
