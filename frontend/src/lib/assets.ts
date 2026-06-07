// Asset loader: tries to fetch the AI-generated asset pack from backend
// (which caches it in MongoDB). The pack is kept in memory only — caching
// 6 MB worth of base64 PNGs in AsyncStorage hits the platform quota on web
// and risks the 6 MB default on Android. Backend round-trip is fast.

const BACKEND_URL = process.env.EXPO_PUBLIC_BACKEND_URL;

export type AssetKey =
  | "runner_running"
  | "runner_jumping"
  | "runner_stopped"
  | "runner_victory"
  | "fire_small"
  | "fire_large"
  | "tile_gold"
  | "badge_month_master";

export type AssetPack = Partial<Record<AssetKey, string>>;

const ALL_KEYS: AssetKey[] = [
  "runner_running",
  "runner_jumping",
  "runner_stopped",
  "runner_victory",
  "fire_small",
  "fire_large",
  "tile_gold",
  "badge_month_master",
];

export function isPackComplete(pack: AssetPack | null | undefined): boolean {
  if (!pack) return false;
  return ALL_KEYS.every(
    (k) => typeof pack[k] === "string" && pack[k]!.length > 100,
  );
}

export function dataUri(b64: string | undefined): string | null {
  if (!b64) return null;
  return `data:image/png;base64,${b64}`;
}

async function fetchServerPack(): Promise<AssetPack | null> {
  if (!BACKEND_URL) return null;
  try {
    const r = await fetch(`${BACKEND_URL}/api/assets`);
    if (!r.ok) return null;
    const j = await r.json();
    if (j?.ready && j?.assets) return j.assets as AssetPack;
    return null;
  } catch {
    return null;
  }
}

async function triggerGenerate(): Promise<AssetPack | null> {
  if (!BACKEND_URL) return null;
  try {
    const r = await fetch(`${BACKEND_URL}/api/assets/generate`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ force: false }),
    });
    if (!r.ok) return null;
    const j = await r.json();
    if (j?.ready && j?.assets) return j.assets as AssetPack;
    return null;
  } catch {
    return null;
  }
}

export async function ensureAssets(
  onProgress?: (s: string) => void,
): Promise<AssetPack | null> {
  onProgress?.("Loading artwork…");
  const server = await fetchServerPack();
  if (isPackComplete(server)) return server;

  onProgress?.("Generating artwork (one-time, ~30s)…");
  const generated = await triggerGenerate();
  if (isPackComplete(generated)) return generated;

  return generated;
}
