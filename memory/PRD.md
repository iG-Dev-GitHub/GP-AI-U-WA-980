# Habit Cross Daily Streaker — PRD

## Overview
Offline-first Android (Expo) habit tracker with a gamified mechanic: each habit is a road lane, and a cartoon runner advances daily as you check in. Missed days spawn fire blocks; 7-day streaks become golden milestone tiles; 30-day streaks unlock a "Month Master" badge. No authentication, all data stored locally.

## Tech Stack
- Frontend: Expo 54, expo-router, React Native 0.81, TypeScript, `@expo/vector-icons`
- Backend: FastAPI (only used once, on first launch, to generate cartoon assets via Gemini Nano Banana)
- Storage: AsyncStorage on device (offline-first)
- AI assets: Gemini Nano Banana via Emergent LLM key (custom transparent PNG character + fire + tile + badge)

## Screens
1. **Onboarding** (3 slides) — Add habit / Check in / Miss days
2. **Home (Road tab)** — list of habit lanes, Done Today CTA per habit, FAB
3. **Habit Detail** — full lane, current/best/total stats, Month Master banner, 8-week calendar
4. **Add Habit** — name, category, color, icon, optional reminder time
5. **Stats tab** — totals, 12-week heatmap, Month Master badges, per-habit records
6. **Settings tab** — in-app reminder toggle, reset all data, about

## Streak / Game Logic
- Done today → green tile, runner advances. `currentStreak` increases.
- 1–2 missed days → small fire ahead, "Fire Warning" banner. Runner stops before fire.
- 3+ missed days → large fire, streak burns (resets to 0) when next check-in happens.
- Every 7th completed day is marked as a Golden Mile tile. Banner appears at any current streak ≥7.
- `bestStreak ≥ 30` → Month Master badge displayed on Stats + detail.

## Backend Endpoints
- `GET /api/assets` → cached asset pack (`ready` flag, base64 PNGs keyed by name)
- `POST /api/assets/generate` → triggers Gemini generation if not cached
- `GET /api/assets/keys` → list of expected asset keys

## Out of Scope (per user choices)
- Push notifications (in-app only)
- Cloud sync / accounts / auth
- Localization beyond English
