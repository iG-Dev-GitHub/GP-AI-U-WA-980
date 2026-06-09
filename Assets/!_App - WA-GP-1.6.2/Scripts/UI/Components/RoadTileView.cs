using HabitCross.Core;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Components
{
    /// <summary>
    /// A single road tile, ported from <c>RoadTile.tsx</c>. The runner, the
    /// milestone star and the fire ember are drawn as shapes so the game board
    /// reads correctly on every device regardless of which fonts/emoji are
    /// available — matching the design's blocky, voxel-like language.
    /// </summary>
    public static class RoadTileView
    {
        public const float TileSize = 44f;
        public const float TileGap = 6f;

        public static VisualElement Create(Tile tile, string laneColor, bool showCharacter,
            string characterGlyphKey, string fireGlyphKey)
        {
            Color bg = Theme.FutureTile;
            Color border = new Color(0f, 0f, 0f, 0.10f);

            switch (tile.kind)
            {
                case TileKind.Done:
                    bg = Theme.SuccessTile; border = Theme.Hex("#33691E"); break;
                case TileKind.Missed:
                    bg = Theme.MissedTile; border = Theme.DangerDeep; break;
                case TileKind.Today:
                    bg = Theme.Hex(laneColor); border = new Color(0f, 0f, 0f, 0.25f); break;
                case TileKind.Future:
                    bg = Theme.FutureTile; border = new Color(0f, 0f, 0f, 0.10f); break;
            }

            var t = new VisualElement();
            t.AddToClassList("road-tile");
            t.style.width = TileSize;
            t.style.height = TileSize;
            t.style.marginRight = TileGap;
            t.style.backgroundColor = bg;
            UIFactory.SetRadius(t, 10f);
            UIFactory.SetBottomBorder(t, 4f, border);
            t.style.alignItems = Align.Center;
            t.style.justifyContent = Justify.Center;
            t.style.flexShrink = 0;
            t.style.overflow = Overflow.Visible;

            if (tile.isMilestone && tile.kind == TileKind.Done)
            {
                t.Add(Shapes.Diamond(20f, Theme.MilestoneGold, Theme.Hex("#F9A825")));
            }
            else if (tile.kind == TileKind.Missed)
            {
                t.Add(Shapes.Diamond(18f, Theme.Hex("#FF3D00"), Theme.Hex("#BF360C")));
            }
            else if (tile.kind == TileKind.Today && !showCharacter)
            {
                var dot = new VisualElement();
                dot.style.width = 8; dot.style.height = 8;
                UIFactory.SetRadius(dot, 4f);
                dot.style.backgroundColor = new Color(1f, 1f, 1f, 0.85f);
                t.Add(dot);
            }

            if (showCharacter)
            {
                var wrap = new VisualElement();
                wrap.style.position = Position.Absolute;
                wrap.style.top = -TileSize + 6f;
                wrap.style.left = 0; wrap.style.right = 0;
                wrap.style.alignItems = Align.Center;
                wrap.pickingMode = PickingMode.Ignore;
                wrap.Add(Shapes.Runner(30f, Theme.TextPrimary));
                t.Add(wrap);
            }

            return t;
        }
    }
}
