using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using HandyControl.Data;

namespace HandyControl.Controls;

public class TwoWayRangeBase : TemplatedControl
{
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<TwoWayRangeBase, double>(
        nameof(Minimum), 0.0);

    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<TwoWayRangeBase, double>(
        nameof(Maximum), 10.0);

    public static readonly StyledProperty<double> ValueStartProperty = AvaloniaProperty.Register<TwoWayRangeBase, double>(
        nameof(ValueStart), 0.0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> ValueEndProperty = AvaloniaProperty.Register<TwoWayRangeBase, double>(
        nameof(ValueEnd), 0.0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> LargeChangeProperty = AvaloniaProperty.Register<TwoWayRangeBase, double>(
        nameof(LargeChange), 1.0);

    public static readonly StyledProperty<double> SmallChangeProperty = AvaloniaProperty.Register<TwoWayRangeBase, double>(
        nameof(SmallChange), 0.1);

    public static readonly RoutedEvent<FunctionEventArgs<DoubleRange>> ValueChangedEvent =
        RoutedEvent.Register<TwoWayRangeBase, FunctionEventArgs<DoubleRange>>(nameof(ValueChanged),
            RoutingStrategies.Bubble);

    static TwoWayRangeBase()
    {
        MinimumProperty.Changed.AddClassHandler<TwoWayRangeBase>((x, e) => x.OnMinimumChanged(e));
        MaximumProperty.Changed.AddClassHandler<TwoWayRangeBase>((x, e) => x.OnMaximumChanged(e));
        ValueStartProperty.Changed.AddClassHandler<TwoWayRangeBase>((x, e) => x.OnValueStartChanged(e));
        ValueEndProperty.Changed.AddClassHandler<TwoWayRangeBase>((x, e) => x.OnValueEndChanged(e));
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double ValueStart
    {
        get => GetValue(ValueStartProperty);
        set => SetValue(ValueStartProperty, value);
    }

    public double ValueEnd
    {
        get => GetValue(ValueEndProperty);
        set => SetValue(ValueEndProperty, value);
    }

    public double LargeChange
    {
        get => GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public event EventHandler<FunctionEventArgs<DoubleRange>>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    private void OnMinimumChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var min = (double) e.NewValue!;
        if ((double) GetValue(MaximumProperty) < min)
        {
            SetCurrentValue(MaximumProperty, min);
        }

        CoerceValue(ValueStartProperty);
        CoerceValue(ValueEndProperty);
    }

    private void OnMaximumChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var max = (double) e.NewValue!;
        if ((double) GetValue(MinimumProperty) > max)
        {
            SetCurrentValue(MinimumProperty, max);
        }

        CoerceValue(ValueStartProperty);
        CoerceValue(ValueEndProperty);
    }

    private void OnValueStartChanged(AvaloniaPropertyChangedEventArgs e)
    {
        CoerceValue(ValueStartProperty);
        RaiseValueChanged(new DoubleRange { Start = (double) e.OldValue!, End = (double) GetValue(ValueEndProperty) },
            new DoubleRange { Start = (double) e.NewValue!, End = (double) GetValue(ValueEndProperty) });
    }

    private void OnValueEndChanged(AvaloniaPropertyChangedEventArgs e)
    {
        CoerceValue(ValueEndProperty);
        RaiseValueChanged(new DoubleRange { Start = (double) GetValue(ValueStartProperty), End = (double) e.OldValue! },
            new DoubleRange { Start = (double) GetValue(ValueStartProperty), End = (double) e.NewValue! });
    }

    private void RaiseValueChanged(DoubleRange oldValue, DoubleRange newValue)
        => RaiseEvent(new FunctionEventArgs<DoubleRange>(ValueChangedEvent, this)
        {
            Info = newValue
        });
}