import { useState } from "react";
import {
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView, useSafeAreaInsets } from "react-native-safe-area-context";
import { useRouter } from "expo-router";
import { MaterialCommunityIcons } from "@expo/vector-icons";

import { useApp } from "@/src/lib/app-context";
import {
  HABIT_CATEGORIES,
  HABIT_COLORS,
  HABIT_ICONS,
} from "@/src/lib/habits";
import type { HabitCategory } from "@/src/lib/types";

export default function AddHabit() {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { addHabit } = useApp();

  const [name, setName] = useState("");
  const [category, setCategory] = useState<HabitCategory>("health");
  const [color, setColor] = useState(HABIT_COLORS[0]);
  const [icon, setIcon] = useState(HABIT_ICONS[0]);
  const [reminderTime, setReminderTime] = useState<string>("");

  const canSave = name.trim().length > 0;

  const onSave = async () => {
    if (!canSave) return;
    await addHabit({
      name,
      category,
      color,
      icon,
      reminderTime: reminderTime || null,
    });
    router.back();
  };

  return (
    <SafeAreaView style={styles.container} edges={["top", "bottom"]} testID="add-habit-screen">
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : undefined}
        style={{ flex: 1 }}
      >
        <View style={styles.header}>
          <TouchableOpacity
            onPress={() => router.back()}
            style={styles.closeBtn}
            testID="add-habit-close"
          >
            <MaterialCommunityIcons name="close" size={24} color="#263238" />
          </TouchableOpacity>
          <Text style={styles.title}>New Habit</Text>
          <View style={{ width: 40 }} />
        </View>

        <ScrollView
          contentContainerStyle={{ paddingBottom: 16 + insets.bottom }}
          keyboardShouldPersistTaps="handled"
        >
          <Text style={styles.lbl}>Name</Text>
          <TextInput
            testID="add-habit-name"
            value={name}
            onChangeText={setName}
            placeholder="e.g. Drink water"
            placeholderTextColor="#90A4AE"
            style={styles.input}
            maxLength={48}
            returnKeyType="done"
          />

          <Text style={styles.lbl}>Category</Text>
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            contentContainerStyle={styles.chipRow}
          >
            {HABIT_CATEGORIES.map((c) => {
              const sel = category === c.key;
              return (
                <TouchableOpacity
                  key={c.key}
                  testID={`category-chip-${c.key}`}
                  onPress={() => {
                    setCategory(c.key);
                    setIcon(c.icon);
                  }}
                  style={[
                    styles.chip,
                    sel ? styles.chipActive : styles.chipIdle,
                  ]}
                >
                  <MaterialCommunityIcons
                    name={c.icon as any}
                    size={16}
                    color={sel ? "#fff" : "#263238"}
                  />
                  <Text
                    style={[
                      styles.chipTxt,
                      { color: sel ? "#fff" : "#263238" },
                    ]}
                  >
                    {c.label}
                  </Text>
                </TouchableOpacity>
              );
            })}
          </ScrollView>

          <Text style={styles.lbl}>Lane color</Text>
          <View style={styles.colorRow}>
            {HABIT_COLORS.map((c) => (
              <TouchableOpacity
                key={c}
                testID={`color-${c}`}
                onPress={() => setColor(c)}
                style={[
                  styles.colorDot,
                  { backgroundColor: c },
                  color === c && styles.colorDotActive,
                ]}
              />
            ))}
          </View>

          <Text style={styles.lbl}>Icon</Text>
          <View style={styles.iconGrid}>
            {HABIT_ICONS.map((n) => {
              const sel = icon === n;
              return (
                <TouchableOpacity
                  key={n}
                  testID={`icon-${n}`}
                  onPress={() => setIcon(n)}
                  style={[
                    styles.iconCell,
                    sel && { backgroundColor: color, borderBottomColor: "rgba(0,0,0,0.25)" },
                  ]}
                >
                  <MaterialCommunityIcons
                    name={n as any}
                    size={20}
                    color={sel ? "#fff" : "#263238"}
                  />
                </TouchableOpacity>
              );
            })}
          </View>

          <Text style={styles.lbl}>Reminder time (optional)</Text>
          <TextInput
            testID="add-habit-reminder"
            value={reminderTime}
            onChangeText={setReminderTime}
            placeholder="e.g. 08:00"
            placeholderTextColor="#90A4AE"
            style={styles.input}
            maxLength={5}
            keyboardType="numbers-and-punctuation"
          />
          <Text style={styles.hint}>
            We’ll surface a gentle in-app banner around this time.
          </Text>
        </ScrollView>

        <View style={[styles.footer, { paddingBottom: insets.bottom + 12 }]}>
          <TouchableOpacity
            testID="add-habit-save"
            disabled={!canSave}
            onPress={onSave}
            style={[styles.saveBtn, !canSave && styles.saveBtnDisabled]}
          >
            <MaterialCommunityIcons
              name="check-bold"
              size={20}
              color={canSave ? "#fff" : "#B0BEC5"}
            />
            <Text
              style={[
                styles.saveTxt,
                { color: canSave ? "#fff" : "#B0BEC5" },
              ]}
            >
              Save habit
            </Text>
          </TouchableOpacity>
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#F5FBFF" },
  header: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 12,
    paddingTop: 4,
    paddingBottom: 12,
  },
  closeBtn: {
    width: 40,
    height: 40,
    borderRadius: 12,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#FFFFFF",
  },
  title: {
    flex: 1,
    textAlign: "center",
    fontWeight: "900",
    fontSize: 18,
    color: "#263238",
  },
  lbl: {
    fontSize: 11,
    fontWeight: "900",
    color: "#90A4AE",
    letterSpacing: 1,
    paddingHorizontal: 20,
    marginTop: 14,
    marginBottom: 8,
  },
  input: {
    marginHorizontal: 16,
    backgroundColor: "#fff",
    borderRadius: 12,
    paddingHorizontal: 14,
    paddingVertical: 12,
    fontSize: 16,
    color: "#263238",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.06)",
  },
  hint: {
    marginHorizontal: 20,
    marginTop: 6,
    fontSize: 11,
    color: "#90A4AE",
    fontWeight: "600",
  },
  chipRow: {
    paddingHorizontal: 16,
    gap: 8,
    flexDirection: "row",
  },
  chip: {
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
    paddingHorizontal: 14,
    height: 36,
    borderRadius: 18,
    borderBottomWidth: 3,
    flexShrink: 0,
  },
  chipIdle: { backgroundColor: "#fff", borderBottomColor: "rgba(0,0,0,0.08)" },
  chipActive: { backgroundColor: "#2962FF", borderBottomColor: "#0D47A1" },
  chipTxt: { fontWeight: "800", fontSize: 13 },
  colorRow: {
    paddingHorizontal: 16,
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 10,
  },
  colorDot: {
    width: 36,
    height: 36,
    borderRadius: 18,
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.18)",
  },
  colorDotActive: {
    borderWidth: 3,
    borderColor: "#263238",
  },
  iconGrid: {
    paddingHorizontal: 16,
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 8,
  },
  iconCell: {
    width: 44,
    height: 44,
    borderRadius: 12,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#fff",
    borderBottomWidth: 3,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  footer: {
    paddingHorizontal: 16,
    paddingTop: 10,
    backgroundColor: "#F5FBFF",
  },
  saveBtn: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 8,
    backgroundColor: "#2962FF",
    borderRadius: 16,
    paddingVertical: 14,
    borderBottomWidth: 4,
    borderBottomColor: "#0D47A1",
  },
  saveBtnDisabled: {
    backgroundColor: "#ECEFF1",
    borderBottomColor: "#CFD8DC",
  },
  saveTxt: { fontWeight: "900", fontSize: 16 },
});
