import { useEffect } from "react";
import { ActivityIndicator, StyleSheet, View } from "react-native";
import { useRouter } from "expo-router";

import { useApp } from "@/src/lib/app-context";

export default function Index() {
  const router = useRouter();
  const { onboardingDone } = useApp();

  useEffect(() => {
    if (onboardingDone === null) return;
    const target = onboardingDone ? "/(tabs)" : "/onboarding";
    // small defer so the navigator is fully mounted
    const t = setTimeout(() => router.replace(target as any), 0);
    return () => clearTimeout(t);
  }, [onboardingDone, router]);

  return (
    <View style={styles.container} testID="boot-screen">
      <ActivityIndicator color="#2962FF" size="large" />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#E1F5FE",
    alignItems: "center",
    justifyContent: "center",
  },
});
