"""Habit Cross Daily Streaker backend.

Primary role: pre-generate (and cache) the cartoon PNG assets used by the
Expo frontend via Gemini Nano Banana. The app itself is offline-first; once
the client has fetched the asset pack it never needs the backend again.
"""

from fastapi import FastAPI, APIRouter, HTTPException
from dotenv import load_dotenv
from starlette.middleware.cors import CORSMiddleware
from motor.motor_asyncio import AsyncIOMotorClient
from pydantic import BaseModel
import os
import asyncio
import logging
import base64
import io
import uuid
from pathlib import Path
from datetime import datetime, timezone

import numpy as np
from PIL import Image

from emergentintegrations.llm.chat import LlmChat, UserMessage


ROOT_DIR = Path(__file__).parent
load_dotenv(ROOT_DIR / ".env")

mongo_url = os.environ["MONGO_URL"]
client = AsyncIOMotorClient(mongo_url)
db = client[os.environ["DB_NAME"]]

EMERGENT_LLM_KEY = os.environ.get("EMERGENT_LLM_KEY", "")
GEMINI_IMAGE_MODEL = "gemini-3.1-flash-image-preview"

app = FastAPI()
api_router = APIRouter(prefix="/api")

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
)
logger = logging.getLogger(__name__)


# Asset keys + prompts. Style: vibrant Crossy-Road-like flat cartoon, isolated
# subject on a SOLID MAGENTA chroma-key background — we strip it server-side
# to a true alpha PNG. Gemini does NOT produce real transparency reliably and
# tends to bake in a checkerboard pattern, so chroma-key is the workaround.
CHROMA_HEX = "#FF00FF"  # magenta — virtually absent from our subjects
STYLE = (
    "flat 2D vector cartoon, vibrant Crossy Road / Chicken Road style, bold "
    "rounded outline, bright cheerful saturated colors, sticker-like, centered "
    "subject with small padding around it, no shadows, no ground, no text, no "
    "letters, no numbers, single clean subject, square composition, solid "
    f"plain pure magenta {CHROMA_HEX} background filling every pixel that is "
    "not part of the subject — absolutely no checkerboard pattern, no grid, no "
    "transparency indicator, no gradient, no texture in the background."
)

ASSET_PROMPTS = {
    "runner_running": (
        f"A cute neutral cartoon runner character mid-stride facing right, "
        f"big friendly eyes, simple rounded body. {STYLE}"
    ),
    "runner_jumping": (
        f"A cute neutral cartoon runner character in a joyful mid-air jump "
        f"facing right with arms up celebrating, big friendly eyes. {STYLE}"
    ),
    "runner_stopped": (
        f"A cute neutral cartoon runner character standing still facing right "
        f"with a worried concerned expression, small sweat drop, arms slightly "
        f"raised. {STYLE}"
    ),
    "runner_victory": (
        f"A cute neutral cartoon runner character in a heroic victory pose "
        f"with one arm raised holding a tiny gold star, sparkles around it, big "
        f"smile. {STYLE}"
    ),
    "fire_small": (
        f"A small cute single cartoon flame, bright orange and yellow fire, "
        f"friendly not scary. {STYLE}"
    ),
    "fire_large": (
        f"A large fierce cartoon bonfire with multiple bright orange red and "
        f"yellow flames, dramatic but cute. {STYLE}"
    ),
    "tile_gold": (
        f"A shiny gold milestone tile, square block with a gold star symbol on "
        f"top, glowing sparkles. {STYLE}"
    ),
    "badge_month_master": (
        f"A premium gold achievement medal, round, with a star in the center, "
        f"ribbon below, sparkles. {STYLE}"
    ),
}


