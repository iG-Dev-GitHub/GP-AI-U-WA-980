using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HabitCross.App
{
    /// <summary>
    /// Minimal navigation over a single host element: one "base" layer (onboarding
    /// or the tab shell) plus a stack of full-screen overlays (habit detail, the
    /// add-habit modal). Keeps screen lifetime simple and allocation-light.
    /// </summary>
    public class ScreenRouter
    {
        private readonly VisualElement _host;
        private VisualElement _base;
        private readonly List<VisualElement> _overlays = new List<VisualElement>();

        public ScreenRouter(VisualElement host)
        {
            _host = host;
        }

        public bool HasOverlay => _overlays.Count > 0;

        /// <summary>Replaces the base layer and clears any overlays.</summary>
        public void SetBase(VisualElement screen)
        {
            foreach (var o in _overlays) o.RemoveFromHierarchy();
            _overlays.Clear();
            _base?.RemoveFromHierarchy();
            _base = screen;
            if (screen != null)
            {
                screen.style.position = Position.Absolute;
                StretchToParent(screen);
                _host.Add(screen);
            }
        }

        public void PushOverlay(VisualElement screen)
        {
            if (screen == null) return;
            screen.style.position = Position.Absolute;
            StretchToParent(screen);
            _host.Add(screen);
            _overlays.Add(screen);
        }

        public void PopOverlay()
        {
            if (_overlays.Count == 0) return;
            var top = _overlays[_overlays.Count - 1];
            _overlays.RemoveAt(_overlays.Count - 1);
            top.RemoveFromHierarchy();
        }

        private static void StretchToParent(VisualElement e)
        {
            e.style.left = 0;
            e.style.top = 0;
            e.style.right = 0;
            e.style.bottom = 0;
        }
    }
}
