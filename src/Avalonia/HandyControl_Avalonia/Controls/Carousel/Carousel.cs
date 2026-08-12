using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;

namespace HandyControl.Controls;

public class Carousel : TemplatedControl
{
    private const string ElementPanel = "PART_Panel";
    private const string ElementPanelPage = "PART_PanelPage";

    private readonly List<double> _widthList = new();
    private readonly Dictionary<object, CarouselItem> _entryDic = new();

    private Panel? _panel;
    private StackPanel? _panelPage;
    private bool _appliedTemplate;
    private int _pageIndex = -1;
    private RadioButton? _selectedButton;
    private DispatcherTimer? _updateTimer;
    private IEnumerable? _itemsSourceInternal;
    private Collection<object>? _items;

    static Carousel()
    {
        FocusableProperty.OverrideDefaultValue<Carousel>(false);
        AutoRunProperty.Changed.AddClassHandler<Carousel>((c, e) => c.TimerSwitch(e.GetNewValue<bool>()));
    }

    public static readonly StyledProperty<bool> AutoRunProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(AutoRun));

    public static readonly StyledProperty<TimeSpan> IntervalProperty =
        AvaloniaProperty.Register<Carousel, TimeSpan>(nameof(Interval), TimeSpan.FromSeconds(2));

    public static readonly StyledProperty<double> ExtendWidthProperty =
        AvaloniaProperty.Register<Carousel, double>(nameof(ExtendWidth));

    public static readonly StyledProperty<bool> IsCenterProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(IsCenter));

    public static readonly StyledProperty<IStyle?> PageButtonStyleProperty =
        AvaloniaProperty.Register<Carousel, IStyle?>(nameof(PageButtonStyle));

    public static readonly StyledProperty<IStyle?> ItemContainerStyleProperty =
        AvaloniaProperty.Register<Carousel, IStyle?>(nameof(ItemContainerStyle));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<Carousel, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<Carousel, IEnumerable?>(nameof(ItemsSource));

    public bool AutoRun
    {
        get => GetValue(AutoRunProperty);
        set => SetValue(AutoRunProperty, value);
    }

    public TimeSpan Interval
    {
        get => GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    public double ExtendWidth
    {
        get => GetValue(ExtendWidthProperty);
        set => SetValue(ExtendWidthProperty, value);
    }

    public bool IsCenter
    {
        get => GetValue(IsCenterProperty);
        set => SetValue(IsCenterProperty, value);
    }

    public IStyle? PageButtonStyle
    {
        get => GetValue(PageButtonStyleProperty);
        set => SetValue(PageButtonStyleProperty, value);
    }

    public IStyle? ItemContainerStyle
    {
        get => GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    ///     页码
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            if (_items == null || _items.Count == 0) return;
            if (_pageIndex == value) return;

            if (value < 0)
                _pageIndex = _items.Count - 1;
            else if (value >= _items.Count)
                _pageIndex = 0;
            else
                _pageIndex = value;

            UpdatePageButtons(_pageIndex);
        }
    }

    public bool HasItems => _items is { Count: > 0 };

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _appliedTemplate = false;

        if (_panelPage != null)
        {
            _panelPage.Children.Clear();
        }

        base.OnApplyTemplate(e);

        _panelPage = e.NameScope.Find<StackPanel>(ElementPanelPage);
        _panel = e.NameScope.Find<Panel>(ElementPanel);

        if (_panelPage == null || _panel == null)
        {
            return;
        }

        _panelPage.AddHandler(Button.ClickEvent, ButtonPages_OnClick);
        _panel.Transitions!.Add(new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(300) });

        if (e.NameScope.Find<Button>("PART_Prev") is { } prevButton)
        {
            prevButton.Click += ButtonPrev_OnClick;
        }
        if (e.NameScope.Find<Button>("PART_Next") is { } nextButton)
        {
            nextButton.Click += ButtonNext_OnClick;
        }

        _appliedTemplate = true;

        ClearItems();
        Refresh();

        Update();
    }

    private void Update()
    {
        TimerSwitch(AutoRun);
        UpdatePageButtons(_pageIndex);
    }

    /// <summary>
    ///     计时器开关
    /// </summary>
    private void TimerSwitch(bool run)
    {
        if (!_appliedTemplate) return;

        if (_updateTimer != null)
        {
            _updateTimer.Stop();
            _updateTimer = null;
        }

        if (!run) return;

        _updateTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = Interval
        };
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (IsPointerOver) return;
        PageIndex++;
    }

    /// <summary>
    ///     更新页按钮
    /// </summary>
    public void UpdatePageButtons(int index = -1)
    {
        if (!CheckNull()) return;
        if (!_appliedTemplate) return;

        var count = _items?.Count ?? 0;

        _widthList.Clear();
        _widthList.Add(0);

        var width = 0.0;
        foreach (var child in _panel!.Children)
        {
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            width += child.DesiredSize.Width;
            _widthList.Add(width);
        }

        _panel.Width = _widthList.Last() + ExtendWidth;

        _panelPage!.Children.Clear();
        for (var i = 0; i < count; i++)
        {
            var radio = new RadioButton();
            if (PageButtonStyle != null)
            {
                radio.Styles.Add(PageButtonStyle);
            }
            _panelPage.Children.Add(radio);
        }

        if (index == -1 && count > 0) index = 0;
        if (index >= 0 && index < count && _panelPage.Children[index] is RadioButton button)
        {
            button.IsChecked = true;
            UpdateItemsPosition();
        }
    }

    /// <summary>
    ///     更新项的位置
    /// </summary>
    private void UpdateItemsPosition()
    {
        if (!CheckNull()) return;
        if (!_appliedTemplate) return;
        if (_items == null || _items.Count == 0) return;
        if (_panel == null) return;
        if (_pageIndex < 0 || _pageIndex >= _widthList.Count) return;

        double offset;
        if (!IsCenter)
        {
            offset = -_widthList[_pageIndex];
        }
        else
        {
            var ctl = _panel.Children[_pageIndex];
            var ctlWidth = ctl.DesiredSize.Width;
            offset = -_widthList[_pageIndex] + (Bounds.Width - ctlWidth) / 2;
        }

        _panel.Margin = new Thickness(offset, 0, 0, 0);
    }

    protected override void OnSizeChanged(Avalonia.Controls.SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateItemsPosition();
    }

    private void ButtonPages_OnClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not RadioButton rb) return;
        if (!CheckNull()) return;

        _selectedButton = rb;

        var index = _panelPage!.Children.IndexOf(_selectedButton);
        if (index != -1)
        {
            PageIndex = index;
        }
    }

    private void ButtonPrev_OnClick(object? sender, RoutedEventArgs e) => PageIndex--;

    private void ButtonNext_OnClick(object? sender, RoutedEventArgs e) => PageIndex++;

    private void ClearItems()
    {
        _pageIndex = -1;
        _widthList.Clear();
        _entryDic.Clear();
        if (_panel != null)
        {
            _panel.Children.Clear();
        }
    }

    private void Refresh()
    {
        if (_panel == null || _items == null) return;

        _panel.Children.Clear();
        _entryDic.Clear();

        foreach (var item in _items)
        {
            AddItem(item);
        }
    }

    private void AddItem(object item)
    {
        if (_panel == null || _entryDic.ContainsKey(item)) return;

        var element = new CarouselItem
        {
            Content = item,
            ContentTemplate = ItemTemplate
        };
        if (ItemContainerStyle != null)
        {
            element.Styles.Add(ItemContainerStyle);
        }

        _entryDic[item] = element;
        _panel.Children.Add(element);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            ItemsSourceChanged(change.GetOldValue<IEnumerable>(), change.GetNewValue<IEnumerable>());
        }
        else if (change.Property == ItemTemplateProperty || change.Property == ItemContainerStyleProperty)
        {
            Refresh();
        }
    }

    private void ItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
    {
        if (oldValue is INotifyCollectionChanged oldNotify)
        {
            oldNotify.CollectionChanged -= InternalCollectionChanged;
        }

        ClearItems();
        _itemsSourceInternal = newValue;

        if (newValue is INotifyCollectionChanged newNotify)
        {
            newNotify.CollectionChanged += InternalCollectionChanged;
        }

        if (newValue != null)
        {
            var items = new ObservableCollection<object>();
            foreach (var item in newValue)
            {
                items.Add(item);
            }
            _items = items;
        }
        else
        {
            _items = null;
        }

        Refresh();
        Update();
    }

    private void InternalCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_panel == null || _items == null) return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                ClearItems();
                Refresh();
                break;
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AddItem(item);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        RemoveItem(item);
                    }
                }
                break;
        }

        UpdatePageButtons(_pageIndex);
    }

    private void RemoveItem(object item)
    {
        if (_panel == null) return;
        if (!_entryDic.TryGetValue(item, out var entry)) return;

        _panel.Children.Remove(entry);
        _entryDic.Remove(item);
        _items?.Remove(item);
    }

    private bool CheckNull() => _panel != null && _panelPage != null;
}