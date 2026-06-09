using HabitCross.App;
using HabitCross.Core;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Screens
{
    /// <summary>The Stats tab: totals, activity heatmap, badges, records. Ported from <c>(tabs)/stats.tsx</c>.</summary>
    public class StatsScreen : ScreenBase
    {
        private const int HeatmapDays = 84; // 12 weeks

        protected override bool LiveRefresh => true;

        public StatsScreen(AppController app) : base(app) { }

        protected override void Compose(VisualElement root)
        {
            root.style.backgroundColor = Theme.ScreenBackground;

            var scroll = ScrollViewUtil.Create(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            var c = scroll.contentContainer;
            c.style.paddingBottom = 24;
            root.Add(scroll);

            var title = UIFactory.Heading("Stats & Badges", 28);
            title.style.paddingLeft = 20; title.style.paddingRight = 20;
            title.style.paddingTop = 12; title.style.paddingBottom = 10;
            c.Add(title);

            // ---- Summary ----
            int totalDone = 0, best = 0;
            foreach (var h in App.Habits)
            {
                totalDone += h.CompletionCount;
                if (h.bestStreak > best) best = h.bestStreak;
            }
            var summary = UIFactory.Row();
            summary.style.paddingLeft = 16; summary.style.paddingRight = 16;
            summary.style.marginBottom = 8;
            summary.Add(StatCard("walk", Theme.Action, "Days crossed", $"{totalDone}", 0));
            summary.Add(StatCard("fire", Theme.Hex("#FF6F00"), "Best streak", $"{best}", 1));
            summary.Add(StatCard("road-variant", Theme.Hex("#00897B"), "Habits", $"{App.Habits.Count}", 2));
            c.Add(summary);

            // ---- Heatmap ----
            c.Add(SectionTitle("Activity heatmap"));
            c.Add(BuildHeatmap());

            // ---- Month Master badges ----
            c.Add(SectionTitle("Month Master badges"));
            var masters = new System.Collections.Generic.List<Habit>();
            foreach (var h in App.Habits)
                if (HabitLogic.HasMonthMaster(h)) masters.Add(h);
            if (masters.Count == 0)
            {
                c.Add(EmptyCard("medal-outline", "Reach a 30-day streak to earn your first Month Master."));
            }
            else
            {
                var grid = new VisualElement();
                grid.style.flexDirection = FlexDirection.Row;
                grid.style.flexWrap = Wrap.Wrap;
                grid.style.paddingLeft = 16; grid.style.paddingRight = 16;
                foreach (var h in masters) grid.Add(BadgeCard(h));
                c.Add(grid);
            }

            // ---- Per-habit records ----
            c.Add(SectionTitle("Per-habit records"));
            if (App.Habits.Count == 0)
            {
                c.Add(EmptyCard(null, "Add a habit to see stats."));
            }
            else
            {
                var list = UIFactory.Column();
                list.style.paddingLeft = 16; list.style.paddingRight = 16;
                foreach (var h in App.Habits) list.Add(RecordRow(h));
                c.Add(list);
            }
        }

        private VisualElement StatCard(string icon, Color color, string label, string value, int index)
        {
            var card = UIFactory.Column();
            card.style.flexGrow = 1; card.style.flexBasis = 0;
            card.style.alignItems = Align.Center;
            card.style.backgroundColor = Theme.Surface;
            UIFactory.SetRadius(card, 14f);
            card.style.paddingTop = 14; card.style.paddingBottom = 14;
            card.style.marginLeft = index == 0 ? 0 : 5;
            card.style.marginRight = index == 2 ? 0 : 5;
            UIFactory.SetBottomBorder(card, 3f, Theme.Shadow);
            card.Add(UIFactory.Glyph(icon, 20, color));
            var v = UIFactory.Text(value, 22, true, Theme.TextPrimary);
            v.style.marginTop = 4;
            card.Add(v);
            card.Add(UIFactory.Text(label, 11, true, Theme.TextSecondary));
            return card;
        }

        private Label SectionTitle(string text)
        {
            var l = UIFactory.Text(text, 16, true, Theme.TextPrimary);
            l.style.paddingLeft = 20; l.style.paddingRight = 20;
            l.style.marginTop = 18; l.style.marginBottom = 10;
            return l;
        }

        private VisualElement BuildHeatmap()
        {
            var card = UIFactory.Card(14f, 12f);
            card.style.marginLeft = 16; card.style.marginRight = 16;

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;

            string today = DateUtil.TodayKey();
            int totalHabits = Mathf.Max(App.Habits.Count, 1);
            for (int i = HeatmapDays - 1; i >= 0; i--)
            {
                string k = DateUtil.AddDays(today, -i);
                int done = 0;
                foreach (var h in App.Habits) if (h.IsDone(k)) done++;
                float pct = done / (float)totalHabits;

                var cell = new VisualElement();
                cell.style.width = 18; cell.style.height = 18;
                cell.style.marginRight = 4; cell.style.marginBottom = 4;
                UIFactory.SetRadius(cell, 4f);
                cell.style.backgroundColor = Theme.HeatColor(pct);
                grid.Add(cell);
            }
            card.Add(grid);

            var legend = UIFactory.Row();
            legend.style.justifyContent = Justify.FlexEnd;
            legend.style.marginTop = 10;
            legend.Add(UIFactory.Text("less ", 10, true, Theme.TextSecondary));
            foreach (var p in new[] { 0f, 0.25f, 0.5f, 0.75f, 1f })
            {
                var lc = new VisualElement();
                lc.style.width = 12; lc.style.height = 12;
                lc.style.marginRight = 4;
                UIFactory.SetRadius(lc, 3f);
                lc.style.backgroundColor = Theme.HeatColor(p);
                legend.Add(lc);
            }
            legend.Add(UIFactory.Text(" more", 10, true, Theme.TextSecondary));
            card.Add(legend);
            return card;
        }

        private VisualElement BadgeCard(Habit h)
        {
            var card = UIFactory.Column();
            card.style.width = Length.Percent(48);
            card.style.alignItems = Align.Center;
            card.style.backgroundColor = Theme.Hex("#FFFDE7");
            UIFactory.SetRadius(card, 14f);
            card.style.paddingTop = 12; card.style.paddingBottom = 12;
            card.style.marginRight = 8; card.style.marginBottom = 10;
            UIFactory.SetBottomBorder(card, 3f, Theme.Hex("#FBC02D"));
            card.Add(UIFactory.Glyph("medal", 56, Theme.MilestoneGold));
            var n = UIFactory.Text(h.name, 14, true, Theme.TextPrimary);
            n.style.marginTop = 6;
            n.style.whiteSpace = WhiteSpace.NoWrap;
            n.style.overflow = Overflow.Hidden;
            n.style.textOverflow = TextOverflow.Ellipsis;
            card.Add(n);
            card.Add(UIFactory.Text($"Best {h.bestStreak} days", 11, true, Theme.Hex("#7E5A00")));
            return card;
        }

        private VisualElement RecordRow(Habit h)
        {
            var state = HabitLogic.ComputeState(h);
            var row = UIFactory.Row();
            row.style.backgroundColor = Theme.Surface;
            UIFactory.SetRadius(row, 12f);
            row.style.paddingTop = 10; row.style.paddingBottom = 10;
            row.style.paddingLeft = 12; row.style.paddingRight = 12;
            row.style.marginBottom = 10;
            UIFactory.SetBottomBorder(row, 3f, Theme.Shadow);

            var chip = new VisualElement();
            chip.style.width = 28; chip.style.height = 28;
            UIFactory.SetRadius(chip, 8f);
            chip.style.backgroundColor = Theme.Hex(h.color);
            chip.style.alignItems = Align.Center; chip.style.justifyContent = Justify.Center;
            chip.style.marginRight = 10;
            chip.Add(UIFactory.Glyph(h.icon, 16, Color.white));
            row.Add(chip);

            var n = UIFactory.Text(h.name, 14, true, Theme.TextPrimary);
            n.style.flexGrow = 1; n.style.flexShrink = 1;
            n.style.whiteSpace = WhiteSpace.NoWrap;
            n.style.overflow = Overflow.Hidden;
            n.style.textOverflow = TextOverflow.Ellipsis;
            row.Add(n);

            row.Add(Metric("fire", Theme.Hex("#FF6F00"), $"{state.currentStreak}"));
            row.Add(Metric("trophy", Theme.Hex("#FFB300"), $"{h.bestStreak}"));
            return row;
        }

        private VisualElement Metric(string icon, Color color, string value)
        {
            var m = UIFactory.Row();
            m.style.backgroundColor = Theme.Hex("#F5F5F5");
            UIFactory.SetRadius(m, 8f);
            m.style.paddingLeft = 8; m.style.paddingRight = 8;
            m.style.paddingTop = 4; m.style.paddingBottom = 4;
            m.style.marginLeft = 6;
            m.Add(UIFactory.Glyph(icon, 14, color));
            m.Add(UIFactory.Text($" {value}", 12, true, Theme.TextPrimary));
            return m;
        }

        private VisualElement EmptyCard(string icon, string text)
        {
            var card = UIFactory.Card(14f, 18f);
            card.style.marginLeft = 16; card.style.marginRight = 16;
            card.style.alignItems = Align.Center;
            if (!string.IsNullOrEmpty(icon))
            {
                var g = UIFactory.Glyph(icon, 40, Theme.TextMuted);
                g.style.marginBottom = 6;
                card.Add(g);
            }
            var t = UIFactory.Text(text, 13, false, Theme.TextSecondary);
            t.style.unityTextAlign = TextAnchor.MiddleCenter;
            t.style.unityFontStyleAndWeight = FontStyle.Bold;
            card.Add(t);
            return card;
        }
    }
}
