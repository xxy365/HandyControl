using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using HandyControl.Data;
using HandyControl.Properties.Langs;
using HandyControl.Tools;

namespace HandyControl.Controls;

public class TimeBar : TemplatedControl
{
    private const string ElementBorderTop = "PART_BorderTop";

    private const string ElementTextBlockMove = "PART_TextBlockMove";

    private const string ElementTextBlockSelected = "PART_TextBlockSelected";

    private const string ElementTextBlockSpeStr = "PART_TextBlockSpeStr";

    private const string ElementCanvasSpe = "PART_CanvasSpe";

    private const string ElementHotspots = "PART_Hotspots";

    private Border? _borderTop;

    private TextBlock? _textBlockMove;

    private TextBlock? _textBlockSelected;

    private TextBlock? _textBlockSpeStr;

    private Canvas? _canvasSpe;

    private Panel? _panelHotspots;

    private Point _mousePoint = new(double.NaN, double.NaN);

    private bool _borderTopIsMouseLeftButtonDown;

    private bool _isDragging;

    private double _itemWidth;

    private double _dragStartX;

    private DateTime _mouseDownTime;

    private int _speCount = 13;

    private int _speIndex = 1;

    private double _tempOffsetX;

    private double _totalOffsetX;

    private readonly bool _isLoaded;

    private readonly DateTime _starTime;

    private readonly List<SpeTextBlock> _speBlockList = new();

    private readonly List<int> _timeSpeList = new()
    {
        7200000,
        3600000,
        1800000,
        600000,
        300000,
        60000,
        30000
    };

    private readonly SortedSet<DateTimeRange> _dateTimeRanges;

    public ObservableCollection<DateTimeRange> Hotspots { get; }

    public static readonly StyledProperty<IBrush?> HotspotsBrushProperty =
        AvaloniaProperty.Register<TimeBar, IBrush?>(nameof(HotspotsBrush));

    public IBrush? HotspotsBrush
    {
        get => GetValue(HotspotsBrushProperty);
        set => SetValue(HotspotsBrushProperty, value);
    }

    public static readonly StyledProperty<bool> ShowSpeStrProperty =
        AvaloniaProperty.Register<TimeBar, bool>(nameof(ShowSpeStr), false);

    public bool ShowSpeStr
    {
        get => GetValue(ShowSpeStrProperty);
        set => SetValue(ShowSpeStrProperty, value);
    }

    public static readonly StyledProperty<string> TimeFormatProperty =
        AvaloniaProperty.Register<TimeBar, string>(nameof(TimeFormat), "yyyy-MM-dd HH:mm:ss");

    public string TimeFormat
    {
        get => GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }

    public static readonly StyledProperty<string> SpeStrProperty =
        AvaloniaProperty.Register<TimeBar, string>(nameof(SpeStr), Lang.Interval1h);

    public string SpeStr
    {
        get => GetValue(SpeStrProperty);
        internal set => SetValue(SpeStrProperty, value);
    }

    public static readonly StyledProperty<DateTime> SelectedTimeProperty =
        AvaloniaProperty.Register<TimeBar, DateTime>(nameof(SelectedTime), DateTime.Now);

