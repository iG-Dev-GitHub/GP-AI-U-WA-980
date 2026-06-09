using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Util
{
    /// <summary>
    /// Pads a target element to the device safe area so nothing is hidden behind
    /// notches, status bars, Dynamic Island, camera cutouts or rounded corners.
    /// Converts <see cref="Screen.safeArea"/> (pixels) into panel-space points
    /// using the target's resolved size, so it is correct under any PanelSettings
    /// scale mode. Re-evaluated on layout changes and polled for rotation.
    /// </summary>
    public static class SafeArea
    {
        /// <summary>Begins applying safe-area padding to <paramref name="target"/>.</summary>
        public static void Attach(VisualElement target)
        {
            if (target == null) return;
            var state = new State();
            target.RegisterCallback<GeometryChangedEvent>(_ => Apply(target, state));
            target.schedule.Execute(() => Apply(target, state)).Every(250);
        }

        private class State
        {
            public Rect lastSafeArea = Rect.zero;
            public Vector2 lastScreen = Vector2.zero;
            public bool applied;
        }

        private static void Apply(VisualElement target, State state)
        {
            Rect sa = Screen.safeArea;
            var screen = new Vector2(Screen.width, Screen.height);
            if (screen.x <= 0 || screen.y <= 0) return;

            float panelW = target.resolvedStyle.width;
            float panelH = target.resolvedStyle.height;
            if (float.IsNaN(panelW) || float.IsNaN(panelH) || panelW <= 0 || panelH <= 0)
                return;

            if (state.applied && sa == state.lastSafeArea && screen == state.lastScreen)
                return;
            state.lastSafeArea = sa;
            state.lastScreen = screen;
            state.applied = true;

            float scaleX = panelW / screen.x;
            float scaleY = panelH / screen.y;

            float left = sa.xMin;
            float right = screen.x - sa.xMax;
            // Screen.safeArea uses a bottom-left origin; UI Toolkit uses top-left.
            float bottom = sa.yMin;
            float top = screen.y - sa.yMax;

            target.style.paddingLeft = Mathf.Max(0, left) * scaleX;
            target.style.paddingRight = Mathf.Max(0, right) * scaleX;
            target.style.paddingTop = Mathf.Max(0, top) * scaleY;
            target.style.paddingBottom = Mathf.Max(0, bottom) * scaleY;
        }
    }

    /// <summary>A container that pads itself to the safe area (convenience wrapper).</summary>
    public class SafeAreaElement : VisualElement
    {
        public SafeAreaElement()
        {
            name = "safe-area";
            style.flexGrow = 1;
            SafeArea.Attach(this);
        }
    }
}
