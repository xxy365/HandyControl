using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using HandyControl.Data;
using HandyControl.Tools;

namespace HandyControl.Controls;

public class RangeSlider : TwoWayRangeBase
{
    private const string ElementTrack = "PART_Track";

    private RangeTrack? _track;

    private RangeThumb? _thumbCurrent;

    private Point _originThumbPoint;

    static RangeSlider()
    {
        AffectsMeasure<RangeSlider>(MinimumProperty, MaximumProperty, ValueStartProperty, ValueEndProperty);

        Thumb.DragStartedEvent.AddClassHandler<RangeSlider>((x, e) => x.OnThumbDragStarted(e), RoutingStrategies.Bubble);
        Thumb.DragDeltaEvent.AddClassHandler<RangeSlider>((x, e) => x.OnThumbDragDelta(e), RoutingStrategies.Bubble);
        Thumb.DragCompletedEvent.AddClassHandler<RangeSlider>((x, e) => x.OnThumbDragCompleted(e), RoutingStrategies.Bubble);
    }

    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<RangeSlider, Orientation>(
        nameof(Orientation));

    public static readonly StyledProperty<bool> IsDirectionReversedProperty = AvaloniaProperty.Register<RangeSlider, bool>(
        nameof(IsDirectionReversed));

    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty = AvaloniaProperty.Register<RangeSlider, bool>(
        nameof(IsSnapToTickEnabled));

    public static readonly StyledProperty<double> TickFrequencyProperty = AvaloniaProperty.Register<RangeSlider, double>(
        nameof(TickFrequency), 1.0);

    public static readonly StyledProperty<TickPlacement> TickPlacementProperty =
        AvaloniaProperty.Register<RangeSlider, TickPlacement>(nameof(TickPlacement));

    public static readonly StyledProperty<AvaloniaList<double>?> TicksProperty =
        TickBar.TicksProperty.AddOwner<RangeSlider>();

    public TickPlacement TickPlacement
    {
        get => GetValue(TickPlacementProperty);
        set => SetValue(TickPlacementProperty, value);
    }

    public AvaloniaList<double>? Ticks
    {
        get => GetValue(TicksProperty);
        set => SetValue(TicksProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    public bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    public double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _thumbCurrent = null;

        base.OnApplyTemplate(e);

        _track = e.NameScope.Find<RangeTrack>(ElementTrack);

        if (_track != null)
        {
            if (_track.DecreaseRepeatButton != null)
            {
                _track.DecreaseRepeatButton.Click += OnDecreaseClick;
            }

            if (_track.CenterRepeatButton != null)
            {
                _track.CenterRepeatButton.Click += OnCenterClick;
            }

            if (_track.IncreaseRepeatButton != null)
            {
                _track.IncreaseRepeatButton.Click += OnIncreaseClick;
            }
        }
    }

    private void OnIncreaseClick(object? sender, RoutedEventArgs e) => MoveToNextTick(LargeChange, false);

    private void OnDecreaseClick(object? sender, RoutedEventArgs e) => MoveToNextTick(-LargeChange, true);

    private void OnCenterClick(object? sender, RoutedEventArgs e)
    {
        var isStart = (ValueStart + ValueEnd) / 2 > ValueStart;
        MoveToNextTick(LargeChange, isStart);
    }

    private void MoveToNextTick(double direction, bool isStart)
    {
        if (MathHelper.AreClose(direction, 0)) return;

        var value = isStart ? ValueStart : ValueEnd;
        var next = SnapToTick(Math.Max(Minimum, Math.Min(Maximum, value + direction)));
        var greaterThan = direction > 0;

        if (MathHelper.AreClose(next, value) &&
            !(greaterThan && MathHelper.AreClose(value, Maximum)) &&
            !(!greaterThan && MathHelper.AreClose(value, Minimum)))
        {
            if (MathHelper.GreaterThan(TickFrequency, 0.0))
            {
                var tickNumber = Math.Round((value - Minimum) / TickFrequency);

                if (greaterThan)
                    tickNumber += 1.0;
                else
                    tickNumber -= 1.0;

                next = Minimum + tickNumber * TickFrequency;
            }
        }

        if (!MathHelper.AreClose(next, value))
        {
            SetCurrentValue(isStart ? ValueStartProperty : ValueEndProperty, next);
        }
    }

    private double SnapToTick(double value)
    {
        if (!IsSnapToTickEnabled) return value;

        var previous = Minimum;
        var next = Maximum;

        if (MathHelper.GreaterThan(TickFrequency, 0.0))
        {
            previous = Minimum + Math.Round((value - Minimum) / TickFrequency) * TickFrequency;
            next = Math.Min(Maximum, previous + TickFrequency);
        }

        return MathHelper.GreaterThanOrClose(value, (previous + next) * 0.5) ? next : previous;
    }

    private void OnThumbDragStarted(VectorEventArgs e)
    {
        if (e.Source is not RangeThumb thumb) return;

        _thumbCurrent = thumb;
        _originThumbPoint = new Point(e.Vector.X, e.Vector.Y);
    }

    private void OnThumbDragDelta(VectorEventArgs e)
    {
        if (_track == null || _thumbCurrent == null) return;

        var isStart = ReferenceEquals(_thumbCurrent, _track.ThumbStart);
        var delta = _track.ValueFromDistance(e.Vector.X, e.Vector.Y);
        UpdateValue((isStart ? ValueStart : ValueEnd) + delta, isStart);
    }

    private void OnThumbDragCompleted(VectorEventArgs e)
    {
        _thumbCurrent = null;
    }

    private void UpdateValue(double value, bool isStart)
    {
        var snappedValue = SnapToTick(value);

        if (isStart)
        {
            if (!MathHelper.AreClose(snappedValue, ValueStart))
            {
                var start = Math.Max(Minimum, Math.Min(Maximum, snappedValue));
                if (start > ValueEnd)
                {
                    SetCurrentValue(ValueStartProperty, ValueEnd);
                    SetCurrentValue(ValueEndProperty, start);
                    _thumbCurrent = _track?.ThumbEnd;
                }
                else
                {
                    SetCurrentValue(ValueStartProperty, start);
                }
            }
        }
        else
        {
            if (!MathHelper.AreClose(snappedValue, ValueEnd))
            {
                var end = Math.Max(Minimum, Math.Min(Maximum, snappedValue));
                if (end < ValueStart)
                {
                    SetCurrentValue(ValueEndProperty, ValueStart);
                    SetCurrentValue(ValueStartProperty, end);
                    _thumbCurrent = _track?.ThumbStart;
                }
                else
                {
                    SetCurrentValue(ValueEndProperty, end);
                }
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_track == null || !IsEnabled) return;

        var pt = e.GetPosition(_track);
        var newValue = _track.ValueFromPoint(pt);
        UpdateValue(newValue, (ValueStart + ValueEnd) / 2 > newValue);
        e.Handled = true;
    }
}