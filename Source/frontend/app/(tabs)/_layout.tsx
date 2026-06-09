import { Tabs } from "expo-router";
import { Pressable, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";

// Wrap the tab button so testID is applied to a real DOM node on web
// (expo-router's default web button doesn't forward tabBarTestID).
function makeTabButton(testID: string) {
  return function TabButton(props: any) {
    return (
      <Pressable
        {...props}
        testID={testID}
        accessibilityRole="button"
        style={({ pressed }) => [
          props.style,
          { opacity: pressed ? 0.7 : 1 },
        ]}
      >
        <View style={{ flex: 1, alignItems: "center", justifyContent: "center" }}>
          {props.children}
        </View>
      </Pressable>
    );
  };
}

export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: "#2962FF",
        tabBarInactiveTintColor: "#90A4AE",
        tabBarStyle: {
          backgroundColor: "#FFFFFF",
          borderTopWidth: 0,
          height: 64,
          paddingBottom: 10,
          paddingTop: 8,
        },
        tabBarLabelStyle: { fontSize: 11, fontWeight: "800" },
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: "Road",
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="road-variant" size={size} color={color} />
          ),
          tabBarButton: makeTabButton("tab-road"),
        }}
      />
      <Tabs.Screen
        name="stats"
        options={{
          title: "Stats",
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="chart-bar" size={size} color={color} />
          ),
          tabBarButton: makeTabButton("tab-stats"),
        }}
      />
      <Tabs.Screen
        name="settings"
        options={{
          title: "Settings",
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="cog" size={size} color={color} />
          ),
          tabBarButton: makeTabButton("tab-settings"),
        }}
      />
    </Tabs>
  );
}
