using HabitCross.App;
using UnityEngine.UIElements;

namespace HabitCross.UI.Screens
{
    /// <summary>
    /// Base for screens. Provides the build/compose pattern and optional live
    /// refresh: screens that opt in are rebuilt whenever habit data changes,
    /// mirroring the React re-render behaviour of the reference app.
    /// </summary>
    public abstract class ScreenBase
    {
        protected readonly AppController App;
        protected VisualElement Root;

        /// <summary>When true, the screen rebuilds on <see cref="AppController.HabitsChanged"/>.</summary>
        protected virtual bool LiveRefresh => false;

        protected ScreenBase(AppController app)
        {
            App = app;
        }

        public VisualElement Build()
        {
            Root = new VisualElement { name = GetType().Name };
            Root.style.flexGrow = 1;
            if (LiveRefresh)
            {
                Root.RegisterCallback<AttachToPanelEvent>(_ => App.HabitsChanged += Render);
                Root.RegisterCallback<DetachFromPanelEvent>(_ => App.HabitsChanged -= Render);
            }
            Render();
            return Root;
        }

        protected void Render()
        {
            Root.Clear();
            Compose(Root);
        }

        protected abstract void Compose(VisualElement root);
    }
}
