using System.Collections.Generic;
using HabitCross.App;
using HabitCross.Config;
using HabitCross.Core;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Screens
{
    /// <summary>Create-habit form. Ported from <c>add-habit.tsx</c>.</summary>
    public class AddHabitScreen : ScreenBase
    {
        private string _name = "";
        private string _category;
        private string _color;
        private string _icon;
        private string _reminder = "";

        private readonly List<(string token, VisualElement el)> _chips = new();
        private readonly List<(string hex, VisualElement el)> _dots = new();
        private readonly List<(string key, VisualElement el)> _cells = new();
        private VisualElement _saveBtn;
        private Label _saveLabel, _saveIcon;

        public AddHabitScreen(AppController app) : base(app) { }

        protected override void Compose(VisualElement root)
        {
            var cfg = App.Config;
            _category = cfg.categories.Length > 0 ? cfg.categories[0].key : "health";
            _color = cfg.habitColors.Length > 0 ? cfg.habitColors[0] : "#FF4081";
            _icon = cfg.icons.Length > 0 ? cfg.icons[0] : "run";

            root.style.backgroundColor = Theme.AddBackground;

            // ---- Header ----
            var header = UIFactory.Row();
            header.style.paddingLeft = 12; header.style.paddingRight = 12;
            header.style.paddingTop = 4; header.style.paddingBottom = 12;
            header.Add(UIFactory.IconButton("close", Theme.TextPrimary, () => App.Back()));
            var title = UIFactory.Text("New Habit", 18, true, Theme.TextPrimary);
            title.style.flexGrow = 1;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.Add(title);
            var spacer = new VisualElement(); spacer.style.width = 40;
            header.Add(spacer);
            root.Add(header);

            // ---- Scrollable form ----
            var scroll = ScrollViewUtil.Create(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            var c = scroll.contentContainer;
            c.style.paddingBottom = 16;
            root.Add(scroll);

            c.Add(FieldLabel("NAME"));
            c.Add(BuildInput("e.g. Drink water", 48, v => { _name = v; UpdateSave(); }, false));

            c.Add(FieldLabel("CATEGORY"));
            c.Add(BuildCategoryRow(cfg));

            c.Add(FieldLabel("LANE COLOR"));
            c.Add(BuildColorRow(cfg));

            c.Add(FieldLabel("ICON"));
            c.Add(BuildIconGrid(cfg));

            c.Add(FieldLabel("REMINDER TIME (OPTIONAL)"));
            c.Add(BuildInput("e.g. 08:00", 5, v => _reminder = v, false));
            var hint = UIFactory.Text("We'll surface a gentle in-app banner around this time.",
                11, false, Theme.TextMuted);
            hint.style.unityFontStyleAndWeight = FontStyle.Bold;
            hint.style.marginLeft = 20; hint.style.marginRight = 20; hint.style.marginTop = 6;
            c.Add(hint);

            // ---- Footer ----
            var footer = new VisualElement();
            footer.style.paddingLeft = 16; footer.style.paddingRight = 16;
            footer.style.paddingTop = 10; footer.style.paddingBottom = 14;
            footer.style.backgroundColor = Theme.AddBackground;
            _saveBtn = new VisualElement();
            _saveBtn.style.flexDirection = FlexDirection.Row;
            _saveBtn.style.alignItems = Align.Center;
            _saveBtn.style.justifyContent = Justify.Center;
            UIFactory.SetRadius(_saveBtn, 16f);
            _saveBtn.style.paddingTop = 14; _saveBtn.style.paddingBottom = 14;
            _saveIcon = UIFactory.Glyph("check-bold", 20, Color.white);
            _saveIcon.style.marginRight = 8;
            _saveLabel = UIFactory.Text("Save habit", 16, true, Color.white);
            _saveBtn.Add(_saveIcon);
            _saveBtn.Add(_saveLabel);
            UIFactory.WirePress(_saveBtn, OnSave, 4f, Theme.ActionDeep);
            footer.Add(_saveBtn);
            root.Add(footer);

            UpdateSave();
        }

        private void OnSave()
        {
            if (_name.Trim().Length == 0) return;
            App.AddHabit(_name, HabitCategoryUtil.FromToken(_category), _color, _icon, _reminder);
            App.Back();
        }

        private Label FieldLabel(string text)
        {
            var l = UIFactory.Text(text, 11, true, Theme.TextMuted);
            l.style.letterSpacing = 1;
            l.style.paddingLeft = 20; l.style.paddingRight = 20;
            l.style.marginTop = 14; l.style.marginBottom = 8;
            return l;
        }

        private VisualElement BuildInput(string placeholder, int maxLength,
            System.Action<string> onChange, bool numeric)
        {
            var wrap = new VisualElement();
            wrap.style.marginLeft = 16; wrap.style.marginRight = 16;
            wrap.style.backgroundColor = Theme.Surface;
            UIFactory.SetRadius(wrap, 12f);
            UIFactory.SetBottomBorder(wrap, 3f, new Color(0, 0, 0, 0.06f));

            var field = new TextField { maxLength = maxLength };
            field.style.marginTop = 0; field.style.marginBottom = 0;
            field.style.marginLeft = 0; field.style.marginRight = 0;
            field.style.paddingLeft = 14; field.style.paddingRight = 14;
            field.style.paddingTop = 12; field.style.paddingBottom = 12;
            field.style.fontSize = 16;
            field.style.color = Theme.TextPrimary;
            field.style.backgroundColor = Color.clear;
            var input = field.Q(className: "unity-text-field__input");
            if (input != null) input.style.backgroundColor = Color.clear;

            var ph = UIFactory.Text(placeholder, 16, false, Theme.TextMuted);
            ph.style.position = Position.Absolute;
            ph.style.left = 14; ph.style.top = 12;
            ph.pickingMode = PickingMode.Ignore;

            field.RegisterValueChangedCallback(e =>
            {
                ph.style.display = string.IsNullOrEmpty(e.newValue) ? DisplayStyle.Flex : DisplayStyle.None;
                onChange?.Invoke(e.newValue);
            });

            wrap.Add(field);
            wrap.Add(ph);
            return wrap;
        }

        private VisualElement BuildCategoryRow(GameConfig cfg)
        {
            var scroll = ScrollViewUtil.Create(ScrollViewMode.Horizontal);
            scroll.style.height = 44;
            scroll.style.flexShrink = 0;
            scroll.contentContainer.style.flexDirection = FlexDirection.Row;
            scroll.contentContainer.style.alignItems = Align.Center;
            scroll.contentContainer.style.paddingLeft = 16;
            scroll.contentContainer.style.paddingRight = 16;

            foreach (var cat in cfg.categories)
            {
                string token = cat.key;
                var chip = UIFactory.Row();
                chip.style.height = 36;
                chip.style.paddingLeft = 14; chip.style.paddingRight = 14;
                chip.style.marginRight = 8;
                chip.style.flexShrink = 0;
                UIFactory.SetRadius(chip, 18f);
                UIFactory.SetBottomBorder(chip, 3f, Theme.Shadow);
                var g = UIFactory.Glyph(cat.icon, 16, Theme.TextPrimary);
                var t = UIFactory.Text($" {cat.label}", 13, true, Theme.TextPrimary);
                chip.Add(g); chip.Add(t);
                chip.userData = new[] { g, t };
                UIFactory.WirePress(chip, () =>
                {
                    _category = token;
                    _icon = cat.icon;
                    UpdateCategorySelection();
                    UpdateIconSelection();
                }, 3f, Theme.Shadow);
                _chips.Add((token, chip));
                scroll.Add(chip);
            }
            UpdateCategorySelection();
            return scroll;
        }

        private void UpdateCategorySelection()
        {
            foreach (var (token, el) in _chips)
            {
                bool sel = token == _category;
                el.style.backgroundColor = sel ? Theme.Action : Theme.Surface;
                el.style.borderBottomColor = sel ? Theme.ActionDeep : Theme.Shadow;
                if (el.userData is Label[] parts)
                {
                    Color col = sel ? Color.white : Theme.TextPrimary;
                    parts[0].style.color = col;
                    parts[1].style.color = col;
                }
            }
        }

        private VisualElement BuildColorRow(GameConfig cfg)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.style.paddingLeft = 16; row.style.paddingRight = 16;

            foreach (var hex in cfg.habitColors)
            {
                string h = hex;
                var dot = new VisualElement();
                dot.style.width = 36; dot.style.height = 36;
                dot.style.marginRight = 10; dot.style.marginBottom = 10;
                UIFactory.SetRadius(dot, 18f);
                dot.style.backgroundColor = Theme.Hex(hex);
                UIFactory.SetBottomBorder(dot, 3f, new Color(0, 0, 0, 0.18f));
                UIFactory.WirePress(dot, () => { _color = h; UpdateColorSelection(); }, 3f, new Color(0, 0, 0, 0.18f));
                _dots.Add((hex, dot));
                row.Add(dot);
            }
            UpdateColorSelection();
            return row;
        }

        private void UpdateColorSelection()
        {
            foreach (var (hex, el) in _dots)
            {
                bool sel = hex == _color;
                el.style.borderTopWidth = sel ? 3 : 0;
                el.style.borderLeftWidth = sel ? 3 : 0;
                el.style.borderRightWidth = sel ? 3 : 0;
                el.style.borderTopColor = Theme.TextPrimary;
                el.style.borderLeftColor = Theme.TextPrimary;
                el.style.borderRightColor = Theme.TextPrimary;
                el.style.borderBottomColor = sel ? Theme.TextPrimary : new Color(0, 0, 0, 0.18f);
            }
        }

        private VisualElement BuildIconGrid(GameConfig cfg)
        {
            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.paddingLeft = 16; grid.style.paddingRight = 16;

            foreach (var key in cfg.icons)
            {
                string k = key;
                var cell = new VisualElement();
                cell.style.width = 44; cell.style.height = 44;
                cell.style.marginRight = 8; cell.style.marginBottom = 8;
                cell.style.alignItems = Align.Center; cell.style.justifyContent = Justify.Center;
                UIFactory.SetRadius(cell, 12f);
                UIFactory.SetBottomBorder(cell, 3f, Theme.Shadow);
                var g = UIFactory.Glyph(key, 20, Theme.TextPrimary);
                cell.Add(g);
                cell.userData = g;
                UIFactory.WirePress(cell, () => { _icon = k; UpdateIconSelection(); }, 3f, Theme.Shadow);
                _cells.Add((key, cell));
                grid.Add(cell);
            }
            UpdateIconSelection();
            return grid;
        }

        private void UpdateIconSelection()
        {
            foreach (var (key, el) in _cells)
            {
                bool sel = key == _icon;
                el.style.backgroundColor = sel ? Theme.Hex(_color) : Theme.Surface;
                el.style.borderBottomColor = sel ? new Color(0, 0, 0, 0.25f) : Theme.Shadow;
                if (el.userData is Label g) g.style.color = sel ? Color.white : Theme.TextPrimary;
            }
        }

        private void UpdateSave()
        {
            bool canSave = _name.Trim().Length > 0;
            if (_saveBtn == null) return;
            _saveBtn.style.backgroundColor = canSave ? Theme.Action : Theme.FutureTile;
            UIFactory.SetBottomBorder(_saveBtn, 4f, canSave ? Theme.ActionDeep : Theme.Hex("#CFD8DC"));
            Color txt = canSave ? Color.white : Theme.Hex("#B0BEC5");
            _saveLabel.style.color = txt;
            _saveIcon.style.color = txt;
        }
    }
}
