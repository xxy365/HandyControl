using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace HandyControl.Controls
{
    [TemplatePart(RootContainer, typeof(Control))]
    public class RibbonTab : HeaderedItemsControl
    {
        private const string RootContainer = "PART_RootContainer";

        private Control? _rootContainer;

        static RibbonTab()
        {
            IsSelectedProperty.Changed.AddClassHandler<RibbonTab>(OnIsSelectedChanged);
            IsVisibleProperty.Changed.AddClassHandler<RibbonTab>(OnVisibilityChanged);
            HeaderProperty.Changed.AddClassHandler<RibbonTab>(OnHeaderChanged);
        }

        public static readonly RoutedEvent<RoutedEventArgs> SelectedEvent =
            RoutedEvent.Register<RibbonTab, RoutedEventArgs>(nameof(Selected), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> UnselectedEvent =
            RoutedEvent.Register<RibbonTab, RoutedEventArgs>(nameof(Unselected), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs>? Selected
        {
            add => AddHandler(SelectedEvent, value);
            remove => RemoveHandler(SelectedEvent, value);
        }

        public event EventHandler<RoutedEventArgs>? Unselected
        {
            add => AddHandler(UnselectedEvent, value);
            remove => RemoveHandler(UnselectedEvent, value);
        }

        public Ribbon? Ribbon => Ribbon.GetRibbon(this);

        internal RibbonTabHeader? RibbonTabHeader
        {
            get
            {
                var ribbon = Ribbon;
                if (ribbon == null)
                {
                    return null;
                }

                var index = ribbon.IndexFromContainer(this);
                if (index < 0)
                {
                    return null;
                }

                var headerItemsControl = ribbon.RibbonTabHeaderItemsControl;
                return headerItemsControl?.ContainerFromIndex(index) as RibbonTabHeader;
            }
        }

        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<RibbonTab, bool>(nameof(IsSelected), defaultBindingMode: BindingMode.TwoWay);

        private static void OnIsSelectedChanged(RibbonTab ribbonTab, AvaloniaPropertyChangedEventArgs e)
        {
            if (ribbonTab.IsSelected)
            {
                if (ribbonTab.Ribbon?.IsDropDownOpen == true)
                {
                    ribbonTab.SwitchContentVisibility(true);
                }

                ribbonTab.OnSelected(new RoutedEventArgs(SelectedEvent, ribbonTab));
                ribbonTab.SyncSelection();
            }
            else
            {
                ribbonTab.SwitchContentVisibility(false);
                ribbonTab.OnUnselected(new RoutedEventArgs(UnselectedEvent, ribbonTab));
            }

            ribbonTab.RibbonTabHeader?.PrepareRibbonTabHeader();
        }

        public bool IsSelected
        {
            get => (bool) GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _rootContainer = e.NameScope.Find<Control>(RootContainer);

            SwitchContentVisibility(IsSelected);
        }

        internal void SwitchContentVisibility(bool isVisible)
        {
            if (_rootContainer != null)
            {
                _rootContainer.IsVisible = isVisible;
            }
        }

        protected virtual void OnSelected(RoutedEventArgs e) => RaiseEvent(e);

        protected virtual void OnUnselected(RoutedEventArgs e) => RaiseEvent(e);

        protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            if (item is RibbonGroup)
            {
                recycleKey = null;
                return false;
            }

            recycleKey = DefaultRecycleKey;
            return true;
        }

        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return new RibbonGroup();
        }

        internal void SyncSelection()
        {
            var ribbon = Ribbon;
            if (ribbon == null)
            {
                return;
            }

            var index = ribbon.IndexFromContainer(this);
            if (index >= 0 && index != ribbon.SelectedIndex)
            {
                ribbon.SelectedIndex = index;
            }
        }

        private static void OnVisibilityChanged(RibbonTab ribbonTab, AvaloniaPropertyChangedEventArgs e)
        {
            ribbonTab.RibbonTabHeader?.PrepareRibbonTabHeader();

            var ribbon = ribbonTab.Ribbon;
            if (ribbon == null || !ribbonTab.IsSelected)
            {
                return;
            }

            if ((bool) e.OldValue! && !(bool) e.NewValue!)
            {
                ribbon.ResetSelection();
            }
        }

        private static void OnHeaderChanged(RibbonTab ribbonTab, AvaloniaPropertyChangedEventArgs e)
        {
            ribbonTab.Ribbon?.NotifyTabHeaderChanged();
        }
    }
}
