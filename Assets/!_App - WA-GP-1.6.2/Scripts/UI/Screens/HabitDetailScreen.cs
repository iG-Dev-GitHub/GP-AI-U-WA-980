using HabitCross.App;
using HabitCross.Core;
using HabitCross.UI.Components;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Screens
{
    /// <summary>Single-habit deep dive. Ported from <c>habit/[id].tsx</c>.</summary>
    public class HabitDetailScreen : ScreenBase
    {
        private const int CalWeeks = 8;
        private readonly string _id;

        protected override bool LiveRefresh => true;

        public HabitDetailScreen(AppController app, string id) : base(app)
        {
            _id = id;
        }

        protected override void Compose(VisualElement root)
        {
            root.style.backgroundColor = Theme.ScreenBackground;
            var habit = App.FindHabit(_id);

            if (habit == null)
            {
                var missing = UIFactory.Text("Habit not found.", 16, true, Theme.TextPrimary);
                UIFactory.SetMargin(missing, 20, 20, 20, 20);
                root.Add(missing);
                var back = UIFactory.ChunkyButton("Go back", Theme.Action, Theme.ActionDeep,
                    Color.white, () => App.Back());
                back.style.marginLeft = 20; back.style.marginRight = 20;
                root.Add(back);
                return;
            }

            var state = HabitLogic.ComputeState(habit);

            // ---- Header ----
            var header = UIFactory.Row();
            header.style.paddingLeft = 12; header.style.paddingRight = 12;
            header.style.paddingTop = 10; header.style.paddingBottom = 10;
            header.Add(UIFactory.IconButton("arrow-left", Theme.TextPrimary, () => App.Back()));

            var titleCol = UIFactory.Column();
            titleCol.style.flexGrow = 1; titleCol.style.flexShrink = 1;
            titleCol.style.marginLeft = 8; titleCol.style.marginRight = 8;
            var name = UIFactory.Text(habit.name, 18, true, Theme.TextPrimary);
            name.style.whiteSpace = WhiteSpace.NoWrap;
            name.style.overflow = Overflow.Hidden;
            name.style.textOverflow = TextOverflow.Ellipsis;
            titleCol.Add(name);
            titleCol.Add(UIFactory.Text($"since {DateUtil.FormatShortDate(habit.createdAt)}",
                11, true, Theme.TextSecondary));
            header.Add(titleCol);

            header.Add(UIFactory.IconButton("trash-can", Theme.Danger, () =>
            {
                App.DeleteHabit(habit.id);
                App.Back();
            }));
            root.Add(header);

            // ---- Scroll body ----
            var scroll = ScrollViewUtil.Create(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            var c = scroll.contentContainer;
            c.style.paddingLeft = 16; c.style.paddingRight = 16;
            c.style.paddingTop = 16; c.style.paddingBottom = 24;
            root.Add(scroll);

            c.Add(HabitLaneView.Create(habit, null, () => App.ToggleHabitToday(habit.id)));

            // ---- Stats row ----
            var stats = UIFactory.Row();
            stats.style.marginTop = 4;
            stats.Add(StatBlock("fire", Theme.Hex("#FF6F00"), "Current", $"{state.currentStreak}d", 0));
            stats.Add(StatBlock("trophy", Theme.Hex("#FFB300"), "Best", $"{habit.bestStreak}d", 1));
            stats.Add(StatBlock("walk", Theme.Action, "Total", $"{state.totalDone}d", 2));
            c.Add(stats);

            // ---- Month Master ----
            if (HabitLogic.HasMonthMaster(habit))
            {
                var banner = UIFactory.Row();
                banner.style.marginTop = 14;
                banner.style.backgroundColor = Theme.Hex("#FFFDE7");
                UIFactory.SetRadius(banner, 14f);
                banner.style.paddingTop = 12; banner.style.paddingBottom = 12;
                banner.style.paddingLeft = 12; banner.style.paddingRight = 12;
                UIFactory.SetBottomBorder(banner, 3f, Theme.Hex("#FBC02D"));
                banner.Add(UIFactory.Glyph("medal", 48, Theme.MilestoneGold));
                var col = UIFactory.Column();
                col.style.flexGrow = 1; col.style.marginLeft = 12;
                col.Add(UIFactory.Text("Month Master", 16, true, Theme.Hex("#7E5A00")));
                col.Add(UIFactory.Text($"Earned at {habit.bestStreak}-day streak", 12, true, Theme.Hex("#7E5A00")));
                banner.Add(col);
                c.Add(banner);
            }

            // ---- Calendar ----
            var calTitle = UIFactory.Text("Calendar", 14, true, Theme.TextPrimary);
            calTitle.style.marginTop = 18; calTitle.style.marginBottom = 8;
            c.Add(calTitle);

            var calCard = UIFactory.Card(14f, 12f);
            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;

            string today = DateUtil.TodayKey();
            for (int i = CalWeeks * 7 - 1; i >= 0; i--)
            {
                string k = DateUtil.AddDays(today, -i);
                bool done = habit.IsDone(k);
                bool isToday = k == today;

                var cell = new VisualElement();
                cell.style.width = 28; cell.style.height = 28;
                cell.style.marginRight = 5; cell.style.marginBottom = 5;
                cell.style.alignItems = Align.Center; cell.style.justifyContent = Justify.Center;
                UIFactory.SetRadius(cell, 6f);
                cell.style.backgroundColor = done ? Theme.SuccessTile
                    : (isToday ? Theme.Hex(habit.color) : Theme.FutureTile);
                if (isToday)
                {
                    cell.style.borderTopWidth = 2; cell.style.borderBottomWidth = 2;
                    cell.style.borderLeftWidth = 2; cell.style.borderRightWidth = 2;
                    cell.style.borderTopColor = Theme.TextPrimary;
                    cell.style.borderBottomColor = Theme.TextPrimary;
                    cell.style.borderLeftColor = Theme.TextPrimary;
                    cell.style.borderRightColor = Theme.TextPrimary;
                }
                if (done)
                {
                    var mark = new VisualElement();
                    mark.style.width = 8; mark.style.height = 8;
                    UIFactory.SetRadius(mark, 4f);
                    mark.style.backgroundColor = Color.white;
                    cell.Add(mark);
                }
                grid.Add(cell);
            }
            calCard.Add(grid);
            var caption = UIFactory.Text($"Last {CalWeeks * 7} days · today is highlighted",
                11, true, Theme.TextMuted);
            caption.style.unityTextAlign = TextAnchor.MiddleCenter;
            caption.style.marginTop = 10;
            calCard.Add(caption);
            c.Add(calCard);
        }

        private VisualElement StatBlock(string icon, Color color, string label, string value, int index)
        {
            var card = UIFactory.Column();
            card.style.flexGrow = 1; card.style.flexBasis = 0;
            card.style.alignItems = Align.Center;
            card.style.backgroundColor = Theme.Surface;
            UIFactory.SetRadius(card, 12f);
            card.style.paddingTop = 12; card.style.paddingBottom = 12;
            card.style.marginLeft = index == 0 ? 0 : 5;
            card.style.marginRight = index == 2 ? 0 : 5;
            UIFactory.SetBottomBorder(card, 3f, Theme.Shadow);
            card.Add(UIFactory.Glyph(icon, 18, color));
            var v = UIFactory.Text(value, 18, true, Theme.TextPrimary);
            v.style.marginTop = 2;
            card.Add(v);
            card.Add(UIFactory.Text(label, 11, true, Theme.TextSecondary));
            return card;
        }
    }
}
