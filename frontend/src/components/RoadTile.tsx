import { Image, StyleSheet, Text, View } from "react-native";
import { MaterialCommunityIcons } from "@expo/vector-icons";

import { dataUri, type AssetPack } from "@/src/lib/assets";

import type { Tile } from "@/src/lib/habits";

export const TILE_SIZE = 44;
export const TILE_GAP = 6;

interface Props {
  tile: Tile;
  laneColor: string;
  assets: AssetPack | null;
  showCharacter: boolean;
  characterAsset?: keyof AssetPack;
  fireAsset?: "fire_small" | "fire_large";
}

export default function RoadTile({
  tile,
  laneColor,
  assets,
  showCharacter,
  characterAsset = "runner_running",
  fireAsset = "fire_small",
}: Props) {
  let bg = "#ECEFF1";
  let borderBottom = "rgba(0,0,0,0.10)";
  if (tile.kind === "done") {
    bg = "#64DD17";
    borderBottom = "#33691E";
  } else if (tile.kind === "missed") {
    bg = "#FFCDD2";
    borderBottom = "#B71C1C";
  } else if (tile.kind === "today") {
    bg = laneColor;
    borderBottom = "rgba(0,0,0,0.25)";
  } else if (tile.kind === "future") {
    bg = "#ECEFF1";
    borderBottom = "rgba(0,0,0,0.10)";
  }

  const goldUri = dataUri(assets?.tile_gold);
  const fireUri = dataUri(assets?.[fireAsset]);
  const charUri = dataUri(assets?.[characterAsset]);

  return (
    <View
      testID={`road-tile-${tile.dateKey}`}
      style={[styles.tile, { backgroundColor: bg, borderBottomColor: borderBottom }]}
    >
      {tile.isMilestone && tile.kind === "done" ? (
        goldUri ? (
          <Image source={{ uri: goldUri }} style={styles.tileImg} />
        ) : (
          <MaterialCommunityIcons name="star" size={26} color="#FFD600" />
        )
      ) : null}

      {tile.kind === "missed" ? (
        fireUri ? (
          <Image source={{ uri: fireUri }} style={styles.tileImg} />
        ) : (
          <MaterialCommunityIcons name="fire" size={24} color="#FF3D00" />
        )
      ) : null}

      {showCharacter ? (
        <View pointerEvents="none" style={styles.charWrap}>
          {charUri ? (
            <Image source={{ uri: charUri }} style={styles.charImg} />
          ) : (
            <MaterialCommunityIcons
              name="run-fast"
              size={28}
              color="#263238"
            />
          )}
        </View>
      ) : null}

      {tile.kind === "today" && !showCharacter ? (
        <Text style={styles.todayMark}>·</Text>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  tile: {
    width: TILE_SIZE,
    height: TILE_SIZE,
    marginRight: TILE_GAP,
    borderRadius: 10,
    borderBottomWidth: 4,
    alignItems: "center",
    justifyContent: "center",
    position: "relative",
  },
  tileImg: {
    width: TILE_SIZE - 10,
    height: TILE_SIZE - 10,
    resizeMode: "contain",
  },
  charWrap: {
    position: "absolute",
    top: -TILE_SIZE + 4,
    left: -4,
    width: TILE_SIZE + 8,
    height: TILE_SIZE,
    alignItems: "center",
    justifyContent: "flex-end",
  },
  charImg: {
    width: TILE_SIZE + 4,
    height: TILE_SIZE + 4,
    resizeMode: "contain",
  },
  todayMark: {
    color: "rgba(255,255,255,0.85)",
    fontSize: 28,
    fontWeight: "900",
    lineHeight: 28,
  },
});
