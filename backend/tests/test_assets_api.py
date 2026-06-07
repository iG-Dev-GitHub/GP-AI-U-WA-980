"""Backend API tests for Habit Cross Daily Streaker.

Covers:
- /api/ root health
- /api/assets/keys -> 8 expected keys
- /api/assets -> ready=true + 8 base64 PNG assets
- /api/assets/generate (force=false) -> cached=true + same pack
"""
import base64
import os

import pytest
import requests

BASE_URL = os.environ.get("EXPO_PUBLIC_BACKEND_URL") or "https://streak-runner-2.preview.emergentagent.com"
BASE_URL = BASE_URL.rstrip("/")

EXPECTED_KEYS = {
    "runner_running",
    "runner_jumping",
    "runner_stopped",
    "runner_victory",
    "fire_small",
    "fire_large",
    "tile_gold",
    "badge_month_master",
}


@pytest.fixture(scope="module")
def s():
    sess = requests.Session()
    sess.headers.update({"Content-Type": "application/json"})
    return sess


# --- root ---
def test_root_health(s):
    r = s.get(f"{BASE_URL}/api/", timeout=30)
    assert r.status_code == 200
    assert "message" in r.json()


# --- /api/assets/keys ---
def test_asset_keys(s):
    r = s.get(f"{BASE_URL}/api/assets/keys", timeout=30)
    assert r.status_code == 200
    body = r.json()
    assert "keys" in body
    assert set(body["keys"]) == EXPECTED_KEYS
    assert len(body["keys"]) == 8


# --- /api/assets ---
def test_get_assets_ready(s):
    r = s.get(f"{BASE_URL}/api/assets", timeout=60)
    assert r.status_code == 200
    body = r.json()
    assert body.get("ready") is True, f"assets not ready: {body}"
    assets = body.get("assets") or {}
    assert set(assets.keys()) == EXPECTED_KEYS
    # Validate that values are non-trivial base64 PNGs
    for k, v in assets.items():
        assert isinstance(v, str) and len(v) > 100, f"{k} too small"
        # Should decode as valid base64
        raw = base64.b64decode(v, validate=False)
        # PNG magic 89 50 4E 47, but Gemini may return other formats; just
        # check it's a real binary > 200 bytes
        assert len(raw) > 200, f"{k} decoded too small: {len(raw)}"


# --- /api/assets/generate (force=false should hit cache) ---
def test_generate_cached(s):
    r = s.post(
        f"{BASE_URL}/api/assets/generate",
        json={"force": False},
        timeout=120,
    )
    assert r.status_code == 200, r.text
    body = r.json()
    assert body.get("ready") is True
    assert body.get("cached") is True, f"expected cached=true, got {body.get('cached')}"
    assert set((body.get("assets") or {}).keys()) == EXPECTED_KEYS


def test_generate_default_body(s):
    """POST without body should still work because `force` defaults to False."""
    r = s.post(f"{BASE_URL}/api/assets/generate", json={}, timeout=120)
    assert r.status_code == 200
    body = r.json()
    assert body.get("ready") is True
    assert body.get("cached") is True
