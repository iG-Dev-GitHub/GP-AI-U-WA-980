import { useMemo } from "react";
import {
  Image,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView, useSafeAreaInsets } from "react-native-safe-area-context";
import { useLocalSearchParams, useRouter } from "expo-router";
import { MaterialCommunityIcons } from "@expo/vector-icons";

import HabitLane from "@/src/components/HabitLane";
import { useApp } from "@/src/lib/app-context";
import { dataUri } from "@/src/lib/assets";
import { addDays, todayKey } from "@/src/lib/date";
import { computeState, hasMonthMaster } from "@/src/lib/habits";

const CAL_WEEKS = 8;

function formatCreated(key: string): string {
  const [y, m, d] = key.split("-").map(Number);
  return new Date(y, m - 1, d).toLocaleDateString();
}

export default function HabitDetail() {
  const insets = useSafeAreaInsets();
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const { habits, toggleHabitToday, deleteHabit, assets } = useApp();

  const habit = habits.find((h) => h.id === id);

  const days = useMemo(() => {
    if (!habit) return [];
    const today = todayKey();
    const arr: { key: string; done: boolean; isToday: boolean }[] = [];
    for (let i = CAL_WEEKS * 7 - 1; i >= 0; i--) {
      const k = addDays(today, -i);
      arr.push({
        key: k,
        done: !!habit.completions[k],
        isToday: k === today,
      });
    }
    return arr;
  }, [habit]);

  if (!habit) {
    return (
      <SafeAreaView style={styles.container} edges={["top"]}>
        <Text style={styles.missing}>Habit not found.</Text>
        <TouchableOpacity onPress={() => router.back()} style={styles.backBtn}>
          <Text style={styles.backTxt}>Go back</Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  const state = computeState(habit);
  const badge = hasMonthMaster(habit);
  const badgeUri = dataUri(assets?.badge_month_master);

  return (
    <SafeAreaView
      style={styles.container}
      edges={["top"]}
      testID={`detail-${habit.id}`}
    >
      <View style={styles.header}>
        <TouchableOpacity
          onPress={() => router.back()}
          style={styles.iconBtn}
          testID="detail-back"
        >
          <MaterialCommunityIcons name="arrow-left" size={22} color="#263238" />
        </TouchableOpacity>
        <View style={{ flex: 1 }}>
          <Text style={styles.title} numberOfLines={1}>
            {habit.name}
          </Text>
          <Text style={styles.subtitle}>since {formatCreated(habit.createdAt)}</Text>
        </View>
        <TouchableOpacity
          onPress={() => deleteHabit(habit.id).then(() => router.back())}
          style={styles.iconBtn}
          testID="detail-delete"
        >
          <MaterialCommunityIcons name="trash-can" size={20} color="#E53935" />
        </TouchableOpacity>
      </View>

      <ScrollView
        contentContainerStyle={{ paddingBottom: insets.bottom + 80, padding: 16 }}
      >
        <HabitLane habit={habit} onDone={() => toggleHabitToday(habit.id)} />

        <View style={styles.statsRow}>
          <StatBlock
            color="#FF6F00"
            icon="fire"
            label="Current"
            value={`${state.currentStreak}d`}
          />
          <StatBlock
            color="#FFB300"
            icon="trophy"
            label="Best"
            value={`${habit.bestStreak}d`}
          />
          <StatBlock
            color="#2962FF"
            icon="walk"
            label="Total"
            value={`${state.totalDone}d`}
          />
        </View>

        {badge ? (
          <View style={styles.badgeBanner}>
            {badgeUri ? (
              <Image source={{ uri: badgeUri }} style={styles.badgeImg} />
            ) : (
              <MaterialCommunityIcons name="medal" size={48} color="#FFD600" />
            )}
            <View style={{ flex: 1 }}>
              <Text style={styles.badgeTitle}>Month Master</Text>
              <Text style={styles.badgeSub}>
                Earned at {habit.bestStreak}-day streak
              </Text>
            </View>
          </View>
        ) : null}

        <Text style={styles.sectionTitle}>Calendar</Text>
        <View style={styles.calCard}>
          <View style={styles.calGrid}>
            {days.map((d) => (
              <View
                key={d.key}
                testID={`cal-${d.key}`}
                style={[
                  styles.calCell,
                  {
                    backgroundColor: d.done
                      ? "#64DD17"
                      : d.isToday
                        ? habit.color
                        : "#ECEFF1",
                  },
                  d.isToday && { borderWidth: 2, borderColor: "#263238" },
                ]}
              >
                {d.done ? (
                  <MaterialCommunityIcons name="check" size={12} color="#fff" />
                ) : null}
              </View>
            ))}
          </View>
          <Text style={styles.calCaption}>
            Last {CAL_WEEKS * 7} days · today is highlighted
          </Text>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

function StatBlock({
  icon,
  label,
  value,
  color,
}: {
  icon: string;
  label: string;
  value: string;
  color: string;
}) {
  return (
    <View style={styles.statBlock}>
      <MaterialCommunityIcons name={icon as any} size={18} color={color} />
      <Text style={styles.statValue}>{value}</Text>
      <Text style={styles.statLabel}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#E1F5FE" },
  header: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 12,
    paddingVertical: 10,
    gap: 8,
  },
  iconBtn: {
    width: 40,
    height: 40,
    backgroundColor: "#fff",
    borderRadius: 12,
    alignItems: "center",
    justifyContent: "center",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  title: { fontSize: 18, fontWeight: "900", color: "#263238" },
  subtitle: { fontSize: 11, color: "#546E7A", fontWeight: "700" },
  statsRow: { flexDirection: "row", gap: 10, marginTop: 4 },
  statBlock: {
    flex: 1,
    backgroundColor: "#fff",
    borderRadius: 12,
    paddingVertical: 12,
    alignItems: "center",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  statValue: { fontWeight: "900", fontSize: 18, color: "#263238", marginTop: 2 },
  statLabel: { fontSize: 11, color: "#546E7A", fontWeight: "700" },
  badgeBanner: {
    marginTop: 14,
    backgroundColor: "#FFFDE7",
    borderRadius: 14,
    padding: 12,
    flexDirection: "row",
    alignItems: "center",
    gap: 12,
    borderBottomWidth: 3,
    borderBottomColor: "#FBC02D",
  },
  badgeImg: { width: 64, height: 64, resizeMode: "contain" },
  badgeTitle: { fontWeight: "900", fontSize: 16, color: "#7E5A00" },
  badgeSub: { fontSize: 12, color: "#7E5A00", fontWeight: "700" },
  sectionTitle: {
    fontSize: 14,
    fontWeight: "900",
    color: "#263238",
    marginTop: 18,
    marginBottom: 8,
  },
  calCard: {
    backgroundColor: "#fff",
    borderRadius: 14,
    padding: 12,
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  calGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 5,
  },
  calCell: {
    width: 28,
    height: 28,
    borderRadius: 6,
    alignItems: "center",
    justifyContent: "center",
  },
  calCaption: {
    marginTop: 10,
    fontSize: 11,
    color: "#90A4AE",
    fontWeight: "700",
    textAlign: "center",
  },
  missing: { padding: 20, fontWeight: "800", color: "#263238" },
  backBtn: {
    margin: 20,
    padding: 14,
    backgroundColor: "#2962FF",
    borderRadius: 12,
    alignItems: "center",
  },
  backTxt: { color: "#fff", fontWeight: "900" },
});
