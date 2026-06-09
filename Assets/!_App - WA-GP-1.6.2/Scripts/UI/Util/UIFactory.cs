using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Util
{
    /// <summary>
    /// Builders for the chunky, blocky "Crossy Road" UI language. Styling is
    /// applied inline so the layout is correct even before USS resolves, while
    /// every element also gets a USS class hook so <c>App.uss</c> can theme or
    /// override it. Mirrors the look defined in <c>design_guidelines.json</c>.
    /// </summary>
    public static class UIFactory
    {
        // ---- Containers ---------------------------------------------------

        public static VisualElement Column(params string[] classes)
        {
            var e = new VisualElement();
            e.style.flexDirection = FlexDirection.Column;
            AddClasses(e, classes);
            return e;
        }

        public static VisualElement Row(params string[] classes)
        {
            var e = new VisualElement();
            e.style.flexDirection = FlexDirection.Row;
            e.style.alignItems = Align.Center;
            AddClasses(e, classes);
            return e;
        }

        public static VisualElement Spacer()
        {
            var e = new VisualElement();
            e.style.flexGrow = 1;
            return e;
        }

        /// <summary>White "physical block" card with a soft bottom border.</summary>
        public static VisualElement Card(float radius = 18f, float pad = 14f)
        {
            var e = new VisualElement();
            e.AddToClassList("card");
            e.style.backgroundColor = Theme.Surface;
            SetRadius(e, radius);
            e.style.paddingTop = pad;
            e.style.paddingBottom = pad;
            e.style.paddingLeft = pad;
            e.style.paddingRight = pad;
            SetBottomBorder(e, 4f, Theme.Shadow);
            e.style.marginBottom = 14f;
            return e;
        }

        // ---- Text ---------------------------------------------------------

        public static Label Text(string text, int size, bool bold, Color color)
        {
            var l = new Label(text ?? string.Empty);
            l.style.fontSize = size;
            l.style.color = color;
            l.style.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal;
            l.style.whiteSpace = WhiteSpace.Normal; // wrap by default
            l.AddToClassList("text");
            return l;
        }

        public static Label Heading(string text, int size = 28)
        {
            var l = Text(text, size, true, Theme.TextPrimary);
            l.AddToClassList("heading");
            return l;
        }

        public static Label Glyph(string iconKey, int size, Color color)
        {
            var l = new Label(Glyphs.For(iconKey));
            l.style.fontSize = size;
            l.style.color = color;
            l.style.unityTextAlign = TextAnchor.MiddleCenter;
            l.AddToClassList("glyph");
            l.pickingMode = PickingMode.Ignore;
            return l;
        }

        // ---- Buttons ------------------------------------------------------

        /// <summary>
        /// A chunky button that visually "presses down" (translate 2px, thinner
        /// bottom border) while held. Works with touch and mouse.
        /// </summary>
        public static VisualElement ChunkyButton(string label, Color bg, Color border,
            Color textColor, Action onClick, string iconKey = null, int fontSize = 16)
        {
            var btn = new VisualElement();
            btn.AddToClassList("chunky-button");
            btn.style.flexDirection = FlexDirection.Row;
            btn.style.alignItems = Align.Center;
            btn.style.justifyContent = Justify.Center;
            btn.style.backgroundColor = bg;
            SetRadius(btn, 16f);
            btn.style.paddingTop = 14;
            btn.style.paddingBottom = 14;
            btn.style.paddingLeft = 16;
            btn.style.paddingRight = 16;
            SetBottomBorder(btn, 4f, border);

            if (!string.IsNullOrEmpty(iconKey))
            {
                var g = Glyph(iconKey, fontSize + 4, textColor);
                g.style.marginRight = 8;
                btn.Add(g);
            }

            var l = Text(label, fontSize, true, textColor);
            btn.Add(l);

            WirePress(btn, onClick, 4f, border);
            return btn;
        }

        /// <summary>A small square/round icon-only button (e.g. header back/close).</summary>
        public static VisualElement IconButton(string iconKey, Color glyphColor, Action onClick,
            float size = 40f, bool round = false)
        {
            var btn = new VisualElement();
            btn.AddToClassList("icon-button");
            btn.style.width = size;
            btn.style.height = size;
            btn.style.alignItems = Align.Center;
            btn.style.justifyContent = Justify.Center;
            btn.style.backgroundColor = Theme.Surface;
            SetRadius(btn, round ? size / 2f : 12f);
            SetBottomBorder(btn, 3f, Theme.Shadow);
            btn.Add(Glyph(iconKey, 22, glyphColor));
            WirePress(btn, onClick, 3f, Theme.Shadow);
            return btn;
        }

        /// <summary>Adds press feedback + click handling to any element.</summary>
        public static void WirePress(VisualElement el, Action onClick, float restBorder, Color borderColor)
        {
            bool down = false;
            el.RegisterCallback<PointerDownEvent>(evt =>
            {
                down = true;
                el.style.translate = new Translate(0, 2, 0);
                el.style.borderBottomWidth = Mathf.Max(0, restBorder - 2f);
                el.CapturePointer(evt.pointerId);
            });
            Action release = () =>
            {
                if (!down) return;
                down = false;
                el.style.translate = new Translate(0, 0, 0);
                el.style.borderBottomWidth = restBorder;
            };
            el.RegisterCallback<PointerUpEvent>(evt =>
            {
                bool wasDown = down;
                release();
                if (el.HasPointerCapture(evt.pointerId)) el.ReleasePointer(evt.pointerId);
                if (wasDown && el.ContainsPoint(evt.localPosition)) onClick?.Invoke();
            });
            el.RegisterCallback<PointerCaptureOutEvent>(_ => release());
        }

        // ---- Style helpers ------------------------------------------------

        /// <summary>
        /// Stops pointer events from bubbling past <paramref name="el"/> so an
        /// inner control (a Done button, a draggable road) does not also trigger
        /// the surrounding card's tap-to-open handler.
        /// </summary>
        public static void BlockPropagation(VisualElement el)
        {
            el.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());
            el.RegisterCallback<PointerUpEvent>(e => e.StopPropagation());
            el.RegisterCallback<PointerMoveEvent>(e => e.StopPropagation());
        }

        public static void SetRadius(VisualElement e, float r)
        {
            e.style.borderTopLeftRadius = r;
            e.style.borderTopRightRadius = r;
            e.style.borderBottomLeftRadius = r;
            e.style.borderBottomRightRadius = r;
        }

        public static void SetBottomBorder(VisualElement e, float width, Color color)
        {
            e.style.borderBottomWidth = width;
            e.style.borderBottomColor = color;
        }

        public static void SetPadding(VisualElement e, float t, float r, float b, float l)
        {
            e.style.paddingTop = t;
            e.style.paddingRight = r;
            e.style.paddingBottom = b;
            e.style.paddingLeft = l;
        }

        public static void SetMargin(VisualElement e, float t, float r, float b, float l)
        {
            e.style.marginTop = t;
            e.style.marginRight = r;
            e.style.marginBottom = b;
            e.style.marginLeft = l;
        }

        private static void AddClasses(VisualElement e, string[] classes)
        {
            if (classes == null) return;
            foreach (var c in classes)
                if (!string.IsNullOrEmpty(c)) e.AddToClassList(c);
        }
    }
}
