using HabitCross.App;
using HabitCross.Core;
using HabitCross.UI.Components;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Screens
{
    /// <summary>The Road tab: habit lanes, summary, and the add-habit FAB. Ported from <c>(tabs)/index.tsx</c>.</summary>
    public class HomeScreen : ScreenBase
    {
        protected override bool LiveRefresh => true;

        public HomeScreen(AppController app) : base(app) { }

        protected override void Compose(VisualElement root)
        {
            root.style.backgroundColor = Theme.ScreenBackground;

            int doneToday = 0, burning = 0, bestStreak = 0;
            foreach (var h in App.Habits)
            {
                var s = HabitLogic.ComputeState(h);
                if (s.doneToday) doneToday++;
                if (s.status == HabitStatus.FireWarning || s.status == HabitStatus.Burned) burning++;
                if (h.bestStreak > bestStreak) bestStreak = h.bestStreak;
            }
            int total = App.Habits.Count;

            // ---- Header ----
            var header = UIFactory.Row();
            header.style.paddingLeft = 20; header.style.paddingRight = 20;
            header.style.paddingTop = 14; header.style.paddingBottom = 14;

            var titleCol = UIFactory.Column();
            titleCol.style.flexGrow = 1;
            titleCol.Add(UIFactory.Text(DateUtil.FormatFullDate(DateUtil.TodayKey()), 12, true, Theme.TextSecondary));
            titleCol.Add(UIFactory.Heading("Your Road", 28));
            header.Add(titleCol);

            var pill = UIFactory.Column();
            pill.style.alignItems = Align.Center;
            pill.style.backgroundColor = Theme.Surface;
            pill.style.paddingLeft = 14; pill.style.paddingRight = 14;
            pill.style.paddingTop = 8; pill.style.paddingBottom = 8;
            UIFactory.SetRadius(pill, 14f);
            UIFactory.SetBottomBorder(pill, 3f, Theme.Shadow);
            pill.Add(UIFactory.Text($"{doneToday}/{total}", 16, true, Theme.Action));
            pill.Add(UIFactory.Text("today", 10, true, Theme.TextSecondary));
            header.Add(pill);
            root.Add(header);

            // ---- Content ----
            if (total == 0)
            {
                root.Add(BuildEmpty());
            }
            else
            {
                var scroll = ScrollViewUtil.Create(ScrollViewMode.Vertical);
                scroll.style.flexGrow = 1;
                var c = scroll.contentContainer;
                c.style.paddingLeft = 16; c.style.paddingRight = 16;
                c.style.paddingTop = 4; c.style.paddingBottom = 24;

                var summary = UIFactory.Row();
                summary.style.marginBottom = 14;
                summary.Add(SummaryCard("fire", Theme.Hex("#FF6F00"), "Best streak", $"{bestStreak} d", true));
                summary.Add(SummaryCard("alert", Theme.Hex("#E65100"), "Burning", $"{burning}", false));
                c.Add(summary);

                foreach (var h in App.Habits)
                {
                    string id = h.id;
                    c.Add(HabitLaneView.Create(h,
                        () => App.ShowHabitDetail(id),
                        () => App.ToggleHabitToday(id)));
                }
                root.Add(scroll);
            }

            // ---- FAB ----
            var fab = new VisualElement { name = "add-habit-fab" };
            fab.style.position = Position.Absolute;
            fab.style.right = 20; fab.style.bottom = 20;
            fab.style.width = 60; fab.style.height = 60;
            UIFactory.SetRadius(fab, 30f);
            fab.style.backgroundColor = Theme.Action;
            fab.style.alignItems = Align.Center;
            fab.style.justifyContent = Justify.Center;
            UIFactory.SetBottomBorder(fab, 5f, Theme.ActionDeep);
            fab.Add(UIFactory.Glyph("plus", 30, Color.white));
            UIFactory.WirePress(fab, () => App.ShowAddHabit(), 5f, Theme.ActionDeep);
            root.Add(fab);
        }

        private VisualElement SummaryCard(string icon, Color color, string label, string value, bool first)
        {
            var card = UIFactory.Column();
            card.style.flexGrow = 1;
            card.style.flexBasis = 0;
            card.style.alignItems = Align.Center;
            card.style.backgroundColor = Theme.Surface;
            UIFactory.SetRadius(card, 14f);
            card.style.paddingTop = 12; card.style.paddingBottom = 12;
            card.style.paddingLeft = 10; card.style.paddingRight = 10;
            card.style.marginLeft = first ? 0 : 5;
            card.style.marginRight = first ? 5 : 0;
            UIFactory.SetBottomBorder(card, 3f, Theme.Shadow);
            card.Add(UIFactory.Glyph(icon, 20, color));
            var v = UIFactory.Text(value, 18, true, Theme.TextPrimary);
            v.style.marginTop = 4;
            card.Add(v);
            card.Add(UIFactory.Text(label, 11, true, Theme.TextSecondary));
            return card;
        }

        private VisualElement BuildEmpty()
        {
            var wrap = UIFactory.Column();
            wrap.style.flexGrow = 1;
            wrap.style.alignItems = Align.Center;
            wrap.style.justifyContent = Justify.Center;
            wrap.style.paddingLeft = 32; wrap.style.paddingRight = 32;
            wrap.Add(UIFactory.Glyph("road-variant", 96, Theme.TextMuted));
            var t = UIFactory.Heading("No road yet", 22);
            t.style.marginTop = 12;
            wrap.Add(t);
            var b = UIFactory.Text("Add your first habit and start running across the daily road.",
                14, false, Theme.TextSecondary);
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.style.unityFontStyleAndWeight = FontStyle.Bold;
            b.style.marginTop = 8;
            wrap.Add(b);
            return wrap;
        }
    }
}