def _chroma_to_alpha(b64_png: str, hex_color: str = CHROMA_HEX) -> str:
    """Convert a solid chroma-key magenta background to true alpha transparency.

    Uses numpy vectorized pixel ops. Tolerant enough to also catch antialiased
    edges. Returns a base64-encoded PNG (no data: prefix) with alpha.
    """
    raw = base64.b64decode(b64_png)
    im = Image.open(io.BytesIO(raw)).convert("RGBA")
    arr = np.array(im)  # (H, W, 4)
    r, g, b = arr[..., 0].astype(np.int16), arr[..., 1].astype(np.int16), arr[..., 2].astype(np.int16)
    # Magenta-ish: red and blue dominant, green much lower.
    mask = (r > 150) & (b > 150) & (g < 130) & ((r - g) > 60) & ((b - g) > 60)
    # Soft edge: where pixel is partly magenta, partially fade alpha.
    soft = (r > 120) & (b > 120) & (g < 160) & ((r - g) > 30) & ((b - g) > 30)
    edge_only = soft & ~mask
    arr[mask] = [0, 0, 0, 0]
    if edge_only.any():
        # Halve alpha and desaturate the magenta tint a touch.
        edge_alpha = arr[..., 3].astype(np.int16)
        edge_alpha[edge_only] = (edge_alpha[edge_only] // 2).astype(np.int16)
        arr[..., 3] = edge_alpha.astype(np.uint8)
    out = Image.fromarray(arr, mode="RGBA")
    buf = io.BytesIO()
    out.save(buf, format="PNG", optimize=True)
    return base64.b64encode(buf.getvalue()).decode("ascii")


class AssetPack(BaseModel):
    id: str
    created_at: str
    assets: dict  # key -> base64 png


class AssetGenerateRequest(BaseModel):
    force: bool = False


async def _generate_one(key: str, prompt: str) -> str:
    """Call Gemini Nano Banana and return base64 PNG (no data: prefix) with a
    real alpha channel — we strip the solid magenta chroma-key background."""
    chat = LlmChat(
        api_key=EMERGENT_LLM_KEY,
        session_id=f"habit-asset-{key}-{uuid.uuid4()}",
        system_message=(
            "You generate single-subject cartoon illustrations for a mobile "
            "game. The background MUST always be a completely uniform solid "
            f"magenta {CHROMA_HEX} color — never a checkerboard, never a "
            "gradient, never any pattern. The subject must never overlap or "
            "use any pink or magenta tones."
        ),
    )
    chat.with_model("gemini", GEMINI_IMAGE_MODEL).with_params(
        modalities=["image", "text"]
    )
    msg = UserMessage(text=prompt)
    _text, images = await chat.send_message_multimodal_response(msg)
    if not images:
        raise RuntimeError(f"no image returned for {key}")
    raw_b64 = images[0]["data"]
    return _chroma_to_alpha(raw_b64)


@api_router.get("/")
async def root():
    return {"message": "Habit Cross Daily Streaker API"}


@api_router.get("/assets")
async def get_assets():
    """Return the most recent cached asset pack, if any."""
    doc = await db.asset_packs.find_one(
        {}, sort=[("created_at", -1)], projection={"_id": 0}
    )
    if not doc:
        return {"ready": False, "assets": {}}
    return {"ready": True, **doc}


@api_router.post("/assets/generate")
async def generate_assets(body: AssetGenerateRequest):
    """Generate the full asset pack with Gemini Nano Banana.

    If a pack exists and force is False, return it. Otherwise generate every
    asset sequentially (to stay within provider rate limits) and persist.
    """
    if not EMERGENT_LLM_KEY:
        raise HTTPException(500, "EMERGENT_LLM_KEY not configured")

    if not body.force:
        existing = await db.asset_packs.find_one(
            {}, sort=[("created_at", -1)], projection={"_id": 0}
        )
        if existing and set(existing.get("assets", {}).keys()) >= set(
            ASSET_PROMPTS.keys()
        ):
            return {"ready": True, "cached": True, **existing}

    assets: dict = {}
    for key, prompt in ASSET_PROMPTS.items():
        try:
            logger.info("Generating asset %s", key)
            assets[key] = await _generate_one(key, prompt)
        except Exception as e:  # noqa: BLE001 - surface gen failures
            logger.exception("Failed to generate %s", key)
            raise HTTPException(502, f"asset generation failed for {key}: {e}")
        # tiny pause to be nice to the API
        await asyncio.sleep(0.2)

    pack = {
        "id": str(uuid.uuid4()),
        "created_at": datetime.now(timezone.utc).isoformat(),
        "assets": assets,
    }
    await db.asset_packs.insert_one(pack.copy())
    return {"ready": True, "cached": False, **pack}


@api_router.get("/assets/keys")
async def get_asset_keys():
    return {"keys": list(ASSET_PROMPTS.keys())}


app.include_router(api_router)

app.add_middleware(
    CORSMiddleware,
    allow_credentials=True,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.on_event("shutdown")
async def shutdown_db_client():
    client.close()
