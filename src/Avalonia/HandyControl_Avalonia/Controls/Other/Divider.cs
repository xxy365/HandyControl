using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace HandyControl.Controls;

public class Divider : ContentControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Divider, Orientation>(nameof(Orientation));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly StyledProperty<IBrush?> LineStrokeProperty =
        AvaloniaProperty.Register<Divider, IBrush?>(nameof(LineStroke));

    public IBrush? LineStroke
    {
        get => GetValue(LineStrokeProperty);
        set => SetValue(LineStrokeProperty, value);
    }

    public static readonly StyledProperty<double> LineStrokeThicknessProperty =
        AvaloniaProperty.Register<Divider, double>(nameof(LineStrokeThickness), 1d);

    public double LineStrokeThickness
    {
        get => GetValue(LineStrokeThicknessProperty);
        set => SetValue(LineStrokeThicknessProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<double>> LineStrokeDashArrayProperty =
        AvaloniaProperty.Register<Divider, AvaloniaList<double>>(nameof(LineStrokeDashArray), new AvaloniaList<double>());

    public AvaloniaList<double> LineStrokeDashArray
    {
        get => GetValue(LineStrokeDashArrayProperty);
        set => SetValue(LineStrokeDashArrayProperty, value);
    }
}
