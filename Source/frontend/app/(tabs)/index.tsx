import { useMemo } from "react";
import {
  FlatList,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView, useSafeAreaInsets } from "react-native-safe-area-context";
import { useRouter } from "expo-router";
import { MaterialCommunityIcons } from "@expo/vector-icons";

import HabitLane from "@/src/components/HabitLane";
import { useApp } from "@/src/lib/app-context";
import { computeState } from "@/src/lib/habits";
import { todayKey } from "@/src/lib/date";

export default function HomeScreen() {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { habits, toggleHabitToday, assetsLoading, assetsStatus } = useApp();

  const summary = useMemo(() => {
    const states = habits.map((h) => computeState(h));
    const doneToday = states.filter((s) => s.doneToday).length;
    const burning = states.filter(
      (s) => s.status === "fire_warning" || s.status === "burned",
    ).length;
    const bestStreak = habits.reduce(
      (m, h) => Math.max(m, h.bestStreak),
      0,
    );
    return { doneToday, total: habits.length, burning, bestStreak };
  }, [habits]);

  const today = todayKey();
  const niceDate = new Date(
    today.split("-").map(Number)[0],
    today.split("-").map(Number)[1] - 1,
    today.split("-").map(Number)[2],
  ).toLocaleDateString(undefined, {
    weekday: "long",
    month: "long",
    day: "numeric",
  });

  return (
    <SafeAreaView style={styles.container} edges={["top"]} testID="home-screen">
      <View style={styles.header}>
        <View style={{ flex: 1 }}>
          <Text style={styles.dateLbl}>{niceDate}</Text>
          <Text style={styles.title}>Your Road</Text>
        </View>
        <View style={styles.statsPill} testID="header-stats">
          <Text style={styles.statsPillNum}>
            {summary.doneToday}/{summary.total || 0}
          </Text>
          <Text style={styles.statsPillLbl}>today</Text>
        </View>
      </View>

      {habits.length === 0 ? (
        <ScrollView
          contentContainerStyle={[styles.emptyWrap, { paddingBottom: insets.bottom + 120 }]}
        >
          <MaterialCommunityIcons name="road-variant" size={96} color="#90A4AE" />
          <Text style={styles.emptyTitle}>No road yet</Text>
          <Text style={styles.emptyBody}>
            Add your first habit and start running across the daily road.
          </Text>
          {assetsLoading ? (
            <View style={styles.assetsLoadingPill}>
              <MaterialCommunityIcons name="palette" size={14} color="#37474F" />
              <Text style={styles.assetsLoadingTxt}>{assetsStatus}</Text>
            </View>
          ) : null}
        </ScrollView>
      ) : (
        <FlatList
          data={habits}
          keyExtractor={(h) => h.id}
          contentContainerStyle={[
            styles.listContent,
            { paddingBottom: insets.bottom + 120 },
          ]}
          ListHeaderComponent={
            <View style={styles.summaryRow}>
              <SummaryCard
                icon="fire"
                color="#FF6F00"
                label="Best streak"
                value={`${summary.bestStreak} d`}
              />
              <SummaryCard
                icon="alert"
                color="#E65100"
                label="Burning"
                value={`${summary.burning}`}
              />
              {assetsLoading ? (
                <SummaryCard
                  icon="palette"
                  color="#37474F"
                  label="Art"
                  value="…"
                />
              ) : null}
            </View>
          }
          renderItem={({ item }) => (
            <HabitLane
              habit={item}
              onPress={() => router.push(`/habit/${item.id}` as any)}
              onDone={() => toggleHabitToday(item.id)}
            />
          )}
        />
      )}

      <TouchableOpacity
        testID="add-habit-fab"
        style={[styles.fab, { bottom: insets.bottom + 76 }]}
        onPress={() => router.push("/add-habit" as any)}
        accessibilityRole="button"
        accessibilityLabel="Add habit"
      >
        <MaterialCommunityIcons name="plus" size={28} color="#fff" />
      </TouchableOpacity>
    </SafeAreaView>
  );
}

function SummaryCard({
  icon,
  color,
  label,
  value,
}: {
  icon: string;
  color: string;
  label: string;
  value: string;
}) {
  return (
    <View style={styles.summaryCard}>
      <MaterialCommunityIcons name={icon as any} size={20} color={color} />
      <Text style={styles.summaryValue}>{value}</Text>
      <Text style={styles.summaryLabel}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#E1F5FE" },
  header: {
    paddingHorizontal: 20,
    paddingVertical: 14,
    flexDirection: "row",
    alignItems: "center",
  },
  dateLbl: { color: "#546E7A", fontWeight: "700", fontSize: 12, marginBottom: 2 },
  title: { fontSize: 28, fontWeight: "900", color: "#263238" },
  statsPill: {
    paddingHorizontal: 14,
    paddingVertical: 8,
    backgroundColor: "#FFFFFF",
    borderRadius: 14,
    alignItems: "center",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  statsPillNum: { fontWeight: "900", fontSize: 16, color: "#2962FF" },
  statsPillLbl: { fontSize: 10, color: "#546E7A", fontWeight: "700" },
  listContent: { paddingHorizontal: 16, paddingTop: 4 },
  summaryRow: { flexDirection: "row", gap: 10, marginBottom: 14 },
  summaryCard: {
    flex: 1,
    backgroundColor: "#FFFFFF",
    borderRadius: 14,
    paddingVertical: 12,
    paddingHorizontal: 10,
    alignItems: "center",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  summaryValue: {
    fontWeight: "900",
    fontSize: 18,
    color: "#263238",
    marginTop: 4,
  },
  summaryLabel: { fontSize: 11, color: "#546E7A", fontWeight: "700" },
  emptyWrap: {
    flexGrow: 1,
    alignItems: "center",
    justifyContent: "center",
    paddingHorizontal: 32,
    gap: 8,
  },
  emptyTitle: {
    fontSize: 22,
    fontWeight: "900",
    color: "#263238",
    marginTop: 12,
  },
  emptyBody: {
    fontSize: 14,
    color: "#546E7A",
    textAlign: "center",
    lineHeight: 20,
    fontWeight: "600",
  },
  assetsLoadingPill: {
    marginTop: 18,
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
    backgroundColor: "#FFFFFF",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 10,
  },
  assetsLoadingTxt: { fontSize: 11, color: "#37474F", fontWeight: "700" },
  fab: {
    position: "absolute",
    right: 20,
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: "#2962FF",
    alignItems: "center",
    justifyContent: "center",
    borderBottomWidth: 5,
    borderBottomColor: "#0D47A1",
    boxShadow: "0 4px 6px rgba(0,0,0,0.2)",
    elevation: 4,
  },
});
