using HabitCross.App;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Screens
{
    /// <summary>3 swipeable tutorial slides with paging dots. Ported from <c>onboarding.tsx</c>.</summary>
    public class OnboardingScreen : ScreenBase
    {
        private struct Slide { public string title, body, glyph; public string bg; public Color glyphColor; }

        private static readonly Slide[] Slides =
        {
            new Slide {
                title = "Add a habit — start running",
                body = "Each habit becomes its own road. Your little runner sprints forward every day you check in.",
                bg = "#80D8FF", glyph = "run-fast", glyphColor = default },
            new Slide {
                title = "Check in daily — move forward",
                body = "Tap Done Today and watch your road turn green. Every 7th day becomes a golden milestone tile.",
                bg = "#B9F6CA", glyph = "star-four-points", glyphColor = default },
            new Slide {
                title = "Miss days — fire blocks your path",
                body = "Skip a day and a flame appears ahead. Miss 3+ days and the streak burns. Get back, put out the fire.",
                bg = "#FFCCBC", glyph = "fire", glyphColor = default },
        };

        private int _page;
        private ScrollView _scroll;
        private VisualElement _dotsRow;
        private VisualElement _cta;
        private Label _ctaLabel;
        private Label _ctaIcon;
        private float _slideWidth = 1f;

        public OnboardingScreen(AppController app) : base(app) { }

        protected override void Compose(VisualElement root)
        {
            root.style.backgroundColor = Theme.Hex("#80D8FF");

            _scroll = ScrollViewUtil.Create(ScrollViewMode.Horizontal);
            _scroll.style.flexGrow = 1;
            _scroll.contentContainer.style.flexDirection = FlexDirection.Row;
            _scroll.name = "onboarding-scroll";

            for (int i = 0; i < Slides.Length; i++)
                _scroll.Add(BuildSlide(Slides[i], i));

            root.Add(_scroll);

            // Paging: snap to nearest slide on release and resize slides to viewport.
            _scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => ResizeSlides());
            _scroll.RegisterCallback<PointerUpEvent>(_ => SnapToNearest(), TrickleDown.NoTrickleDown);

            _dotsRow = BuildDots();
            root.Add(_dotsRow);
            root.Add(BuildFooter());

            UpdatePageVisuals();
        }

        private VisualElement BuildSlide(Slide s, int index)
        {
            var slide = new VisualElement { name = $"onboarding-slide-{index}" };
            slide.style.backgroundColor = Theme.Hex(s.bg);
            slide.style.flexShrink = 0;
            slide.style.alignItems = Align.Center;
            slide.style.justifyContent = Justify.Center;
            slide.style.paddingLeft = 28; slide.style.paddingRight = 28;
            slide.style.paddingTop = 40; slide.style.paddingBottom = 40;

            var disc = new VisualElement();
            disc.style.width = 200; disc.style.height = 200;
            UIFactory.SetRadius(disc, 100f);
            disc.style.backgroundColor = new Color(1f, 1f, 1f, 0.55f);
            disc.style.alignItems = Align.Center;
            disc.style.justifyContent = Justify.Center;
            disc.style.marginBottom = 32;
            UIFactory.SetBottomBorder(disc, 6f, new Color(0, 0, 0, 0.08f));
            disc.Add(BuildIllustration(index));
            slide.Add(disc);

            var title = UIFactory.Text(s.title, 26, true, Theme.TextPrimary);
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 14;
            slide.Add(title);

            var body = UIFactory.Text(s.body, 15, false, Theme.Hex("#37474F"));
            body.style.unityTextAlign = TextAnchor.MiddleCenter;
            body.style.unityFontStyleAndWeight = FontStyle.Bold;
            slide.Add(body);

            return slide;
        }

        private VisualElement BuildIllustration(int index)
        {
            switch (index)
            {
                case 0: // runner
                    return Shapes.Runner(110f, Theme.Hex("#FF4081"));
                case 1: // golden milestone
                    return Shapes.Diamond(96f, Theme.MilestoneGold, Theme.Hex("#F9A825"));
                default: // fire / missed
                    return Shapes.Diamond(96f, Theme.Hex("#FF3D00"), Theme.Hex("#BF360C"));
            }
        }

        private VisualElement BuildDots()
        {
            var row = new VisualElement { name = "onboarding-dots" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.paddingTop = 16; row.style.paddingBottom = 16;
            for (int i = 0; i < Slides.Length; i++)
            {
                var dot = new VisualElement();
                dot.style.height = 10;
                dot.style.marginLeft = 4; dot.style.marginRight = 4;
                UIFactory.SetRadius(dot, 5f);
                row.Add(dot);
            }
            return row;
        }

        private VisualElement BuildFooter()
        {
            var footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.alignItems = Align.Center;
            footer.style.paddingLeft = 24; footer.style.paddingRight = 24;
            footer.style.paddingTop = 8; footer.style.paddingBottom = 16;

            var skip = UIFactory.Text("Skip", 15, true, Theme.Hex("#37474F"));
            skip.style.paddingLeft = 16; skip.style.paddingRight = 16;
            skip.style.paddingTop = 14; skip.style.paddingBottom = 14;
            UIFactory.WirePress(skip, () => App.FinishOnboarding(), 0f, Theme.Surface);
            footer.Add(skip);

            _cta = new VisualElement { name = "onboarding-next" };
            _cta.style.flexGrow = 1;
            _cta.style.flexDirection = FlexDirection.Row;
            _cta.style.alignItems = Align.Center;
            _cta.style.justifyContent = Justify.Center;
            _cta.style.backgroundColor = Theme.Action;
            UIFactory.SetRadius(_cta, 16f);
            _cta.style.paddingTop = 14; _cta.style.paddingBottom = 14;
            UIFactory.SetBottomBorder(_cta, 4f, Theme.ActionDeep);
            _ctaLabel = UIFactory.Text("Next", 16, true, Color.white);
            _ctaIcon = UIFactory.Glyph("arrow-right-bold", 22, Color.white);
            _ctaIcon.style.marginLeft = 8;
            _cta.Add(_ctaLabel);
            _cta.Add(_ctaIcon);
            UIFactory.WirePress(_cta, GoNext, 4f, Theme.ActionDeep);
            footer.Add(_cta);

            return footer;
        }

        private void ResizeSlides()
        {
            float w = _scroll.contentViewport.resolvedStyle.width;
            if (w <= 1 || float.IsNaN(w)) return;
            _slideWidth = w;
            foreach (var child in _scroll.contentContainer.Children())
                child.style.width = w;
            _scroll.scrollOffset = new Vector2(_page * w, 0);
        }

        private void SnapToNearest()
        {
            if (_slideWidth <= 1) return;
            int p = Mathf.Clamp(Mathf.RoundToInt(_scroll.scrollOffset.x / _slideWidth), 0, Slides.Length - 1);
            _page = p;
            _scroll.scrollOffset = new Vector2(p * _slideWidth, 0);
            UpdatePageVisuals();
        }

        private void GoNext()
        {
            if (_page < Slides.Length - 1)
            {
                _page++;
                _scroll.scrollOffset = new Vector2(_page * _slideWidth, 0);
                UpdatePageVisuals();
            }
            else
            {
                App.FinishOnboarding();
            }
        }

        private void UpdatePageVisuals()
        {
            var dots = _dotsRow.Children();
            int i = 0;
            foreach (var dot in dots)
            {
                bool active = i == _page;
                dot.style.width = active ? 24 : 10;
                dot.style.backgroundColor = active ? Theme.TextPrimary : new Color(0, 0, 0, 0.18f);
                i++;
            }
            bool last = _page == Slides.Length - 1;
            _ctaLabel.text = last ? "Start Running" : "Next";
            _ctaIcon.text = Glyphs.For(last ? "play-circle" : "arrow-right-bold");
        }
    }
}
