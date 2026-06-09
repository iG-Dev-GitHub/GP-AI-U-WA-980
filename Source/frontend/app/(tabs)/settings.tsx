import { useState } from "react";
import {
  ScrollView,
  StyleSheet,
  Switch,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView, useSafeAreaInsets } from "react-native-safe-area-context";
import { MaterialCommunityIcons } from "@expo/vector-icons";

import { useApp } from "@/src/lib/app-context";

export default function SettingsScreen() {
  const insets = useSafeAreaInsets();
  const { habits, resetEverything } = useApp();
  const [reminderOn, setReminderOn] = useState(true);
  const [confirmReset, setConfirmReset] = useState(false);
  const [resetMessage, setResetMessage] = useState<string | null>(null);

  const doReset = async () => {
    await resetEverything();
    setConfirmReset(false);
    setResetMessage("All data cleared. Welcome back!");
    setTimeout(() => setResetMessage(null), 3000);
  };

  return (
    <SafeAreaView style={styles.container} edges={["top"]} testID="settings-screen">
      <ScrollView contentContainerStyle={{ paddingBottom: insets.bottom + 90 }}>
        <Text style={styles.title}>Settings</Text>

        <View style={styles.section}>
          <Text style={styles.sectionLabel}>Reminders</Text>
          <View style={styles.row}>
            <View style={styles.rowLeft}>
              <MaterialCommunityIcons name="bell" size={20} color="#2962FF" />
              <View style={{ flex: 1 }}>
                <Text style={styles.rowTitle}>In-app reminders</Text>
                <Text style={styles.rowSub}>
                  Show a banner on launch when habits aren’t done today.
                </Text>
              </View>
            </View>
            <Switch
              testID="setting-reminder-toggle"
              value={reminderOn}
              onValueChange={setReminderOn}
              trackColor={{ false: "#CFD8DC", true: "#90CAF9" }}
              thumbColor={reminderOn ? "#2962FF" : "#ECEFF1"}
            />
          </View>
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionLabel}>Data</Text>
          <View style={styles.row}>
            <View style={styles.rowLeft}>
              <MaterialCommunityIcons name="database" size={20} color="#00897B" />
              <View style={{ flex: 1 }}>
                <Text style={styles.rowTitle}>Stored locally</Text>
                <Text style={styles.rowSub}>
                  {habits.length} habit{habits.length === 1 ? "" : "s"} · offline-first,
                  no account needed.
                </Text>
              </View>
            </View>
          </View>

          {confirmReset ? (
            <View style={styles.confirmRow}>
              <Text style={styles.confirmTxt}>Delete all habits and history?</Text>
              <View style={{ flexDirection: "row", gap: 8 }}>
                <TouchableOpacity
                  testID="setting-reset-cancel"
                  onPress={() => setConfirmReset(false)}
                  style={[styles.smallBtn, { backgroundColor: "#ECEFF1" }]}
                >
                  <Text style={styles.smallBtnTxt}>Cancel</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  testID="setting-reset-confirm"
                  onPress={doReset}
                  style={[styles.smallBtn, { backgroundColor: "#E53935" }]}
                >
                  <Text style={[styles.smallBtnTxt, { color: "#fff" }]}>
                    Yes, reset
                  </Text>
                </TouchableOpacity>
              </View>
            </View>
          ) : (
            <TouchableOpacity
              testID="setting-reset"
              style={[styles.dangerBtn]}
              onPress={() => setConfirmReset(true)}
            >
              <MaterialCommunityIcons name="trash-can" size={18} color="#fff" />
              <Text style={styles.dangerTxt}>Reset all data</Text>
            </TouchableOpacity>
          )}

          {resetMessage ? (
            <Text style={styles.successMsg} testID="setting-reset-success">
              {resetMessage}
            </Text>
          ) : null}
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionLabel}>About</Text>
          <View style={styles.row}>
            <View style={styles.rowLeft}>
              <MaterialCommunityIcons name="information" size={20} color="#546E7A" />
              <View style={{ flex: 1 }}>
                <Text style={styles.rowTitle}>Habit Cross Daily Streaker</Text>
                <Text style={styles.rowSub}>
                  Offline-first habit tracker with a runner crossing your daily road.
                </Text>
              </View>
            </View>
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#E1F5FE" },
  title: {
    fontSize: 28,
    fontWeight: "900",
    color: "#263238",
    paddingHorizontal: 20,
    paddingTop: 12,
    paddingBottom: 6,
  },
  section: {
    backgroundColor: "#FFFFFF",
    marginHorizontal: 16,
    marginTop: 12,
    borderRadius: 14,
    padding: 14,
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
    gap: 10,
  },
  sectionLabel: {
    fontSize: 11,
    fontWeight: "900",
    color: "#90A4AE",
    letterSpacing: 1,
  },
  row: { flexDirection: "row", alignItems: "center", gap: 10 },
  rowLeft: { flexDirection: "row", alignItems: "center", gap: 10, flex: 1 },
  rowTitle: { fontWeight: "800", fontSize: 15, color: "#263238" },
  rowSub: { fontSize: 12, color: "#546E7A", marginTop: 2, fontWeight: "600" },
  dangerBtn: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 6,
    backgroundColor: "#E53935",
    borderRadius: 12,
    paddingVertical: 12,
    borderBottomWidth: 3,
    borderBottomColor: "#B71C1C",
  },
  dangerTxt: { color: "#fff", fontWeight: "900", fontSize: 14 },
  confirmRow: {
    backgroundColor: "#FFEBEE",
    borderRadius: 12,
    padding: 12,
    gap: 10,
  },
  confirmTxt: { fontWeight: "800", color: "#B71C1C", fontSize: 13 },
  smallBtn: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: 10,
    flex: 1,
    alignItems: "center",
  },
  smallBtnTxt: { fontWeight: "900", fontSize: 13, color: "#263238" },
  successMsg: {
    color: "#1B5E20",
    fontWeight: "800",
    fontSize: 12,
    textAlign: "center",
    marginTop: 4,
  },
});
