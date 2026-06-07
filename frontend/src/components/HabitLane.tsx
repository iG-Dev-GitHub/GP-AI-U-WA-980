import { useMemo } from "react";
import {
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";

import RoadTile, { TILE_GAP, TILE_SIZE } from "./RoadTile";
import { useApp } from "@/src/lib/app-context";
import { buildTiles, computeState } from "@/src/lib/habits";
import type { Habit } from "@/src/lib/types";

interface Props {
  habit: Habit;
  onPress?: () => void;
  onDone?: () => void;
}

const PAST = 5;
const FUTURE = 6;

export default function HabitLane({ habit, onPress, onDone }: Props) {
  const { assets } = useApp();
  const state = useMemo(() => computeState(habit), [habit]);
  const tiles = useMemo(() => buildTiles(habit, PAST, FUTURE), [habit]);

  // Character position: stands on today's tile if not done yet, or on the
  // most recently completed tile. If burned, sits behind the fire block.
  const todayIdx = PAST; // tiles[PAST] is today
  let characterIdx = todayIdx;
  if (state.doneToday) characterIdx = todayIdx;
  else if (state.status === "fire_warning" || state.status === "burned") {
    // stand on the tile right before the first fire
    let i = todayIdx - 1;
    while (i > 0 && tiles[i].kind === "missed") i--;
    characterIdx = i;
  }

  const characterAsset =
    state.status === "today_done"
      ? "runner_jumping"
      : state.status === "burned"
        ? "runner_stopped"
        : state.status === "fire_warning"
          ? "runner_stopped"
          : "runner_running";

  const fireAsset =
    state.status === "burned" || state.missedDays >= 3 ? "fire_large" : "fire_small";

  return (
    <TouchableOpacity
      testID={`habit-card-${habit.id}`}
      activeOpacity={0.9}
      onPress={onPress}
      style={styles.card}
    >
      <View style={styles.header}>
        <View style={[styles.iconChip, { backgroundColor: habit.color }]}>
          <MaterialCommunityIcons name={habit.icon as any} size={22} color="#fff" />
        </View>
        <View style={{ flex: 1 }}>
          <Text style={styles.name} numberOfLines={1} testID={`habit-name-${habit.id}`}>
            {habit.name}
          </Text>
          <View style={styles.metaRow}>
            <MaterialCommunityIcons name="fire" size={14} color="#FF6F00" />
            <Text style={styles.metaText} testID={`habit-streak-${habit.id}`}>
              {state.currentStreak} day streak
            </Text>
            {habit.bestStreak > 0 ? (
              <>
                <Text style={styles.metaDot}>·</Text>
                <MaterialCommunityIcons name="trophy" size={14} color="#FFB300" />
                <Text style={styles.metaText}>Best {habit.bestStreak}</Text>
              </>
            ) : null}
          </View>
        </View>
        <TouchableOpacity
          testID={`done-today-${habit.id}`}
          accessibilityRole="button"
          accessibilityLabel={
            state.doneToday ? "Mark as not done today" : "Mark as done today"
          }
          onPress={onDone}
          style={[
            styles.doneBtn,
            state.doneToday ? styles.doneBtnActive : styles.doneBtnIdle,
          ]}
        >
          <MaterialCommunityIcons
            name={state.doneToday ? "check-circle" : "check-bold"}
            size={20}
            color={state.doneToday ? "#fff" : "#1B5E20"}
          />
          <Text
            style={[
              styles.doneTxt,
              { color: state.doneToday ? "#fff" : "#1B5E20" },
            ]}
          >
            {state.doneToday ? "Done" : "Done Today"}
          </Text>
        </TouchableOpacity>
      </View>

      <View style={styles.laneWrap}>
        <View style={[styles.lane, { backgroundColor: shadeLane(habit.color) }]}>
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            contentContainerStyle={styles.tilesRow}
          >
            {tiles.map((t, i) => (
              <RoadTile
                key={t.dateKey}
                tile={t}
                laneColor={habit.color}
                assets={assets}
                showCharacter={i === characterIdx}
                characterAsset={characterAsset as any}
                fireAsset={fireAsset}
              />
            ))}
          </ScrollView>
        </View>
      </View>

      {state.status === "burned" ? (
        <View style={[styles.banner, { backgroundColor: "#FFEBEE" }]}>
          <MaterialCommunityIcons name="fire" size={16} color="#B71C1C" />
          <Text style={[styles.bannerTxt, { color: "#B71C1C" }]}>
            Streak burned · check in today to start again
          </Text>
        </View>
      ) : state.status === "fire_warning" ? (
        <View style={[styles.banner, { backgroundColor: "#FFF3E0" }]}>
          <MaterialCommunityIcons name="alert" size={16} color="#E65100" />
          <Text style={[styles.bannerTxt, { color: "#E65100" }]}>
            Fire ahead · 1 day missed, get back on track
          </Text>
        </View>
      ) : state.currentStreak >= 7 ? (
        <View style={[styles.banner, { backgroundColor: "#FFF8E1" }]}>
          <MaterialCommunityIcons name="star" size={16} color="#F57F17" />
          <Text style={[styles.bannerTxt, { color: "#F57F17" }]}>
            Golden Mile · {state.currentStreak} day streak
          </Text>
        </View>
      ) : null}
    </TouchableOpacity>
  );
}

function shadeLane(hex: string): string {
  // simple lighten by mixing with white at 0.85
  const m = /^#([0-9a-fA-F]{6})$/.exec(hex);
  if (!m) return "#ECEFF1";
  const r = parseInt(m[1].slice(0, 2), 16);
  const g = parseInt(m[1].slice(2, 4), 16);
  const b = parseInt(m[1].slice(4, 6), 16);
  const mix = (c: number) => Math.round(c + (255 - c) * 0.78);
  return `rgb(${mix(r)}, ${mix(g)}, ${mix(b)})`;
}

const LANE_HEIGHT = TILE_SIZE + 32;

const styles = StyleSheet.create({
  card: {
    backgroundColor: "#fff",
    borderRadius: 18,
    padding: 14,
    marginBottom: 14,
    borderBottomWidth: 4,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  header: { flexDirection: "row", alignItems: "center", gap: 12 },
  iconChip: {
    width: 40,
    height: 40,
    borderRadius: 12,
    alignItems: "center",
    justifyContent: "center",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.18)",
  },
  name: { fontSize: 17, fontWeight: "900", color: "#263238" },
  metaRow: { flexDirection: "row", alignItems: "center", marginTop: 2, gap: 4 },
  metaText: { fontSize: 12, color: "#546E7A", fontWeight: "700" },
  metaDot: { color: "#90A4AE", marginHorizontal: 4 },
  doneBtn: {
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
    paddingHorizontal: 12,
    paddingVertical: 10,
    borderRadius: 12,
    borderBottomWidth: 3,
  },
  doneBtnIdle: {
    backgroundColor: "#C8E6C9",
    borderBottomColor: "#1B5E20",
  },
  doneBtnActive: {
    backgroundColor: "#2E7D32",
    borderBottomColor: "#1B5E20",
  },
  doneTxt: { fontWeight: "900", fontSize: 13 },
  laneWrap: { marginTop: 14 },
  lane: {
    borderRadius: 14,
    paddingVertical: 18,
    paddingHorizontal: 10,
    minHeight: LANE_HEIGHT + 24,
  },
  tilesRow: {
    flexDirection: "row",
    alignItems: "center",
    paddingRight: TILE_GAP,
  },
  banner: {
    marginTop: 10,
    paddingHorizontal: 10,
    paddingVertical: 8,
    borderRadius: 10,
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
  },
  bannerTxt: { fontSize: 12, fontWeight: "800" },
});
