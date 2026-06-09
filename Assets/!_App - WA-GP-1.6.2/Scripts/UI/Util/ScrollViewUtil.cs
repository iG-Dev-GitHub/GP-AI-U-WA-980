using UnityEngine.UIElements;

namespace HabitCross.UI.Util
{
    /// <summary>
    /// Helpers for ScrollView configuration. Per project requirement, every
    /// ScrollView must hide its scrollbars while keeping scrolling fully
    /// functional (touch / drag / mouse wheel). We hide the bars (rather than
    /// disabling them) so the scroll logic, wheel and pointer dragging stay live.
    /// </summary>
    public static class ScrollViewUtil
    {
        /// <summary>Creates a ScrollView with hidden scrollbars and drag scrolling.</summary>
        public static ScrollView Create(ScrollViewMode mode = ScrollViewMode.Vertical)
        {
            var sv = new ScrollView(mode);
            Configure(sv);
            return sv;
        }

        /// <summary>Hides scrollbars on an existing ScrollView and enables drag.</summary>
        public static void Configure(ScrollView sv)
        {
            if (sv == null) return;

            // Keep scrolling available, but never show the bars.
            sv.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            sv.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            // Touch-friendly drag/inertia (works for mouse drag too).
            sv.mode = sv.mode; // no-op; keeps API obvious
            sv.touchScrollBehavior = ScrollView.TouchScrollBehavior.Elastic;
            sv.scrollDecelerationRate = 0.135f;
            sv.elasticity = 0.1f;

            // Belt-and-suspenders: collapse the scroller elements if present so
            // they never reserve layout space even on platforms that force them.
            HideScroller(sv.verticalScroller);
            HideScroller(sv.horizontalScroller);
        }

        private static void HideScroller(Scroller scroller)
        {
            if (scroller == null) return;
            scroller.style.display = DisplayStyle.None;
            scroller.style.width = 0;
            scroller.style.height = 0;
        }
    }
}
