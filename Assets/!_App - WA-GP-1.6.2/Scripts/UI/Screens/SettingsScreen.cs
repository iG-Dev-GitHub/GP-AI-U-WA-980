using HabitCross.App;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Screens
{
    /// <summary>The Settings tab: reminder toggle, data reset, about. Ported from <c>(tabs)/settings.tsx</c>.</summary>
    public class SettingsScreen : ScreenBase
    {
        private bool _reminderOn = true;
        private bool _confirmReset;
        private string _resetMessage;

        public SettingsScreen(AppController app) : base(app) { }

        protected override void Compose(VisualElement root)
        {
            root.style.backgroundColor = Theme.ScreenBackground;

            var scroll = ScrollViewUtil.Create(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            var c = scroll.contentContainer;
            c.style.paddingBottom = 24;
            root.Add(scroll);

            var title = UIFactory.Heading("Settings", 28);
            title.style.paddingLeft = 20; title.style.paddingRight = 20;
            title.style.paddingTop = 12; title.style.paddingBottom = 6;
            c.Add(title);

            // ---- Reminders ----
            var reminders = Section("REMINDERS");
            var rRow = UIFactory.Row();
            var rLeft = UIFactory.Row();
            rLeft.style.flexGrow = 1;
            rLeft.Add(UIFactory.Glyph("bell", 20, Theme.Action));
            var rText = UIFactory.Column();
            rText.style.flexGrow = 1; rText.style.flexShrink = 1; rText.style.marginLeft = 10;
            rText.Add(UIFactory.Text("In-app reminders", 15, true, Theme.TextPrimary));
            rText.Add(UIFactory.Text("Show a banner on launch when habits aren't done today.",
                12, false, Theme.TextSecondary));
            rLeft.Add(rText);
            rRow.Add(rLeft);
            rRow.Add(BuildSwitch(_reminderOn, v => { _reminderOn = v; Render(); }));
            reminders.Add(rRow);
            c.Add(reminders);

            // ---- Data ----
            var data = Section("DATA");
            var dRow = UIFactory.Row();
            dRow.Add(UIFactory.Glyph("database", 20, Theme.Hex("#00897B")));
            var dText = UIFactory.Column();
            dText.style.flexGrow = 1; dText.style.marginLeft = 10;
            dText.Add(UIFactory.Text("Stored locally", 15, true, Theme.TextPrimary));
            int n = App.Habits.Count;
            dText.Add(UIFactory.Text($"{n} habit{(n == 1 ? "" : "s")} · offline-first, no account needed.",
                12, false, Theme.TextSecondary));
            dRow.Add(dText);
            data.Add(dRow);

            if (_confirmReset)
            {
                var confirm = UIFactory.Column();
                confirm.style.backgroundColor = Theme.Hex("#FFEBEE");
                UIFactory.SetRadius(confirm, 12f);
                confirm.style.paddingTop = 12; confirm.style.paddingBottom = 12;
                confirm.style.paddingLeft = 12; confirm.style.paddingRight = 12;
                confirm.style.marginTop = 4;
                confirm.Add(UIFactory.Text("Delete all habits and history?", 13, true, Theme.DangerDeep));
                var btns = UIFactory.Row();
                btns.style.marginTop = 10;
                var cancel = UIFactory.ChunkyButton("Cancel", Theme.FutureTile, Theme.Shadow,
                    Theme.TextPrimary, () => { _confirmReset = false; Render(); }, null, 13);
                cancel.style.flexGrow = 1; cancel.style.marginRight = 4;
                var yes = UIFactory.ChunkyButton("Yes, reset", Theme.Danger, Theme.DangerDeep,
                    Color.white, DoReset, null, 13);
                yes.style.flexGrow = 1; yes.style.marginLeft = 4;
                btns.Add(cancel); btns.Add(yes);
                confirm.Add(btns);
                data.Add(confirm);
            }
            else
            {
                var danger = UIFactory.ChunkyButton("Reset all data", Theme.Danger, Theme.DangerDeep,
                    Color.white, () => { _confirmReset = true; Render(); }, "trash-can", 14);
                danger.style.marginTop = 4;
                data.Add(danger);
            }

            if (!string.IsNullOrEmpty(_resetMessage))
            {
                var msg = UIFactory.Text(_resetMessage, 12, true, Theme.Hex("#1B5E20"));
                msg.style.unityTextAlign = TextAnchor.MiddleCenter;
                msg.style.marginTop = 4;
                data.Add(msg);
            }
            c.Add(data);
        }

        private void DoReset()
        {
            App.ResetEverything();
            _confirmReset = false;
            _resetMessage = "All data cleared. Welcome back!";
            Render();
            // Clear the message after a short delay.
            Root.schedule.Execute(() =>
            {
                _resetMessage = null;
                if (Root.panel != null) Render();
            }).StartingIn(3000);
        }

        private VisualElement Section(string label)
        {
            var s = UIFactory.Column();
            s.style.backgroundColor = Theme.Surface;
            s.style.marginLeft = 16; s.style.marginRight = 16; s.style.marginTop = 12;
            UIFactory.SetRadius(s, 14f);
            s.style.paddingTop = 14; s.style.paddingBottom = 14;
            s.style.paddingLeft = 14; s.style.paddingRight = 14;
            UIFactory.SetBottomBorder(s, 3f, Theme.Shadow);
            var l = UIFactory.Text(label, 11, true, Theme.TextMuted);
            l.style.letterSpacing = 1;
            l.style.marginBottom = 10;
            s.Add(l);
            return s;
        }

        private VisualElement BuildSwitch(bool on, System.Action<bool> onChange)
        {
            var track = new VisualElement();
            track.style.width = 46; track.style.height = 26;
            UIFactory.SetRadius(track, 13f);
            track.style.backgroundColor = on ? Theme.Hex("#90CAF9") : Theme.Hex("#CFD8DC");
            track.style.justifyContent = Justify.Center;

            var thumb = new VisualElement();
            thumb.style.width = 20; thumb.style.height = 20;
            UIFactory.SetRadius(thumb, 10f);
            thumb.style.backgroundColor = on ? Theme.Action : Theme.FutureTile;
            thumb.style.position = Position.Absolute;
            thumb.style.left = on ? 23 : 3;
            track.Add(thumb);

            UIFactory.WirePress(track, () => onChange?.Invoke(!on), 0f, Theme.Surface);
            return track;
        }
    }
}
