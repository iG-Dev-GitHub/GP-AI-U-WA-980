using System;
using HabitCross.Core;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Components
{
    /// <summary>
    /// A full habit lane card: header (icon, name, streak, Done button), the
    /// horizontal road strip with the floating runner, and a status banner.
    /// Ported from <c>HabitLane.tsx</c>.
    /// </summary>
    public static class HabitLaneView
    {
        private const int Past = 5;
        private const int Future = 6;

        public static VisualElement Create(Habit habit, Action onPress, Action onDone)
        {
            var state = HabitLogic.ComputeState(habit);
            var tiles = HabitLogic.BuildTiles(habit, Past, Future);
            int characterIdx = HabitLogic.CharacterIndex(tiles, state, Past);

            string characterGlyph = "run";
            string fireGlyph = "fire";

            var card = UIFactory.Card(18f, 14f);
            card.AddToClassList("habit-card");
            if (onPress != null) UIFactory.WirePress(card, onPress, 4f, Theme.Shadow);

            // ---- Header ----
            var header = UIFactory.Row();
            header.style.alignItems = Align.Center;

            var chip = new VisualElement();
            chip.style.width = 40; chip.style.height = 40;
            chip.style.backgroundColor = Theme.Hex(habit.color);
            UIFactory.SetRadius(chip, 12f);
            UIFactory.SetBottomBorder(chip, 3f, new Color(0, 0, 0, 0.18f));
            chip.style.alignItems = Align.Center;
            chip.style.justifyContent = Justify.Center;
            chip.style.marginRight = 12;
            chip.Add(UIFactory.Glyph(habit.icon, 22, Color.white));
            header.Add(chip);

            var nameCol = UIFactory.Column();
            nameCol.style.flexGrow = 1;
            var name = UIFactory.Text(habit.name, 17, true, Theme.TextPrimary);
            name.style.whiteSpace = WhiteSpace.NoWrap;
            name.style.overflow = Overflow.Hidden;
            name.style.textOverflow = TextOverflow.Ellipsis;
            nameCol.Add(name);

            var meta = UIFactory.Row();
            meta.style.marginTop = 2;
            meta.Add(UIFactory.Glyph("fire", 14, Theme.Hex("#FF6F00")));
            var streak = UIFactory.Text($" {state.currentStreak} day streak", 12, true, Theme.TextSecondary);
            meta.Add(streak);
            if (habit.bestStreak > 0)
            {
                var dot = UIFactory.Text("  ·  ", 12, true, Theme.TextMuted);
                meta.Add(dot);
                meta.Add(UIFactory.Glyph("trophy", 14, Theme.Hex("#FFB300")));
                meta.Add(UIFactory.Text($" Best {habit.bestStreak}", 12, true, Theme.TextSecondary));
            }
            nameCol.Add(meta);
            header.Add(nameCol);

            // ---- Done button ----
            bool done = state.doneToday;
            var doneBtn = UIFactory.Row();
            doneBtn.AddToClassList("done-button");
            doneBtn.style.justifyContent = Justify.Center;
            doneBtn.style.paddingLeft = 12; doneBtn.style.paddingRight = 12;
            doneBtn.style.paddingTop = 10; doneBtn.style.paddingBottom = 10;
            UIFactory.SetRadius(doneBtn, 12f);
            UIFactory.SetBottomBorder(doneBtn, 3f, Theme.Hex("#1B5E20"));
            doneBtn.style.backgroundColor = done ? Theme.Hex("#2E7D32") : Theme.Hex("#C8E6C9");
            Color doneText = done ? Color.white : Theme.Hex("#1B5E20");
            doneBtn.Add(UIFactory.Glyph(done ? "check-circle" : "check-bold", 18, doneText));
            var doneLbl = UIFactory.Text(done ? " Done" : " Done Today", 13, true, doneText);
            doneBtn.Add(doneLbl);
            UIFactory.WirePress(doneBtn, onDone, 3f, Theme.Hex("#1B5E20"));
            UIFactory.BlockPropagation(doneBtn);
            header.Add(doneBtn);

            card.Add(header);

            // ---- Lane ----
            var laneWrap = new VisualElement();
            laneWrap.style.marginTop = 14;
            var lane = new VisualElement();
            lane.AddToClassList("lane");
            lane.style.backgroundColor = Theme.ShadeLane(habit.color);
            UIFactory.SetRadius(lane, 14f);
            lane.style.paddingTop = 10; lane.style.paddingBottom = 14;
            lane.style.paddingLeft = 10; lane.style.paddingRight = 10;
            lane.style.overflow = Overflow.Hidden;

            var scroll = ScrollViewUtil.Create(ScrollViewMode.Horizontal);
            scroll.style.height = RoadTileView.TileSize * 2f + 8f; // tile + room for the runner
            scroll.style.flexShrink = 0;
            var row = scroll.contentContainer;
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = RoadTileView.TileSize; // room for the floating runner
            row.style.paddingRight = RoadTileView.TileGap;

            for (int i = 0; i < tiles.Count; i++)
            {
                row.Add(RoadTileView.Create(tiles[i], habit.color,
                    i == characterIdx, characterGlyph, fireGlyph));
            }
            // Block the card's open-detail press while dragging the road.
            UIFactory.BlockPropagation(scroll);
            lane.Add(scroll);
            laneWrap.Add(lane);
            card.Add(laneWrap);

            // ---- Banner ----
            VisualElement banner = BuildBanner(state);
            if (banner != null) card.Add(banner);

            return card;
        }

        private static VisualElement BuildBanner(HabitState state)
        {
            string text; Color bg; Color fg; string icon;
            if (state.status == HabitStatus.Burned)
            {
                bg = Theme.Hex("#FFEBEE"); fg = Theme.Hex("#B71C1C"); icon = "fire";
                text = " Streak burned · check in today to start again";
            }
            else if (state.status == HabitStatus.FireWarning)
            {
                bg = Theme.Hex("#FFF3E0"); fg = Theme.Hex("#E65100"); icon = "alert";
                int d = Mathf.Max(1, state.missedDays);
                text = $" Fire ahead · {d} day{(d == 1 ? "" : "s")} missed, get back on track";
            }
            else if (state.currentStreak >= 7)
            {
                bg = Theme.Hex("#FFF8E1"); fg = Theme.Hex("#F57F17"); icon = "star";
                text = $" Golden Mile · {state.currentStreak} day streak";
            }
            else
            {
                return null;
            }

            var banner = UIFactory.Row();
            banner.AddToClassList("banner");
            banner.style.marginTop = 10;
            banner.style.paddingLeft = 10; banner.style.paddingRight = 10;
            banner.style.paddingTop = 8; banner.style.paddingBottom = 8;
            UIFactory.SetRadius(banner, 10f);
            banner.style.backgroundColor = bg;
            banner.Add(UIFactory.Glyph(icon, 16, fg));
            var lbl = UIFactory.Text(text, 12, true, fg);
            lbl.style.flexShrink = 1;
            banner.Add(lbl);
            return banner;
        }
    }
}
