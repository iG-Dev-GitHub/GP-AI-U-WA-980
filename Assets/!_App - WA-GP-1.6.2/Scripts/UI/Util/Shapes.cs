using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.UI.Util
{
    /// <summary>
    /// Font-independent vector-ish markers built from VisualElements. Used for the
    /// runner, milestones and fire so the game reads correctly on any device,
    /// matching the blocky / voxel design language.
    /// </summary>
    public static class Shapes
    {
        /// <summary>A rotated rounded square — reads as a star / gem / ember.</summary>
        public static VisualElement Diamond(float size, Color fill, Color border)
        {
            var d = new VisualElement();
            d.style.width = size; d.style.height = size;
            d.style.backgroundColor = fill;
            UIFactory.SetRadius(d, size * 0.18f);
            d.style.borderBottomWidth = Mathf.Max(2f, size * 0.1f);
            d.style.borderBottomColor = border;
            d.style.rotate = new Rotate(new Angle(45f));
            d.pickingMode = PickingMode.Ignore;
            return d;
        }

        /// <summary>A friendly runner: a dark disc with two eyes.</summary>
        public static VisualElement Runner(float size, Color color)
        {
            var head = new VisualElement();
            head.style.width = size; head.style.height = size;
            UIFactory.SetRadius(head, size / 2f);
            head.style.backgroundColor = color;
            head.style.borderBottomWidth = Mathf.Max(2f, size * 0.1f);
            head.style.borderBottomColor = new Color(0f, 0f, 0f, 0.25f);
            head.style.flexDirection = FlexDirection.Row;
            head.style.justifyContent = Justify.Center;
            head.style.alignItems = Align.Center;
            head.style.paddingTop = size * 0.27f;
            head.pickingMode = PickingMode.Ignore;

            float eye = Mathf.Max(3f, size * 0.16f);
            head.Add(Eye(eye));
            var gap = new VisualElement(); gap.style.width = size * 0.13f;
            head.Add(gap);
            head.Add(Eye(eye));
            return head;
        }

        private static VisualElement Eye(float size)
        {
            var e = new VisualElement();
            e.style.width = size; e.style.height = size;
            UIFactory.SetRadius(e, size / 2f);
            e.style.backgroundColor = Color.white;
            return e;
        }
    }
}
