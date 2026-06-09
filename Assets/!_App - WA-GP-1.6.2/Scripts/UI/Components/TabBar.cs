using System;
using System.Collections.Generic;
using HabitCross.UI.Util;
using UnityEngine.UIElements;

namespace HabitCross.UI.Components
{
    /// <summary>Bottom navigation bar (Road / Stats / Settings).</summary>
    public class TabBar
    {
        private struct Item { public string icon; public string label; }

        private static readonly Item[] Items =
        {
            new Item { icon = "road-variant", label = "Road" },
            new Item { icon = "chart-bar", label = "Stats" },
            new Item { icon = "cog", label = "Settings" },
        };

        public VisualElement Root { get; }
        private readonly List<(Label glyph, Label label)> _cells = new List<(Label, Label)>();
        private int _selected;

        public TabBar(int selected, Action<int> onSelect)
        {
            _selected = selected;
            Root = new VisualElement { name = "tab-bar" };
            Root.AddToClassList("tab-bar");
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.backgroundColor = Theme.Surface;
            Root.style.height = 64;
            Root.style.paddingTop = 8;
            Root.style.paddingBottom = 10;

            for (int i = 0; i < Items.Length; i++)
            {
                int index = i;
                var cell = new VisualElement();
                cell.style.flexGrow = 1;
                cell.style.alignItems = Align.Center;
                cell.style.justifyContent = Justify.Center;

                var glyph = UIFactory.Glyph(Items[i].icon, 22, Color(i));
                var label = UIFactory.Text(Items[i].label, 11, true, Color(i));
                label.style.marginTop = 2;
                cell.Add(glyph);
                cell.Add(label);

                UIFactory.WirePress(cell, () => onSelect?.Invoke(index), 0f, Theme.Surface);
                Root.Add(cell);
                _cells.Add((glyph, label));
            }
        }

        public void SetSelected(int index)
        {
            _selected = index;
            for (int i = 0; i < _cells.Count; i++)
            {
                _cells[i].glyph.style.color = Color(i);
                _cells[i].label.style.color = Color(i);
            }
        }

        private UnityEngine.Color Color(int i) => i == _selected ? Theme.Action : Theme.TextMuted;
    }
}
