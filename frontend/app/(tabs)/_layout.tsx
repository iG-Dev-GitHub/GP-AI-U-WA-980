import { Tabs } from "expo-router";
import { MaterialCommunityIcons } from "@expo/vector-icons";

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
          tabBarTestID: "tab-road",
        }}
      />
      <Tabs.Screen
        name="stats"
        options={{
          title: "Stats",
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="chart-bar" size={size} color={color} />
          ),
          tabBarTestID: "tab-stats",
        }}
      />
      <Tabs.Screen
        name="settings"
        options={{
          title: "Settings",
          tabBarIcon: ({ color, size }) => (
            <MaterialCommunityIcons name="cog" size={size} color={color} />
          ),
          tabBarTestID: "tab-settings",
        }}
      />
    </Tabs>
  );
}
