using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace HandyControl.Controls;

// TODO: this control is a work-in-progress, it needs to support things like automatic resizing.
public class Ribbon : ItemsControl
{
    private const string TabHeaderItemsControl = "PART_TabHeaderItemsControl";

    private const string RootPanel = "PART_RootPanel";

    private const string ContentPanel = "PART_ContentPanel";

    private ItemsControl? _tabHeaderItemsControl;

    private Panel? _rootPanel;

    private Panel? _contentPanel;

    private Window? _window;

    private readonly ObservableCollection<object?> _tabHeaderItemsSource = new();

    internal ItemsControl? RibbonTabHeaderItemsControl => _tabHeaderItemsControl;

    static Ribbon()
    {
        SelectedIndexProperty.Changed.AddClassHandler<Ribbon>(OnSelectedIndexChanged);
        IsMinimizedProperty.Changed.AddClassHandler<Ribbon>(OnIsMinimizedChanged);
    }

    public Ribbon()
    {
        SetRibbon(this, this);

        Loaded += Ribbon_Loaded;
        Unloaded += Ribbon_Unloaded;
        Items.CollectionChanged += Items_CollectionChanged;
        ContainerPrepared += Ribbon_ContainerPrepared;
    }

    internal static readonly AttachedProperty<Ribbon?> RibbonProperty =
        AvaloniaProperty.RegisterAttached<Ribbon, AvaloniaObject, Ribbon?>("Ribbon", inherits: true);

    internal static void SetRibbon(AvaloniaObject element, Ribbon? value) =>
        element.SetValue(RibbonProperty, value);

    internal static Ribbon? GetRibbon(AvaloniaObject element) =>
        (Ribbon?) element.GetValue(RibbonProperty);

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<Ribbon, bool>(nameof(IsDropDownOpen), true);

    private static void OnIsDropDownOpenChanged(Ribbon d, AvaloniaPropertyChangedEventArgs e)
    {
        d.OnIsDropDownOpenChanged((bool) e.NewValue!);
    }

    private void OnIsDropDownOpenChanged(bool isDropDownOpen)
    {
        if (_contentPanel == null)
        {
            return;
        }

        SwitchCurrentTabContentVisibility(isDropDownOpen);
        _contentPanel.IsVisible = isDropDownOpen;
    }

    public bool IsDropDownOpen
    {
        get => (bool) GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly StyledProperty<bool> IsMinimizedProperty =
        AvaloniaProperty.Register<Ribbon, bool>(nameof(IsMinimized));

    public bool IsMinimized
    {
        get => (bool) GetValue(IsMinimizedProperty);
        set => SetValue(IsMinimizedProperty, value);
    }

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<Ribbon, int>(nameof(SelectedIndex), -1);

    private static void OnSelectedIndexChanged(Ribbon ribbon, AvaloniaPropertyChangedEventArgs e)
    {
        ribbon.SyncSelectedIndex((int) e.NewValue!);
    }

    public int SelectedIndex
    {
        get => (int) GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<Ribbon, object?>(nameof(SelectedItem));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static void OnIsMinimizedChanged(Ribbon ribbon, AvaloniaPropertyChangedEventArgs e)
    {
        ribbon.IsDropDownOpen = !(bool) e.NewValue!;
        if (!ribbon.IsDropDownOpen)
        {
            ribbon.SelectedIndex = -1;
        }
    }

    public static readonly StyledProperty<double> ContentHeightProperty =
        AvaloniaProperty.Register<Ribbon, double>(nameof(ContentHeight));

    public double ContentHeight
    {
        get => (double) GetValue(ContentHeightProperty);
        set => SetValue(ContentHeightProperty, value);
    }

    public static readonly StyledProperty<object?> PrefixContentProperty =
        AvaloniaProperty.Register<Ribbon, object?>(nameof(PrefixContent));

    public object? PrefixContent
    {
        get => GetValue(PrefixContentProperty);
        set => SetValue(PrefixContentProperty, value);
    }

    public static readonly StyledProperty<object?> PostfixContentProperty =
        AvaloniaProperty.Register<Ribbon, object?>(nameof(PostfixContent));

    public object? PostfixContent
    {
        get => GetValue(PostfixContentProperty);
        set => SetValue(PostfixContentProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _tabHeaderItemsControl = e.NameScope.Find<ItemsControl>(TabHeaderItemsControl);
        if (_tabHeaderItemsControl is { ItemsSource: null })
        {
            _tabHeaderItemsControl.ItemsSource = _tabHeaderItemsSource;
        }

        _rootPanel = e.NameScope.Find<Panel>(RootPanel);
        _contentPanel = e.NameScope.Find<Panel>(ContentPanel);

        if (IsMinimized && _rootPanel != null && _tabHeaderItemsControl != null)
        {
            _rootPanel.Height = _tabHeaderItemsControl.Bounds.Height;
        }

        if (!IsDropDownOpen && _contentPanel != null)
        {
            _contentPanel.Height = 0;
        }
    }

    internal void ResetSelection()
    {
        SelectedIndex = -1;
        InitializeSelection();
    }

    internal void NotifyMouseClickedOnTabHeader(RibbonTabHeader tabHeader, PointerPressedEventArgs e)
    {
        if (_tabHeaderItemsControl == null)
        {
            return;
        }

        var selectedIndex = _tabHeaderItemsControl.IndexFromContainer(tabHeader);

        if (e.ClickCount == 1)
        {
            var currentSelectedIndex = SelectedIndex;

            if (currentSelectedIndex < 0 || currentSelectedIndex != selectedIndex)
            {
                SelectedIndex = selectedIndex;

                if (IsMinimized)
                {
                    IsDropDownOpen = true;
                }
            }
            else
            {
                if (IsMinimized)
                {
                    IsDropDownOpen = !IsDropDownOpen;
                    if (!IsDropDownOpen)
                    {
                        SelectedIndex = -1;
                    }
                }
            }
        }
        else if (e.ClickCount == 2)
        {
            IsMinimized = !IsMinimized;
            IsDropDownOpen = !IsMinimized;

            if (IsMinimized && !IsDropDownOpen)
            {
                SelectedIndex = -1;
            }
            else
            {
                SelectedIndex = selectedIndex;
            }
        }
    }

    internal void NotifyTabHeaderChanged()
    {
        if (Items.Count <= 0 && _tabHeaderItemsSource.Count <= 0)
        {
            return;
        }

        RefreshHeaderCollection();
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Replace ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            InitializeSelection();
        }

        if ((e.Action != NotifyCollectionChangedAction.Move && e.Action != NotifyCollectionChangedAction.Remove) ||
            Items.Count <= 0)
        {
            RefreshHeaderCollection();
        }
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is RibbonTab)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new RibbonTab();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is RibbonTab ribbonTab)
        {
            ribbonTab.IsSelected = index == SelectedIndex;
        }
    }

