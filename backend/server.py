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
import uuid
from pathlib import Path
from datetime import datetime, timezone

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
# subject on transparent background, sharp clean edges, NO text.
ASSET_PROMPTS = {
    "runner_running": (
        "A cute neutral cartoon runner character, flat 2D vector style, "
        "vibrant Crossy Road / Chicken Road art direction, mid-stride running "
        "pose facing right, big friendly eyes, simple rounded body, bold "
        "outline, bright cheerful colors, completely isolated on a fully "
        "transparent background, no shadow, no ground, no text, no UI, "
        "centered, square composition."
    ),
    "runner_jumping": (
        "A cute neutral cartoon runner character, flat 2D vector style, "
        "vibrant Crossy Road art direction, joyful mid-air jump pose facing "
        "right with arms up celebrating, big friendly eyes, bright cheerful "
        "colors, isolated on a fully transparent background, no shadow, no "
        "text, centered, square composition."
    ),
    "runner_stopped": (
        "A cute neutral cartoon runner character, flat 2D vector style, "
        "vibrant Crossy Road art direction, standing still facing right with "
        "a worried concerned expression, sweat drop, arms slightly raised in "
        "front, bright colors, isolated on a fully transparent background, "
        "no shadow, no text, centered, square composition."
    ),
    "runner_victory": (
        "A cute neutral cartoon runner character, flat 2D vector style, "
        "vibrant Crossy Road art direction, heroic victory pose with one arm "
        "raised holding a tiny gold star, sparkles around it, big smile, "
        "bright cheerful colors, isolated on a fully transparent background, "
        "no shadow, no text, centered, square composition."
    ),
    "fire_small": (
        "A small cute cartoon flame, single bright orange and yellow fire, "
        "flat 2D vector style, vibrant Crossy Road art direction, bold "
        "rounded outline, friendly not scary, isolated on a fully transparent "
        "background, no shadow, no ground, no text, centered, square."
    ),
    "fire_large": (
        "A large fierce cartoon bonfire, multiple bright orange red and yellow "
        "flames, flat 2D vector style, vibrant Crossy Road art direction, bold "
        "outline, dramatic but cute, isolated on a fully transparent "
        "background, no shadow, no ground, no text, centered, square."
    ),
    "tile_gold": (
        "A shiny gold milestone tile, square block with a gold star symbol on "
        "top, flat 2D vector style, vibrant Crossy Road art direction, glowing "
        "sparkles, bold outline, isolated on a fully transparent background, "
        "no shadow on the ground, no text, centered, square."
    ),
    "badge_month_master": (
        "A premium gold achievement badge, round medal with a number 30 in the "
        "center and a star, ribbon below, flat 2D vector style, vibrant cartoon "
        "art direction like Crossy Road, sparkles, bold outline, isolated on a "
        "fully transparent background, no shadow, centered, square."
    ),
}


class AssetPack(BaseModel):
    id: str
    created_at: str
    assets: dict  # key -> base64 png


class AssetGenerateRequest(BaseModel):
    force: bool = False


async def _generate_one(key: str, prompt: str) -> str:
    """Call Gemini Nano Banana and return base64 PNG (no data: prefix)."""
    chat = LlmChat(
        api_key=EMERGENT_LLM_KEY,
        session_id=f"habit-asset-{key}-{uuid.uuid4()}",
        system_message=(
            "You generate single-subject cartoon illustrations on transparent "
            "backgrounds for a mobile game. Always honor the requested style."
        ),
    )
    chat.with_model("gemini", GEMINI_IMAGE_MODEL).with_params(
        modalities=["image", "text"]
    )
    msg = UserMessage(text=prompt)
    _text, images = await chat.send_message_multimodal_response(msg)
    if not images:
        raise RuntimeError(f"no image returned for {key}")
    return images[0]["data"]  # already base64-encoded string


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
