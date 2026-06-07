import { useRef, useState } from "react";
import {
  Dimensions,
  Image,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useRouter } from "expo-router";
import { MaterialCommunityIcons } from "@expo/vector-icons";

import { useApp } from "@/src/lib/app-context";
import { dataUri } from "@/src/lib/assets";

const { width } = Dimensions.get("window");

interface Slide {
  title: string;
  body: string;
  bg: string;
  iconAsset?: "runner_running" | "fire_large" | "tile_gold";
  fallbackIcon: string;
  fallbackIconColor: string;
}

const SLIDES: Slide[] = [
  {
    title: "Add a habit — start running",
    body: "Each habit becomes its own road. Your little runner sprints forward every day you check in.",
    bg: "#80D8FF",
    iconAsset: "runner_running",
    fallbackIcon: "run-fast",
    fallbackIconColor: "#FF4081",
  },
  {
    title: "Check in daily — move forward",
    body: "Tap Done Today and watch your road turn green. Every 7th day becomes a golden milestone tile.",
    bg: "#B9F6CA",
    iconAsset: "tile_gold",
    fallbackIcon: "star-four-points",
    fallbackIconColor: "#FFD600",
  },
  {
    title: "Miss days — fire blocks your path",
    body: "Skip a day and a flame appears ahead. Miss 3+ days and the streak burns. Get back, put out the fire.",
    bg: "#FFCCBC",
    iconAsset: "fire_large",
    fallbackIcon: "fire",
    fallbackIconColor: "#FF3D00",
  },
];

export default function Onboarding() {
  const router = useRouter();
  const { finishOnboarding, assets } = useApp();
  const scrollRef = useRef<ScrollView>(null);
  const [page, setPage] = useState(0);

  const onMomentumEnd = (e: { nativeEvent: { contentOffset: { x: number } } }) => {
    const x = e.nativeEvent.contentOffset.x;
    setPage(Math.round(x / width));
  };

  const goNext = async () => {
    if (page < SLIDES.length - 1) {
      scrollRef.current?.scrollTo({ x: (page + 1) * width, animated: true });
      setPage(page + 1);
    } else {
      await finishOnboarding();
      router.replace("/(tabs)");
    }
  };

  return (
    <SafeAreaView style={styles.container} edges={["top", "bottom"]}>
      <ScrollView
        ref={scrollRef}
        horizontal
        pagingEnabled
        showsHorizontalScrollIndicator={false}
        onMomentumScrollEnd={onMomentumEnd}
        testID="onboarding-scroll"
      >
        {SLIDES.map((s, i) => {
          const imgUri = s.iconAsset ? dataUri(assets?.[s.iconAsset]) : null;
          return (
            <View
              key={i}
              style={[styles.slide, { width, backgroundColor: s.bg }]}
              testID={`onboarding-slide-${i}`}
            >
              <View style={styles.illustrationWrap}>
                {imgUri ? (
                  <Image source={{ uri: imgUri }} style={styles.illustration} />
                ) : (
                  <MaterialCommunityIcons
                    name={s.fallbackIcon as any}
                    size={120}
                    color={s.fallbackIconColor}
                  />
                )}
              </View>
              <Text style={styles.title}>{s.title}</Text>
              <Text style={styles.body}>{s.body}</Text>
            </View>
          );
        })}
      </ScrollView>

      <View style={styles.dotsRow} testID="onboarding-dots">
        {SLIDES.map((_, i) => (
          <View
            key={i}
            style={[styles.dot, i === page ? styles.dotActive : styles.dotIdle]}
          />
        ))}
      </View>

      <View style={styles.footer}>
        <TouchableOpacity
          testID="onboarding-skip"
          onPress={async () => {
            await finishOnboarding();
            router.replace("/(tabs)");
          }}
          style={styles.skipBtn}
        >
          <Text style={styles.skipTxt}>Skip</Text>
        </TouchableOpacity>
        <TouchableOpacity
          testID="onboarding-next"
          onPress={goNext}
          style={styles.cta}
        >
          <Text style={styles.ctaTxt}>
            {page === SLIDES.length - 1 ? "Start Running" : "Next"}
          </Text>
          <MaterialCommunityIcons
            name={page === SLIDES.length - 1 ? "play-circle" : "arrow-right-bold"}
            size={22}
            color="#fff"
          />
        </TouchableOpacity>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#80D8FF" },
  slide: {
    flex: 1,
    paddingHorizontal: 28,
    paddingVertical: 40,
    alignItems: "center",
    justifyContent: "center",
  },
  illustrationWrap: {
    width: 220,
    height: 220,
    borderRadius: 110,
    backgroundColor: "rgba(255,255,255,0.55)",
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 32,
    borderBottomWidth: 6,
    borderBottomColor: "rgba(0,0,0,0.08)",
  },
  illustration: { width: 180, height: 180, resizeMode: "contain" },
  title: {
    fontSize: 26,
    fontWeight: "900",
    color: "#263238",
    textAlign: "center",
    marginBottom: 14,
  },
  body: {
    fontSize: 15,
    color: "#37474F",
    textAlign: "center",
    lineHeight: 22,
    fontWeight: "600",
    paddingHorizontal: 8,
  },
  dotsRow: {
    flexDirection: "row",
    justifyContent: "center",
    paddingVertical: 16,
    gap: 8,
  },
  dot: { width: 10, height: 10, borderRadius: 5 },
  dotIdle: { backgroundColor: "rgba(0,0,0,0.18)" },
  dotActive: { backgroundColor: "#263238", width: 24 },
  footer: {
    flexDirection: "row",
    paddingHorizontal: 24,
    paddingBottom: 16,
    paddingTop: 8,
    gap: 12,
    alignItems: "center",
  },
  skipBtn: { paddingHorizontal: 16, paddingVertical: 14 },
  skipTxt: { color: "#37474F", fontWeight: "800", fontSize: 15 },
  cta: {
    flex: 1,
    backgroundColor: "#2962FF",
    borderRadius: 16,
    paddingVertical: 14,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 8,
    borderBottomWidth: 4,
    borderBottomColor: "#0D47A1",
  },
  ctaTxt: { color: "#fff", fontWeight: "900", fontSize: 16 },
});
