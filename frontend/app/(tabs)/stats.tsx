import { useMemo } from "react";
import { Image, ScrollView, StyleSheet, Text, View } from "react-native";
import { SafeAreaView, useSafeAreaInsets } from "react-native-safe-area-context";
import { MaterialCommunityIcons } from "@expo/vector-icons";

import { useApp } from "@/src/lib/app-context";
import { dataUri } from "@/src/lib/assets";
import { addDays, todayKey } from "@/src/lib/date";
import { computeState, hasMonthMaster } from "@/src/lib/habits";

const HEATMAP_DAYS = 84; // 12 weeks

export default function StatsScreen() {
  const insets = useSafeAreaInsets();
  const { habits, assets } = useApp();

  const stats = useMemo(() => {
    const states = habits.map((h) => ({ habit: h, state: computeState(h) }));
    const totalDone = states.reduce((n, s) => n + s.state.totalDone, 0);
    const bestStreak = habits.reduce((m, h) => Math.max(m, h.bestStreak), 0);
    const monthMasters = habits.filter(hasMonthMaster);
    const goldenMiles = habits.filter((h) => h.bestStreak >= 7);
    return { states, totalDone, bestStreak, monthMasters, goldenMiles };
  }, [habits]);

  const heat = useMemo(() => {
    const today = todayKey();
    const days: { key: string; pct: number; done: number }[] = [];
    const totalHabits = Math.max(habits.length, 1);
    for (let i = HEATMAP_DAYS - 1; i >= 0; i--) {
      const k = addDays(today, -i);
      const done = habits.filter((h) => h.completions[k]).length;
      days.push({ key: k, pct: done / totalHabits, done });
    }
    return days;
  }, [habits]);

  const badgeUri = dataUri(assets?.badge_month_master);

  return (
    <SafeAreaView style={styles.container} edges={["top"]} testID="stats-screen">
      <ScrollView contentContainerStyle={{ paddingBottom: insets.bottom + 90 }}>
        <Text style={styles.title}>Stats & Badges</Text>

        <View style={styles.summaryRow}>
          <Stat label="Days crossed" value={`${stats.totalDone}`} icon="walk" color="#2962FF" />
          <Stat label="Best streak" value={`${stats.bestStreak}`} icon="fire" color="#FF6F00" />
          <Stat
            label="Habits"
            value={`${habits.length}`}
            icon="road-variant"
            color="#00897B"
          />
        </View>

        <Text style={styles.sectionTitle}>Activity heatmap</Text>
        <View style={styles.heatmapCard}>
          <View style={styles.heatGrid}>
            {heat.map((d) => (
              <View
                key={d.key}
                testID={`heatmap-${d.key}`}
                style={[
                  styles.heatCell,
                  {
                    backgroundColor: heatColor(d.pct),
                  },
                ]}
              />
            ))}
          </View>
          <View style={styles.heatLegend}>
            <Text style={styles.heatLegendTxt}>less</Text>
            {[0, 0.25, 0.5, 0.75, 1].map((p) => (
              <View
                key={p}
                style={[styles.heatLegendCell, { backgroundColor: heatColor(p) }]}
              />
            ))}
            <Text style={styles.heatLegendTxt}>more</Text>
          </View>
        </View>

        <Text style={styles.sectionTitle}>Month Master badges</Text>
        {stats.monthMasters.length === 0 ? (
          <View style={styles.emptyBadge}>
            <MaterialCommunityIcons name="medal-outline" size={40} color="#90A4AE" />
            <Text style={styles.emptyBadgeTxt}>
              Reach a 30-day streak to earn your first Month Master.
            </Text>
          </View>
        ) : (
          <View style={styles.badgeGrid}>
            {stats.monthMasters.map((h) => (
              <View key={h.id} style={styles.badgeCard} testID={`badge-${h.id}`}>
                {badgeUri ? (
                  <Image source={{ uri: badgeUri }} style={styles.badgeImg} />
                ) : (
                  <MaterialCommunityIcons name="medal" size={56} color="#FFD600" />
                )}
                <Text style={styles.badgeName} numberOfLines={1}>
                  {h.name}
                </Text>
                <Text style={styles.badgeSub}>Best {h.bestStreak} days</Text>
              </View>
            ))}
          </View>
        )}

        <Text style={styles.sectionTitle}>Per-habit records</Text>
        {habits.length === 0 ? (
          <View style={styles.emptyBadge}>
            <Text style={styles.emptyBadgeTxt}>Add a habit to see stats.</Text>
          </View>
        ) : (
          <View style={{ gap: 10, paddingHorizontal: 16 }}>
            {stats.states.map(({ habit, state }) => (
              <View
                key={habit.id}
                style={styles.recordRow}
                testID={`record-${habit.id}`}
              >
                <View
                  style={[styles.recordChip, { backgroundColor: habit.color }]}
                >
                  <MaterialCommunityIcons
                    name={habit.icon as any}
                    size={16}
                    color="#fff"
                  />
                </View>
                <Text style={styles.recordName} numberOfLines={1}>
                  {habit.name}
                </Text>
                <View style={styles.recordMetric}>
                  <MaterialCommunityIcons name="fire" size={14} color="#FF6F00" />
                  <Text style={styles.recordMetricTxt}>{state.currentStreak}</Text>
                </View>
                <View style={styles.recordMetric}>
                  <MaterialCommunityIcons name="trophy" size={14} color="#FFB300" />
                  <Text style={styles.recordMetricTxt}>{habit.bestStreak}</Text>
                </View>
              </View>
            ))}
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

function Stat({
  label,
  value,
  icon,
  color,
}: {
  label: string;
  value: string;
  icon: string;
  color: string;
}) {
  return (
    <View style={styles.statCard}>
      <MaterialCommunityIcons name={icon as any} size={20} color={color} />
      <Text style={styles.statValue}>{value}</Text>
      <Text style={styles.statLabel}>{label}</Text>
    </View>
  );
}

function heatColor(pct: number): string {
  if (pct <= 0) return "#ECEFF1";
  if (pct < 0.34) return "#C5E1A5";
  if (pct < 0.67) return "#9CCC65";
  if (pct < 1) return "#7CB342";
  return "#558B2F";
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#E1F5FE" },
  title: {
    fontSize: 28,
    fontWeight: "900",
    color: "#263238",
    paddingHorizontal: 20,
    paddingTop: 12,
    paddingBottom: 10,
  },
  summaryRow: {
    flexDirection: "row",
    paddingHorizontal: 16,
    gap: 10,
    marginBottom: 8,
  },
  statCard: {
    flex: 1,
    backgroundColor: "#fff",
    borderRadius: 14,
    paddingVertical: 14,
    alignItems: "center",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  statValue: { fontWeight: "900", fontSize: 22, color: "#263238", marginTop: 4 },
  statLabel: { fontSize: 11, color: "#546E7A", fontWeight: "700" },
  sectionTitle: {
    fontSize: 16,
    fontWeight: "900",
    color: "#263238",
    paddingHorizontal: 20,
    marginTop: 18,
    marginBottom: 10,
  },
  heatmapCard: {
    marginHorizontal: 16,
    backgroundColor: "#fff",
    borderRadius: 14,
    padding: 12,
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  heatGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 4,
  },
  heatCell: { width: 18, height: 18, borderRadius: 4 },
  heatLegend: {
    flexDirection: "row",
    alignItems: "center",
    gap: 4,
    marginTop: 10,
    justifyContent: "flex-end",
  },
  heatLegendCell: { width: 12, height: 12, borderRadius: 3 },
  heatLegendTxt: { fontSize: 10, color: "#546E7A", fontWeight: "700" },
  emptyBadge: {
    marginHorizontal: 16,
    backgroundColor: "#fff",
    borderRadius: 14,
    padding: 18,
    alignItems: "center",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
    gap: 6,
  },
  emptyBadgeTxt: {
    fontSize: 13,
    color: "#546E7A",
    fontWeight: "600",
    textAlign: "center",
  },
  badgeGrid: {
    paddingHorizontal: 16,
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 10,
  },
  badgeCard: {
    width: "48%",
    backgroundColor: "#FFFDE7",
    borderRadius: 14,
    padding: 12,
    alignItems: "center",
    borderBottomWidth: 3,
    borderBottomColor: "#FBC02D",
  },
  badgeImg: { width: 80, height: 80 },
  badgeName: {
    fontWeight: "900",
    color: "#263238",
    marginTop: 6,
    fontSize: 14,
  },
  badgeSub: { fontSize: 11, color: "#7E5A00", fontWeight: "700" },
  recordRow: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#fff",
    borderRadius: 12,
    paddingVertical: 10,
    paddingHorizontal: 12,
    gap: 10,
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  recordChip: {
    width: 28,
    height: 28,
    borderRadius: 8,
    alignItems: "center",
    justifyContent: "center",
  },
  recordName: { flex: 1, fontWeight: "800", color: "#263238", fontSize: 14 },
  recordMetric: {
    flexDirection: "row",
    alignItems: "center",
    gap: 4,
    backgroundColor: "#F5F5F5",
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 8,
  },
  recordMetricTxt: { fontWeight: "800", color: "#263238", fontSize: 12 },
});