    public DateTime SelectedTime
    {
        get => GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    private static void OnSelectedTimeChanged(TimeBar timeBar, AvaloniaPropertyChangedEventArgs e)
    {
        if (timeBar._textBlockSelected != null)
        {
            timeBar.OnSelectedTimeChanged((DateTime)e.NewValue!);
        }
    }

    private void OnSelectedTimeChanged(DateTime time)
    {
        if (_textBlockSelected != null)
        {
            _textBlockSelected.Text = time.ToString(TimeFormat);
        }

        if (!(_isDragging || _borderTopIsMouseLeftButtonDown))
        {
            _totalOffsetX = (_starTime - SelectedTime).TotalMilliseconds / _timeSpeList[_speIndex] * _itemWidth;
        }

        UpdateSpeBlock();
        UpdateMouseFollowBlockPos();
    }

    public static readonly RoutedEvent<FunctionEventArgs<DateTime>> TimeChangedEvent =
        RoutedEvent.Register<TimeBar, FunctionEventArgs<DateTime>>(nameof(TimeChanged), RoutingStrategies.Bubble);

    public event EventHandler<FunctionEventArgs<DateTime>> TimeChanged
    {
        add => AddHandler(TimeChangedEvent, value);
        remove => RemoveHandler(TimeChangedEvent, value);
    }

    public TimeBar()
    {
        _starTime = DateTime.Now;
        SelectedTime = new DateTime(_starTime.Year, _starTime.Month, _starTime.Day, 0, 0, 0);
        _starTime = SelectedTime;
        _isLoaded = true;

        var hotspots = new ObservableCollection<DateTimeRange>();
        _dateTimeRanges = new SortedSet<DateTimeRange>(new DateTimeRangeComparer());
        hotspots.CollectionChanged += Items_CollectionChanged;
        Hotspots = hotspots;

        SelectedTimeProperty.Changed.AddClassHandler<TimeBar>(OnSelectedTimeChanged);
        ShowSpeStrProperty.Changed.AddClassHandler<TimeBar>(OnShowSpeStrChanged);
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (DateTimeRange item in e.NewItems!)
                {
                    _dateTimeRanges.Add(item);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (DateTimeRange item in e.OldItems!)
                {
                    _dateTimeRanges.Remove(item);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (DateTimeRange item in e.OldItems!)
                {
                    _dateTimeRanges.Remove(item);
                }
                foreach (DateTimeRange item in e.NewItems!)
                {
                    _dateTimeRanges.Add(item);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                _dateTimeRanges.Clear();
                break;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_borderTop != null)
        {
            _borderTop.PointerPressed -= BorderTop_OnPointerPressed;
            _borderTop.PointerMoved -= BorderTop_OnPointerMoved;
            _borderTop.PointerReleased -= BorderTop_OnPointerReleased;
            _borderTop.PointerEntered -= BorderTop_OnPointerEntered;
            _borderTop.PointerExited -= BorderTop_OnPointerExited;
        }

        base.OnApplyTemplate(e);

        _borderTop = e.NameScope.Find<Border>(ElementBorderTop);
        _textBlockMove = e.NameScope.Find<TextBlock>(ElementTextBlockMove);
        _textBlockSelected = e.NameScope.Find<TextBlock>(ElementTextBlockSelected);
        _textBlockSpeStr = e.NameScope.Find<TextBlock>(ElementTextBlockSpeStr);
        _canvasSpe = e.NameScope.Find<Canvas>(ElementCanvasSpe);
        _panelHotspots = e.NameScope.Find<Panel>(ElementHotspots);

        if (_borderTop == null || _textBlockMove == null || _textBlockSelected == null || _textBlockSpeStr == null || _canvasSpe == null)
        {
            throw new InvalidOperationException("TimeBar template parts not found.");
        }

        _borderTop.Cursor = new Cursor(StandardCursorType.Hand);
        _borderTop.PointerPressed += BorderTop_OnPointerPressed;
        _borderTop.PointerMoved += BorderTop_OnPointerMoved;
        _borderTop.PointerReleased += BorderTop_OnPointerReleased;
        _borderTop.PointerEntered += BorderTop_OnPointerEntered;
        _borderTop.PointerExited += BorderTop_OnPointerExited;

        if (_isLoaded)
        {
            Update();
        }

        _textBlockSelected.Text = SelectedTime.ToString(TimeFormat);
    }

    private static void OnShowSpeStrChanged(TimeBar timeBar, AvaloniaPropertyChangedEventArgs e)
    {
        if (timeBar._textBlockSpeStr != null)
        {
            timeBar._textBlockSpeStr.IsVisible = (bool)e.NewValue!;
        }
    }

    private void BorderTop_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_textBlockMove != null)
        {
            _textBlockMove.IsVisible = true;
        }
    }

    private void BorderTop_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_textBlockMove != null)
        {
            _textBlockMove.IsVisible = false;
        }
    }

    public int SpeIndex
    {
        get => _speIndex;
        private set
        {
            if (_speIndex == value)
            {
                return;
            }

            if (value < 0)
            {
                SpeStr = Lang.Interval2h;
                _speIndex = 0;
                return;
            }

            if (value > 6)
            {
                SpeStr = Lang.Interval30s;
                _speIndex = 6;
                return;
            }

            SetSpeTimeFormat("HH:mm");
            switch (value)
            {
                case 0:
                    SpeStr = Lang.Interval2h;
                    break;
                case 1:
                    SpeStr = Lang.Interval1h;
                    break;
                case 2:
                    SpeStr = Lang.Interval30m;
                    break;
                case 3:
                    SpeStr = Lang.Interval10m;
                    break;
                case 4:
                    SpeStr = Lang.Interval5m;
                    break;
                case 5:
                    SpeStr = Lang.Interval1m;
                    break;
                case 6:
                    SetSpeTimeFormat("HH:mm:ss");
                    SpeStr = Lang.Interval30s;
                    break;
            }

            _speIndex = value;
        }
    }

    private void SetSpeTimeFormat(string format)
    {
        foreach (var item in _speBlockList)
        {
            item.TimeFormat = format;
        }
    }

    private void UpdateSpeBlock()
    {
        var rest = (_totalOffsetX + _tempOffsetX) % _itemWidth;
        if (double.IsNaN(rest) || double.IsInfinity(rest))
        {
            rest = 0;
        }

        for (var i = 0; i < _speCount; i++)
        {
            var item = _speBlockList[i];
            item.MoveX(rest + (_itemWidth - item.Width) / 2);
        }

        var sub = rest <= 0 ? _speCount / 2 : _speCount / 2 - 1;

        for (var i = 0; i < _speCount; i++)
        {
            _speBlockList[i].Time = TimeConvert(SelectedTime).AddMilliseconds((i - sub) * _timeSpeList[_speIndex]);
        }

        if (_panelHotspots != null && _dateTimeRanges.Count > 0)
        {
            UpdateHotspots();
        }
    }

    private DateTime TimeConvert(DateTime time)
    {
        return _speIndex switch
        {
            0 => new DateTime(time.Year, time.Month, time.Day, time.Hour / 2 * 2, 0, 0),
            1 => new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0),
            2 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 30 * 30, 0),
            3 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 10 * 10, 0),
            4 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 5 * 5, 0),
            5 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0),
            6 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second / 30 * 30),
            _ => time
        };
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (_borderTopIsMouseLeftButtonDown)
        {
            return;
        }

        SpeIndex += e.Delta.Y > 0 ? 1 : -1;
        _totalOffsetX = (_starTime - SelectedTime).TotalMilliseconds / _timeSpeList[SpeIndex] * _itemWidth;
        if (double.IsNaN(_totalOffsetX) || double.IsInfinity(_totalOffsetX))
        {
            _totalOffsetX = 0;
        }

        UpdateSpeBlock();
        UpdateMouseFollowBlockPos();
        e.Handled = true;
    }

    private void BorderTop_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(_borderTop).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _borderTopIsMouseLeftButtonDown = true;
        _isDragging = false;
        _mouseDownTime = SelectedTime;
        _dragStartX = e.GetPosition(this).X;
        _mousePoint = e.GetPosition(this);
        e.Pointer.Capture(_borderTop);
        e.Handled = true;
    }

    private void BorderTop_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var p = e.GetPosition(this);
        _mousePoint = p;

        if (_borderTopIsMouseLeftButtonDown)
        {
            _isDragging = true;
            var offset = p.X - _dragStartX;
            _borderTop!.RenderTransform = new TranslateTransform(offset, 0);
            _tempOffsetX = offset;
            SelectedTime = _mouseDownTime - TimeSpan.FromMilliseconds(offset / _itemWidth * _timeSpeList[_speIndex]);
        }

        UpdateMouseFollowBlockPos();
    }

    private void BorderTop_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_borderTopIsMouseLeftButtonDown)
        {
            return;
        }

        var p = e.GetPosition(this);
        _mousePoint = p;

        if (_isDragging)
        {
            var offset = (_borderTop!.RenderTransform as TranslateTransform)?.X ?? 0;
            _tempOffsetX = 0;
            _totalOffsetX = (_totalOffsetX + offset) % Bounds.Width;
            _borderTop!.RenderTransform = null;
            _isDragging = false;
            RaiseTimeChanged();
        }
        else
        {
            _tempOffsetX = Bounds.Width / 2 - p.X;
            SelectedTime -= TimeSpan.FromMilliseconds(_tempOffsetX / _itemWidth * _timeSpeList[_speIndex]);
            _totalOffsetX = (_totalOffsetX + _tempOffsetX) % Bounds.Width;
            _tempOffsetX = 0;
            UpdateMouseFollowBlockPos();
            RaiseTimeChanged();
        }

        _borderTopIsMouseLeftButtonDown = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _mousePoint = e.GetPosition(this);
        UpdateMouseFollowBlockPos();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Update();
    }

    private void Update()
    {
        if (_canvasSpe == null || Bounds.Width < 1)
        {
            return;
        }

        _speBlockList.Clear();
        _canvasSpe.Children.Clear();
        _speCount = (int)(Bounds.Width / 800 * 9) | 1;

        var itemWidthOld = _itemWidth;
        _itemWidth = Bounds.Width / _speCount;
        if (itemWidthOld > 0)
        {
            _totalOffsetX = _itemWidth / itemWidthOld * _totalOffsetX % Bounds.Width;
        }

        if (double.IsNaN(_totalOffsetX) || double.IsInfinity(_totalOffsetX))
        {
            _totalOffsetX = 0;
        }

        var rest = (_totalOffsetX + _tempOffsetX) % _itemWidth;
        var sub = rest <= 0 || double.IsNaN(rest) ? _speCount / 2 : _speCount / 2 - 1;

        for (var i = 0; i < _speCount; i++)
        {
            var block = new SpeTextBlock
            {
                Time = TimeConvert(SelectedTime).AddMilliseconds((i - sub) * _timeSpeList[_speIndex]),
                TextAlignment = TextAlignment.Center,
                TimeFormat = "HH:mm"
            };
            _speBlockList.Add(block);
            _canvasSpe.Children.Add(block);
        }

        if (_speIndex == 6)
        {
            SetSpeTimeFormat("HH:mm:ss");
        }

        ShowSpeStr = Bounds.Width > 320;
        for (var i = 0; i < _speCount; i++)
        {
            var item = _speBlockList[i];
            item.X = _itemWidth * i;
            item.MoveX((_itemWidth - item.Width) / 2);
        }

        UpdateSpeBlock();
        UpdateMouseFollowBlockPos();
    }

    private void UpdateMouseFollowBlockPos()
    {
        if (_textBlockMove == null || double.IsNaN(_mousePoint.X))
        {
            return;
        }

        var p = _mousePoint;
        var milliseconds = (p.X - Bounds.Width / 2) / _itemWidth * _timeSpeList[_speIndex];
        if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds))
        {
            return;
        }

        _textBlockMove.Text = milliseconds < 0
            ? (SelectedTime - TimeSpan.FromMilliseconds(-milliseconds)).ToString(TimeFormat)
            : (SelectedTime + TimeSpan.FromMilliseconds(milliseconds)).ToString(TimeFormat);
        _textBlockMove.Margin = new Thickness(p.X - _textBlockMove.Bounds.Width / 2, 2, 0, 0);
    }

    private void UpdateHotspots()
    {
        var milliseconds = Bounds.Width * 0.5 / _itemWidth * _timeSpeList[_speIndex];
        if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds))
        {
            return;
        }

        _panelHotspots!.Children.Clear();

        foreach (var rect in GetHotspotsRectangle(milliseconds))
        {
            _panelHotspots.Children.Add(rect);
        }

        _panelHotspots.IsVisible = _panelHotspots.Children.Count > 0;
    }

    private IEnumerable<Rectangle> GetHotspotsRectangle(double milliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        var selectedTime = SelectedTime;
        var start = selectedTime - timeSpan;
        var end = selectedTime + timeSpan;

        var set = _dateTimeRanges.GetViewBetween(new DateTimeRange(start), new DateTimeRange(end));
        var unitLength = Bounds.Width / milliseconds * 0.5;

        foreach (var range in set)
        {
            var width = range.TotalMilliseconds * unitLength;
            var sub = range.Start - start;
            var x = sub.TotalMilliseconds * unitLength;
            yield return new Rectangle
            {
                Fill = HotspotsBrush,
                Height = 4,
                Width = width,
                Margin = new Thickness(x, 0, 0, 0),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
        }
    }

    private void RaiseTimeChanged()
    {
        RaiseEvent(new FunctionEventArgs<DateTime>(TimeChangedEvent, this)
        {
            Info = SelectedTime
        });
    }
}