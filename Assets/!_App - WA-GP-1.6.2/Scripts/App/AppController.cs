using System;
using System.Collections.Generic;
using HabitCross.Config;
using HabitCross.Core;
using HabitCross.UI.Components;
using HabitCross.UI.Screens;
using HabitCross.UI.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace HabitCross.App
{
    /// <summary>
    /// Application entry point. Creates the runtime UIDocument + PanelSettings,
    /// owns the habit data (the equivalent of the web app's AppProvider context),
    /// and drives navigation between the onboarding, tab, detail and add-habit
    /// screens. All asset references are injected via the inspector with safe
    /// in-code fallbacks, so there is no dependency on Resources, resource paths
    /// or asset file names at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public class AppController : MonoBehaviour
    {
        [Header("UI assets (optional — code fallbacks provided)")]
        [Tooltip("Root UXML cloned into the document. If unset, the root is built in code.")]
        [SerializeField] private VisualTreeAsset appRootUxml;
        [Tooltip("Main USS stylesheet applied to the panel root.")]
        [SerializeField] private StyleSheet appStyle;
        [Tooltip("Theme used by the runtime panel.")]
        [SerializeField] private ThemeStyleSheet theme;
        [Tooltip("Game configuration (palette, categories, icons).")]
        [SerializeField] private GameConfig config;
        [Tooltip("Panel settings asset carrying the text settings (emoji/fallback fonts). A panel is created in code when unset.")]
        [SerializeField] private PanelSettings panelSettings;
        [Tooltip("Onboarding slide illustrations (runner, gold tile, fire). Code-drawn shapes are used when unset.")]
        [SerializeField] private Texture2D[] onboardingIllustrations;

        [Header("Fonts")]
        [Tooltip("Primary UI font. Provides a consistent, readable face on all devices.")]
        [SerializeField] private Font bodyFont;
        [Tooltip("Optional fallback fonts (e.g. emoji/symbol coverage).")]
        [SerializeField] private Font[] fallbackFonts;

        [Header("Layout")]
        [SerializeField] private Vector2Int referenceResolution = new Vector2Int(1080, 1920);
        [Range(0f, 1f)]
        [SerializeField] private float screenMatch = 0.5f;

        // ---- Runtime ------------------------------------------------------
        private UIDocument _doc;
        private PanelSettings _panel;
        private bool _ownsPanel;
        private VisualElement _host;
        private ScreenRouter _router;

        // ---- State (app context) -----------------------------------------
        private readonly List<Habit> _habits = new List<Habit>();
        private bool _onboardingDone;
        private int _currentTab;

        public IReadOnlyList<Habit> Habits => _habits;
        public GameConfig Config { get; private set; }

        public Texture2D OnboardingIllustration(int index) =>
            onboardingIllustrations != null && index >= 0 && index < onboardingIllustrations.Length
                ? onboardingIllustrations[index]
                : null;

        /// <summary>Raised whenever habit data changes so live screens can refresh.</summary>
        public event Action HabitsChanged;

        // ---- Lifecycle ----------------------------------------------------

        private void Awake()
        {
            Config = config != null ? config : GameConfig.CreateDefault();
            _habits.Clear();
            _habits.AddRange(HabitStore.LoadHabits());
            _onboardingDone = HabitStore.IsOnboardingDone();
        }

        private int _buildRetries;

        private void Start()
        {
            BuildPanel();
            TryBuild();
        }

        // The runtime panel's root may not exist on the exact frame panelSettings
        // is assigned; retry for a few frames before giving up.
        private void TryBuild()
        {
            if (_doc != null && _doc.rootVisualElement != null)
            {
                BuildRoot();
                ShowInitial();
                return;
            }
            if (_buildRetries++ < 10) Invoke(nameof(TryBuild), 0f);
            else Debug.LogError("[AppController] UIDocument root never became available.");
        }

        private void OnDestroy()
        {
            if (_ownsPanel && _panel != null) Destroy(_panel);
        }

        // ---- Bootstrap ----------------------------------------------------

        private void BuildPanel()
        {
            // Prefer the serialized asset: its text settings (emoji/fallback fonts)
            // can only be wired in the inspector, not through the runtime API.
            _ownsPanel = panelSettings == null;
            if (_ownsPanel)
            {
                _panel = ScriptableObject.CreateInstance<PanelSettings>();
                _panel.name = "HabitCrossPanelSettings";
            }
            else
            {
                _panel = panelSettings;
            }
            if (theme != null) _panel.themeStyleSheet = theme;
            else Debug.LogWarning("[AppController] No ThemeStyleSheet assigned; " +
                                  "text may not render until one is provided.");

            _panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            _panel.referenceResolution = referenceResolution;
            _panel.match = screenMatch;
            _panel.clearColor = true;
            _panel.colorClearValue = Theme.ScreenBackground;

            _doc = GetComponent<UIDocument>();
            if (_doc == null) _doc = gameObject.AddComponent<UIDocument>();
            if (appRootUxml != null) _doc.visualTreeAsset = appRootUxml;
            _doc.panelSettings = _panel;
        }

        private void BuildRoot()
        {
            var root = _doc.rootVisualElement;
            if (appStyle != null && !root.styleSheets.Contains(appStyle))
                root.styleSheets.Add(appStyle);

            ApplyFonts(root);
            root.style.color = Theme.TextPrimary;

            // Use the UXML-provided root if present, otherwise build it in code.
            VisualElement appRoot = root.Q("app-root");
            if (appRoot == null)
            {
                appRoot = new VisualElement { name = "app-root" };
                appRoot.style.flexGrow = 1;
                root.Add(appRoot);
            }
            SafeArea.Attach(appRoot);

            _host = appRoot.Q("screen-host");
            if (_host == null)
            {
                _host = new VisualElement { name = "screen-host" };
                _host.style.flexGrow = 1;
                appRoot.Add(_host);
            }

            _router = new ScreenRouter(_host);
        }

        private void ApplyFonts(VisualElement root)
        {
            if (bodyFont == null) return;
            // Apply the primary font face to the panel root; all text inherits it.
            // Fallback fonts (e.g. emoji/symbol coverage) are layered through the
            // panel text settings when those assets are supplied in the inspector,
            // avoiding any hard-coded asset dependency.
            root.style.unityFont = bodyFont;
        }

        // ---- Navigation ---------------------------------------------------

        private void ShowInitial()
        {
            if (_onboardingDone) ShowTabs(0);
            else ShowOnboarding();
        }

        public void ShowOnboarding()
        {
            _router.SetBase(new OnboardingScreen(this).Build());
        }

        public void ShowTabs(int index)
        {
            _currentTab = Mathf.Clamp(index, 0, 2);
            _router.SetBase(BuildTabShell());
        }

        public void GoToTab(int index)
        {
            if (_router.HasOverlay) _router.PopOverlay();
            ShowTabs(index);
        }

        public void ShowAddHabit()
        {
            _router.PushOverlay(new AddHabitScreen(this).Build());
        }

        public void ShowHabitDetail(string id)
        {
            _router.PushOverlay(new HabitDetailScreen(this, id).Build());
        }

        public void Back()
        {
            _router.PopOverlay();
            HabitsChanged?.Invoke();
        }

        private VisualElement BuildTabShell()
        {
            var shell = new VisualElement { name = "tab-shell" };
            shell.style.flexGrow = 1;
            shell.style.backgroundColor = Theme.ScreenBackground;

            var content = new VisualElement { name = "tab-content" };
            content.style.flexGrow = 1;
            shell.Add(content);

            void Render()
            {
                content.Clear();
                VisualElement screen = _currentTab switch
                {
                    1 => new StatsScreen(this).Build(),
                    2 => new SettingsScreen(this).Build(),
                    _ => new HomeScreen(this).Build(),
                };
                screen.style.flexGrow = 1;
                content.Add(screen);
            }

            TabBar tabBar = null;
            tabBar = new TabBar(_currentTab, i =>
            {
                _currentTab = i;
                Render();
                tabBar.SetSelected(i);
            });
            shell.Add(tabBar.Root);
            Render();
            return shell;
        }

        // ---- Data operations (app context) --------------------------------

        public Habit FindHabit(string id) => _habits.Find(h => h.id == id);

        public Habit AddHabit(string name, HabitCategory category, string color, string icon, string reminderTime)
        {
            string c = string.IsNullOrEmpty(color)
                ? Config.ColorForIndex(_habits.Count)
                : color;
            var habit = HabitLogic.CreateHabit(name, category, c, icon, reminderTime);
            _habits.Add(habit);
            Persist();
            return habit;
        }

        public void ToggleHabitToday(string id)
        {
            var h = FindHabit(id);
            if (h == null) return;
            HabitLogic.ToggleToday(h);
            Persist();
        }

        public void DeleteHabit(string id)
        {
            int idx = _habits.FindIndex(h => h.id == id);
            if (idx >= 0)
            {
                _habits.RemoveAt(idx);
                Persist();
            }
        }

        public void FinishOnboarding()
        {
            HabitStore.MarkOnboardingDone();
            _onboardingDone = true;
            ShowTabs(0);
        }

        public void ResetEverything()
        {
            HabitStore.ResetAll();
            _habits.Clear();
            _onboardingDone = false;
            Persist();
        }

        private void Persist()
        {
            HabitStore.SaveHabits(_habits);
            HabitsChanged?.Invoke();
        }
    }
}
