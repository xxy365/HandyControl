using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace HandyControl.Controls
{
    public class RibbonGroup : HeaderedItemsControl
    {
        private const string LauncherButton = "PART_LauncherButton";

        public static readonly RoutedEvent<RoutedEventArgs> LauncherClickEvent =
            RoutedEvent.Register<RibbonGroup, RoutedEventArgs>(nameof(LauncherClick), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs>? LauncherClick
        {
            add => AddHandler(LauncherClickEvent, value);
            remove => RemoveHandler(LauncherClickEvent, value);
        }

        public static readonly StyledProperty<bool> ShowLauncherButtonProperty =
            AvaloniaProperty.Register<RibbonGroup, bool>(nameof(ShowLauncherButton));

        public bool ShowLauncherButton
        {
            get => (bool) GetValue(ShowLauncherButtonProperty);
            set => SetValue(ShowLauncherButtonProperty, value);
        }

        public static readonly StyledProperty<bool> ShowSplitterProperty =
            AvaloniaProperty.Register<RibbonGroup, bool>(nameof(ShowSplitter), true);

        public bool ShowSplitter
        {
            get => (bool) GetValue(ShowSplitterProperty);
            set => SetValue(ShowSplitterProperty, value);
        }

        public static readonly StyledProperty<Poptip?> LauncherPoptipProperty =
            AvaloniaProperty.Register<RibbonGroup, Poptip?>(nameof(LauncherPoptip));

        public Poptip? LauncherPoptip
        {
            get => GetValue(LauncherPoptipProperty);
            set => SetValue(LauncherPoptipProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var launcherButton = e.NameScope.Find<Button>(LauncherButton);
            if (launcherButton != null)
            {
                launcherButton.Click += LauncherButton_OnClick;
            }
        }

        private void LauncherButton_OnClick(object? sender, RoutedEventArgs e)
        {
            OnLauncherClick(new RoutedEventArgs(LauncherClickEvent, this));
        }

        protected virtual void OnLauncherClick(RoutedEventArgs e) => RaiseEvent(e);
    }
}
