import { storage } from "@/src/utils/storage";

const BACKEND_URL = process.env.EXPO_PUBLIC_BACKEND_URL;
const CACHE_KEY = "assets.pack.v1";

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
  return ALL_KEYS.every((k) => typeof pack[k] === "string" && pack[k]!.length > 100);
}

export function dataUri(b64: string | undefined): string | null {
  if (!b64) return null;
  return `data:image/png;base64,${b64}`;
}

async function loadCached(): Promise<AssetPack | null> {
  const raw = await storage.getItem<string>(CACHE_KEY, "");
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AssetPack;
  } catch {
    return null;
  }
}

async function saveCached(pack: AssetPack): Promise<void> {
  await storage.setItem(CACHE_KEY, JSON.stringify(pack));
}

// Returns the most recent server-side pack (if any), or null.
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

// Public: tries local cache -> server cache -> generation.
// `onProgress` is called with a status string for UX.
export async function ensureAssets(
  onProgress?: (s: string) => void,
): Promise<AssetPack | null> {
  const cached = await loadCached();
  if (isPackComplete(cached)) return cached;

  onProgress?.("Fetching artwork…");
  const server = await fetchServerPack();
  if (isPackComplete(server)) {
    await saveCached(server!);
    return server;
  }

  onProgress?.("Generating artwork (one-time, ~30s)…");
  const generated = await triggerGenerate();
  if (isPackComplete(generated)) {
    await saveCached(generated!);
    return generated;
  }

  // Partial pack -> still save what we have.
  if (generated) await saveCached(generated);
  return generated;
}

export async function getCachedPack(): Promise<AssetPack | null> {
  return loadCached();
}