    private void OnPreviewPointerButton(PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (!properties.IsLeftButtonPressed && !properties.IsRightButtonPressed)
        {
            return;
        }

        var position = e.GetPosition(this);
        if (this.InputHitTest(position) == null)
        {
            if (IsMinimized && IsDropDownOpen)
            {
                IsDropDownOpen = false;
                SelectedIndex = -1;
            }
        }
    }

    private void Ribbon_Loaded(object? sender, RoutedEventArgs e)
    {
        _window = TopLevel.GetTopLevel(this) as Window;
        if (_window != null)
        {
            _window.Deactivated += Window_Deactivated;
            _window.AddHandler(PointerPressedEvent, Window_PreviewPointerPressed, RoutingStrategies.Tunnel);
            _window.AddHandler(PointerReleasedEvent, Window_PreviewPointerReleased, RoutingStrategies.Tunnel);
        }

        if (IsMinimized && _rootPanel != null && _tabHeaderItemsControl != null)
        {
            _rootPanel.Height = _tabHeaderItemsControl.Bounds.Height;
        }
    }

    private void Ribbon_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (_window != null)
        {
            _window.Deactivated -= Window_Deactivated;
        }
    }

    private void Window_PreviewPointerPressed(object? sender, PointerPressedEventArgs e) => OnPreviewPointerButton(e);

    private void Window_PreviewPointerReleased(object? sender, PointerReleasedEventArgs e) => OnPreviewPointerButton(e);

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        if (IsMinimized && IsDropDownOpen)
        {
            IsDropDownOpen = false;
            SelectedIndex = -1;
        }
    }

    private void Ribbon_ContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        InitializeSelection();
        RefreshHeaderCollection();
    }

    private void SyncSelectedIndex(int newIndex)
    {
        var count = Items.Count;
        for (var index = 0; index < count; ++index)
        {
            if (ContainerFromIndex(index) is RibbonTab ribbonTab)
            {
                ribbonTab.IsSelected = index == newIndex;
            }
        }
    }

    private int GetFirstVisibleTabIndex()
    {
        var count = Items.Count;
        for (var index = 0; index < count; ++index)
        {
            if (ContainerFromIndex(index) is RibbonTab { IsVisible: true })
            {
                return index;
            }
        }

        return -1;
    }

    private void SwitchCurrentTabContentVisibility(bool isVisible)
    {
        var tab = GetCurrentTab();
        tab?.SwitchContentVisibility(isVisible);
    }

    private RibbonTab? GetCurrentTab()
    {
        var index = SelectedIndex;

        if (index == -1)
        {
            return null;
        }

        return ContainerFromIndex(index) as RibbonTab;
    }

    private void InitializeSelection()
    {
        if (!IsDropDownOpen)
        {
            SelectedIndex = -1;
            return;
        }

        if (SelectedIndex >= 0 || Items.Count <= 0)
        {
            return;
        }

        var firstVisibleTabIndex = GetFirstVisibleTabIndex();
        if (firstVisibleTabIndex < 0)
        {
            return;
        }

        SelectedIndex = firstVisibleTabIndex;
    }

    private void RefreshHeaderCollection()
    {
        var itemsCount = Items.Count;
        for (var index = 0; index < itemsCount; ++index)
        {
            object? header = null;
            if (ContainerFromIndex(index) is RibbonTab ribbonTab)
            {
                header = ribbonTab.Header;
            }

            header ??= string.Empty;

            if (index >= _tabHeaderItemsSource.Count)
            {
                _tabHeaderItemsSource.Add(header);
            }
            else
            {
                _tabHeaderItemsSource[index] = header;
            }
        }

        var headerCount = _tabHeaderItemsSource.Count;
        for (var index = 0; index < headerCount - itemsCount; ++index)
        {
            _tabHeaderItemsSource.RemoveAt(itemsCount);
        }
    }
}
