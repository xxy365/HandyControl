using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace HandyControl.Controls;

public class RectangleAttach
{
    public static readonly AttachedProperty<bool> CircularProperty =
        AvaloniaProperty.RegisterAttached<RectangleAttach, AvaloniaObject, bool>("Circular");

    public static void SetCircular(AvaloniaObject element, bool value) =>
        element.SetValue(CircularProperty, value);

    public static bool GetCircular(AvaloniaObject element) =>
        element.GetValue(CircularProperty);

    static RectangleAttach()
    {
        CircularProperty.Changed.AddClassHandler<AvaloniaObject>(OnCircularChanged);
    }

    private static void OnCircularChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not Rectangle rectangle)
        {
            return;
        }

        if ((bool)e.NewValue!)
        {
            rectangle.SizeChanged += Rectangle_SizeChanged;
            UpdateRadius(rectangle);
        }
        else
        {
            rectangle.SizeChanged -= Rectangle_SizeChanged;
        }
    }

    private static void Rectangle_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is Rectangle rectangle)
        {
            UpdateRadius(rectangle);
        }
    }

    private static void UpdateRadius(Rectangle rectangle)
    {
        var min = Math.Min(rectangle.Bounds.Width, rectangle.Bounds.Height);
        rectangle.SetCurrentValue(Rectangle.RadiusXProperty, min / 2);
        rectangle.SetCurrentValue(Rectangle.RadiusYProperty, min / 2);
    }
}